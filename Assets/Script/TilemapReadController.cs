using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public class TilemapReadController : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private List<TileData> tileDatas;
    [SerializeField] private int useRange = 1;

    private Dictionary<TileBase, TileData> dataFromTiles;
    private SpriteRenderer highlightSprite;
    private Vector3Int lastHighlightPos;
    private bool isHighlighted = false;

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

        GameObject hlObj = new GameObject("TileHighlight");
        hlObj.transform.SetParent(transform);
        highlightSprite = hlObj.AddComponent<SpriteRenderer>();
        highlightSprite.sortingOrder = 10;
        highlightSprite.color = new Color(1, 1, 0, 0.5f);
        highlightSprite.sprite = CreateBorderSprite();
        highlightSprite.gameObject.SetActive(false);
    }

    private Sprite CreateBorderSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        Color clear = Color.clear;
        Color yellow = Color.yellow;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                bool border = x == 0 || x == size - 1 || y == 0 || y == size - 1;
                tex.SetPixel(x, y, border ? yellow : clear);
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private void Update()
    {
        UpdateHighlight();

        if (IsPointerOverUI()) return;

        if (Input.GetMouseButtonDown(0))
        {
            TryUseTile(Input.mousePosition);
        }
    }

    private void UpdateHighlight()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;
        Vector3Int gridPos = tilemap.WorldToCell(worldPos);
        Vector3Int playerGrid = tilemap.WorldToCell(GetPlayerPosition());
        int dx = Mathf.Abs(gridPos.x - playerGrid.x);
        int dy = Mathf.Abs(gridPos.y - playerGrid.y);
        bool inRange = dx <= useRange && dy <= useRange && tilemap.GetTile(gridPos) != null;

        if (isHighlighted && gridPos == lastHighlightPos && inRange) return;

        if (isHighlighted)
        {
            highlightSprite.gameObject.SetActive(false);
            isHighlighted = false;
        }

        if (inRange)
        {
            Vector3 tileCenter = tilemap.GetCellCenterWorld(gridPos);
            highlightSprite.transform.position = tileCenter;
            highlightSprite.gameObject.SetActive(true);
            lastHighlightPos = gridPos;
            isHighlighted = true;
        }
    }

    private void TryUseTile(Vector2 mousePosition)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0;

        Vector3Int gridPosition = tilemap.WorldToCell(worldPosition);
        TileBase tileBase = tilemap.GetTile(gridPosition);

        if (tileBase == null) return;

        Vector3Int playerGridPos = tilemap.WorldToCell(GetPlayerPosition());
        int dx = Mathf.Abs(gridPosition.x - playerGridPos.x);
        int dy = Mathf.Abs(gridPosition.y - playerGridPos.y);
        if (dx > useRange || dy > useRange) return;

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