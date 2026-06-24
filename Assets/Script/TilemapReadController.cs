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
    [SerializeField] private TileData plowableData;
    [SerializeField] private TileData nonPlowableData;
    [SerializeField] private int useRange = 1;                     // range interaksi dari player (dalam tile)
    [SerializeField] private float playerCenterOffset = 0.75f;

    [SerializeField] private CropsManager cropsManager;

    private Dictionary<TileBase, TileData> dataFromTiles;          // lookup cepat: tile → properti
    private SpriteRenderer highlightSprite;                        // border kuning di tile yg di-hover
    private Vector3Int lastHighlightPos;
    private bool isHighlighted = false;
    private HotbarManager hotbar;

    private void Start()
    {
        dataFromTiles = new Dictionary<TileBase, TileData>();

        if (tilemap == null)
        {
            Tilemap[] all = FindObjectsOfType<Tilemap>();
            foreach (Tilemap t in all)
            {
                if (t.name.Contains("Base"))
                {
                    tilemap = t;
                    break;
                }
            }
            if (tilemap == null && all.Length > 0)
                tilemap = all[0];
        }

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

        if (plowableData != null)
        {
            foreach (TileBase tile in plowableData.tiles)
                if (!dataFromTiles.ContainsKey(tile))
                    dataFromTiles.Add(tile, plowableData);
        }

        if (nonPlowableData != null)
        {
            foreach (TileBase tile in nonPlowableData.tiles)
                if (!dataFromTiles.ContainsKey(tile))
                    dataFromTiles.Add(tile, nonPlowableData);
        }

        GameObject hlObj = new GameObject("TileHighlight");        // bikin object highlight
        hlObj.transform.SetParent(transform);
        highlightSprite = hlObj.AddComponent<SpriteRenderer>();
        highlightSprite.sortingOrder = 10;
        highlightSprite.color = new Color(1, 1, 0, 0.5f);          // kuning transparan
        highlightSprite.sprite = CreateBorderSprite();
        highlightSprite.gameObject.SetActive(false);
    }

    // bikin sprite border kotak secara procedural pake Texture2D
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

        bool toolSelected = IsAnyToolSelected();
        bool seedSelected = IsSeedSelected();
        if (!toolSelected && !seedSelected)      // highlight cuma kalo pake tool/seed
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
        Vector3Int gridPos = tilemap.WorldToCell(worldPos);        // posisi mouse di grid

        Vector3Int playerGrid = GetPlayerGridPosition();
        int dx = Mathf.Abs(gridPos.x - playerGrid.x);
        int dy = Mathf.Abs(gridPos.y - playerGrid.y);
        bool inRange = dx <= useRange && dy <= useRange && hlMap.GetTile(gridPos) != null;

        if (seedSelected && !(cropsManager != null && cropsManager.Check(gridPos)))
            inRange = false;                                       // seed cuma bisa di tile yg udah di-cangkul

        if (isHighlighted && gridPos == lastHighlightPos && inRange) return; // gak berubah, skip

        if (isHighlighted)
        {
            highlightSprite.gameObject.SetActive(false);
            isHighlighted = false;
        }

        if (inRange)                                                // tampilkan highlight di tile ini
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
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
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
        if (tilemap == null) return false;
        if (!IsAnyToolSelected() && !IsSeedSelected() && !IsKapakSelected()) return false;
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
        if (tilemap == null || (!IsAnyToolSelected() && !IsSeedSelected() && !IsKapakSelected())) return false;
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
        if (tilemap == null || (!IsAnyToolSelected() && !IsSeedSelected() && !IsKapakSelected())) return false;
        Vector3Int gridPos = GetMouseGridPosition();
        Vector3Int playerGrid = GetPlayerGridPosition();
        Vector3Int d = gridPos - playerGrid;
        return d.x != 0 && d.y != 0 && Mathf.Abs(d.x) <= useRange && Mathf.Abs(d.y) <= useRange && tilemap.GetTile(gridPos) != null;
    }

    // konversi grid position ke world position di kaki player (bukan tengah tile)
    public Vector3 GridToWorldFeet(Vector3Int gridPos)
    {
        Vector3 center = tilemap.GetCellCenterWorld(gridPos);
        center.y -= playerCenterOffset;
        return center;
    }

    public Vector3 GridToWorldCenter(Vector3Int gridPos)
    {
        if (tilemap == null) return Vector3.zero;
        return tilemap.GetCellCenterWorld(gridPos);
    }

    public bool IsToolSelected()
    {
        return hotbar != null && hotbar.SelectedItem != null && hotbar.SelectedItem.itemName == "tools";
    }

    public bool IsWateringCanSelected()
    {
        return hotbar != null && hotbar.SelectedItem != null && hotbar.SelectedItem.itemName == "water can";
    }

    public bool IsKapakSelected()
    {
        return hotbar != null && hotbar.SelectedItem != null && hotbar.SelectedItem.itemName == "Kapak";
    }

    public bool IsSeedSelected()
    {
        return hotbar != null && hotbar.SelectedItem != null && hotbar.SelectedItem.isSeed;
    }

    private bool IsAnyToolSelected()
    {
        return hotbar != null && hotbar.SelectedItem != null &&
               (hotbar.SelectedItem.itemName == "tools" || hotbar.SelectedItem.itemName == "water can");
    }

    public bool CanPlowAt(Vector3Int gridPos)
    {
        if (tilemap == null || cropsManager == null) return false;
        if (cropsManager.Check(gridPos)) return false;              // udah di-cangkul
        TileData tileData = GetTileData(tilemap.GetTile(gridPos));
        return tileData == null || tileData.plowable;                // tile bisa di-cangkul
    }

    public bool CanWaterAt(Vector3Int gridPos)
    {
        if (tilemap == null || cropsManager == null) return false;
        if (!cropsManager.Check(gridPos)) return false;             // harus udah di-cangkul
        return !cropsManager.IsWatered(gridPos);                    // jangan siram 2x
    }

    public bool CanSeedAt(Vector3Int gridPos)
    {
        if (tilemap == null || cropsManager == null) return false;
        if (!cropsManager.Check(gridPos)) return false;
        if (cropsManager.IsSeeded(gridPos)) return false;
        if (cropsManager.IsFullyGrown(gridPos)) return false;
        Vector3Int playerGrid = GetPlayerGridPosition();
        int dx = Mathf.Abs(gridPos.x - playerGrid.x);
        int dy = Mathf.Abs(gridPos.y - playerGrid.y);
        return dx <= useRange && dy <= useRange;
    }

    public bool IsAlreadyPlowed(Vector3Int gridPos)
    {
        return cropsManager != null && cropsManager.Check(gridPos);
    }
}
