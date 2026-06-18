using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public class TilemapReadController : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private List<TileData> tileDatas;
    [SerializeField] private float useRange = 3f;

    private Dictionary<TileBase, TileData> dataFromTiles;

    private void Start()
    {
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

    private void Update()
    {
        if (IsPointerOverUI()) return;

        if (Input.GetMouseButtonDown(0))
        {
            TryUseTile(Input.mousePosition);
        }
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

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject();
    }

    private Vector2 GetPlayerPosition()
    {
        if (GameManager.Instance != null && GameManager.Instance.player != null)
            return GameManager.Instance.player.transform.position;

        return Vector2.zero;
    }

    public Vector3Int GetGridPosition(Vector2 position, bool mousePosition = false)
    {
        Vector3 worldPosition = mousePosition
            ? Camera.main.ScreenToWorldPoint(position)
            : (Vector3)position;

        return tilemap.WorldToCell(worldPosition);
    }

    public TileBase GetTileBase(Vector3Int gridPosition)
    {
        TileBase tileBase = tilemap.GetTile(gridPosition);

        Debug.Log("Tile in position = " + gridPosition + " is " + tileBase);
        return tileBase;
    }

    public TileBase GetTileBase(Vector2 mousePosition)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0;

        Vector3Int gridPosition = tilemap.WorldToCell(worldPosition);
        return tilemap.GetTile(gridPosition);
    }

    public TileData GetTileData(TileBase tileBase)
    {
        if (tileBase == null) return null;

        dataFromTiles.TryGetValue(tileBase, out TileData data);
        return data;
    }
}