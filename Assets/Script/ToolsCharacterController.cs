using UnityEngine;
using UnityEngine.Tilemaps;
public class ToolsCharacterController : MonoBehaviour
{
    Rigidbody2D rb;

    [SerializeField] float offsetDistance = 1f;
    [SerializeField] float sizeOfInteractableArea = 1.2f;
    [SerializeField] float maxUseDistance = 1.5f;
    [SerializeField] TilemapReadController tilemapReadController;

    [SerializeField] CropsManager cropsManager;
    [SerializeField] TileData plowableTiles;

    Camera mainCam;

    private bool selectable;
    private Vector3Int selectedTilePosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;

        if (tilemapReadController == null)
            tilemapReadController = FindFirstObjectByType<TilemapReadController>();
        if (cropsManager == null)
            cropsManager = FindFirstObjectByType<CropsManager>();
    }

    void Update()
    {
        SelectTile();
        CanSelectCheck();

        if (Input.GetMouseButtonDown(0))
        {
            if (!UseToolWorld())
            {
                UseToolGrid();
            }
        }
    }

    private void SelectTile()
    {
        if (tilemapReadController == null) return;
        selectedTilePosition =
            tilemapReadController.GetGridPosition(
                Input.mousePosition,
                true
            );
    }

    private void CanSelectCheck()
    {
        Vector2 characterPosition = transform.position;
        Vector2 mousePosition =
            mainCam.ScreenToWorldPoint(Input.mousePosition);

        selectable =
            Vector2.Distance(
                characterPosition,
                mousePosition
            ) <= maxUseDistance;
    }

    private bool UseToolWorld()
    {
        Vector3 mousePosition =
            mainCam.ScreenToWorldPoint(Input.mousePosition);

        mousePosition.z = 0;

        float dist =
            Vector2.Distance(mousePosition, transform.position);

        if (dist > maxUseDistance)
            return false;

        HotbarManager hotbar =
            FindFirstObjectByType<HotbarManager>();

        if (hotbar != null && hotbar.SelectedItem != null)
        {
            Debug.Log(
                "Using item: " +
                hotbar.SelectedItem.itemName
            );
        }

        Vector2 aimDirection =
            mousePosition - transform.position;

        Vector2 direction;

        if (Mathf.Abs(aimDirection.x) > Mathf.Abs(aimDirection.y))
            direction = new Vector2(Mathf.Sign(aimDirection.x), 0);
        else
            direction = new Vector2(0, Mathf.Sign(aimDirection.y));

        Vector2 position =
            rb.position + offsetDistance * direction;

        Collider2D[] colliders =
            Physics2D.OverlapCircleAll(
                position,
                sizeOfInteractableArea
            );

        foreach (Collider2D collider in colliders)
        {
            ToolHit hit =
                collider.GetComponent<ToolHit>();

            if (hit != null)
            {
                hit.Hit();
                return true;
            }
        }

        return false;
    }

    private void UseToolGrid()
    {
        if (!selectable || tilemapReadController == null || cropsManager == null) return;

        TileBase tileBase = tilemapReadController.GetTileBase(selectedTilePosition);
        TileData tileData = tilemapReadController.GetTileData(tileBase);

        if (cropsManager.Check(selectedTilePosition))
        {
            cropsManager.Seed(selectedTilePosition);
        }
        else if (tileData != null && tileData.plowable)
        {
            cropsManager.Plow(selectedTilePosition);
        }
    }
}