using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class ToolsCharacterController : MonoBehaviour
{
    Rigidbody2D rb;

    [SerializeField] float offsetDistance = 1f;
    [SerializeField] float sizeOfInteractableArea = 1.2f;
    [SerializeField] float maxUseDistance = 1.5f;

    [SerializeField] private Tilemap gridTilemap;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private List<TileData> tileDatas;
    [SerializeField] CropsManager cropsManager;

    private Dictionary<TileBase, TileData> dataFromTiles;

    Camera mainCam;

    private bool selectable;
    private Vector3Int selectedTilePosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;

        if (gridTilemap == null)
            gridTilemap = FindFirstObjectByType<Tilemap>();
        if (groundTilemap == null)
            groundTilemap = gridTilemap;
        if (cropsManager == null)
            cropsManager = FindFirstObjectByType<CropsManager>();

        dataFromTiles = new Dictionary<TileBase, TileData>();
        foreach (TileData tileData in tileDatas)
        {
            foreach (TileBase tile in tileData.tiles)
            {
                if (!dataFromTiles.ContainsKey(tile))
                    dataFromTiles.Add(tile, tileData);
            }
        }
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
        if (gridTilemap == null) return;
        Vector3 worldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;
        selectedTilePosition = gridTilemap.WorldToCell(worldPos);
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
        if (!selectable || cropsManager == null || groundTilemap == null) return;

        TileBase tileBase = groundTilemap.GetTile(selectedTilePosition);
        TileData tileData = GetTileData(tileBase);

        if (cropsManager.Check(selectedTilePosition))
        {
            cropsManager.Seed(selectedTilePosition);
        }
        else if (tileData != null && tileData.plowable)
        {
            cropsManager.Plow(selectedTilePosition);
        }
    }

    private TileData GetTileData(TileBase tileBase)
    {
        if (tileBase == null) return null;
        dataFromTiles.TryGetValue(tileBase, out TileData data);
        return data;
    }
}