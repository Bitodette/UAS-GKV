using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapReadController : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] float useRange = 3f;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI()) return;
            TryUseTile(Input.mousePosition);
        } 
    }

    private bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current != null
            && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    private void TryUseTile(Vector2 mousePosition)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0;
        Vector3Int gridPosition = tilemap.WorldToCell(worldPosition);
        TileBase tileBase = tilemap.GetTile(gridPosition);

        if (tileBase == null) return;

        float dist = Vector2.Distance(worldPosition, GetPlayerPosition());
        if (dist > useRange) return;

        Debug.Log("Tile in position = " + gridPosition + " is " + tileBase);
    }

    private Vector2 GetPlayerPosition()
    {
        if (GameManager.Instance != null && GameManager.Instance.player != null)
            return GameManager.Instance.player.transform.position;
        return Vector2.zero;
    }

    public TileBase GetTileBase(Vector2 mousePosition)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0;
        Vector3Int gridPosition = tilemap.WorldToCell(worldPosition);
        return tilemap.GetTile(gridPosition);
    }
}
