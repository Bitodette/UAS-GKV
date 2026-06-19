using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public class TilemapReadController : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Tilemap highlightTilemap;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private List<TileData> tileDatas;
    [SerializeField] private int useRange = 1;
    [SerializeField] private float playerCenterOffset = 0.75f;

    [SerializeField] private CropsManager cropsManager;

    private Dictionary<TileBase, TileData> dataFromTiles;
    private SpriteRenderer highlightSprite;
    private Vector3Int lastHighlightPos;
    private bool isHighlighted = false;
    private HotbarManager hotbar;

    private void Start()
    {
        dataFromTiles = new Dictionary<TileBase, TileData>();

        if (tilemap == null)
            tilemap = FindFirstObjectByType<Tilemap>();

        if (cropsManager == null)
            cropsManager = FindFirstObjectByType<CropsManager>();

        hotbar = FindFirstObjectByType<HotbarManager>();

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
        if (tilemap == null) return;
        Tilemap hlMap = highlightTilemap != null ? highlightTilemap : tilemap;

        bool toolSelected = hotbar != null && hotbar.SelectedItem != null && hotbar.SelectedItem.itemName == "tools";
        if (!toolSelected)
        {
            if (isHighlighted)
            {
                highlightSprite.gameObject.SetActive(false);
                isHighlighted = false;
            }
            return;
        }

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;
        Vector3Int gridPos = tilemap.WorldToCell(worldPos);

        Vector3Int playerGrid = GetPlayerGridPosition();
        int dx = Mathf.Abs(gridPos.x - playerGrid.x);
        int dy = Mathf.Abs(gridPos.y - playerGrid.y);
        bool inRange = dx <= useRange && dy <= useRange && hlMap.GetTile(gridPos) != null;

        if (!IsPointerOverUI() && Input.GetMouseButtonDown(0))
        {
            Debug.Log($"Klik grid=({gridPos.x},{gridPos.y}) playerGrid=({playerGrid.x},{playerGrid.y}) dx={dx} dy={dy} range={useRange} adaTile={hlMap.GetTile(gridPos) != null} inRange={inRange}");

            if (inRange && cropsManager != null)
            {
                Tilemap readMap = groundTilemap != null ? groundTilemap : tilemap;
                TileBase tileBase = readMap.GetTile(gridPos);
                TileData tileData = GetTileData(tileBase);
                bool isSeeded = cropsManager.Check(gridPos);
                Debug.Log($"→ AKAN di-{(isSeeded ? "SEED" : "PLOW")} di ({gridPos.x},{gridPos.y})");
                if (isSeeded)
                    cropsManager.Seed(gridPos);
                else if (tileData != null && tileData.plowable)
                    cropsManager.Plow(gridPos);
            }
            else
            {
                Debug.Log($"→ TIDAK action. inRange={inRange} cropsManager={cropsManager != null}");
            }
        }

        if (isHighlighted && gridPos == lastHighlightPos && inRange) return;

        if (isHighlighted)
        {
            highlightSprite.gameObject.SetActive(false);
            isHighlighted = false;
        }

        if (inRange)
        {
            Vector3 tileCenter = hlMap.GetCellCenterWorld(gridPos);
            highlightSprite.transform.position = tileCenter;
            highlightSprite.gameObject.SetActive(true);
            lastHighlightPos = gridPos;
            isHighlighted = true;
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject();
    }

    private Vector2 GetPlayerPosition()
    {
        if (GameManager.Instance != null && GameManager.Instance.player != null)
            return (Vector2)GameManager.Instance.player.transform.position + Vector2.up * playerCenterOffset;

        return Vector2.zero;
    }

    public Vector3Int GetPlayerGridPosition()
    {
        if (tilemap == null) return Vector3Int.zero;
        return tilemap.WorldToCell(GetPlayerPosition());
    }

    public TileData GetTileData(TileBase tileBase)
    {
        if (tileBase == null) return null;
        dataFromTiles.TryGetValue(tileBase, out TileData data);
        return data;
    }

    public bool IsMouseOverInRangeTile()
    {
        if (tilemap == null || !IsToolSelected()) return false;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;
        Vector3Int gridPos = tilemap.WorldToCell(worldPos);
        Vector3Int playerGrid = GetPlayerGridPosition();
        int dx = Mathf.Abs(gridPos.x - playerGrid.x);
        int dy = Mathf.Abs(gridPos.y - playerGrid.y);
        return dx <= useRange && dy <= useRange && tilemap.GetTile(gridPos) != null;
    }

    public bool IsMouseOverPlayerTile()
    {
        if (tilemap == null || !IsToolSelected()) return false;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;
        Vector3Int gridPos = tilemap.WorldToCell(worldPos);
        return gridPos == GetPlayerGridPosition();
    }

    public Vector3Int GetMouseGridPosition()
    {
        if (tilemap == null) return Vector3Int.zero;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;
        return tilemap.WorldToCell(worldPos);
    }

    public bool IsMouseOverDiagonalTile()
    {
        if (tilemap == null || !IsToolSelected()) return false;
        Vector3Int gridPos = GetMouseGridPosition();
        Vector3Int playerGrid = GetPlayerGridPosition();
        Vector3Int d = gridPos - playerGrid;
        return d.x != 0 && d.y != 0 && Mathf.Abs(d.x) <= useRange && Mathf.Abs(d.y) <= useRange && tilemap.GetTile(gridPos) != null;
    }

    public Vector3 GridToWorldFeet(Vector3Int gridPos)
    {
        Vector3 center = tilemap.GetCellCenterWorld(gridPos);
        center.y -= playerCenterOffset;
        return center;
    }

    public bool IsToolSelected()
    {
        return hotbar != null && hotbar.SelectedItem != null && hotbar.SelectedItem.itemName == "tools";
    }
}
