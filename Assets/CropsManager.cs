using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Crops
{
    public bool isWatered;
    public bool seeded;
    public int growthStage;
}

public class CropsManager : MonoBehaviour
{
    [SerializeField] private TileBase plowed;
    [SerializeField] private Sprite plowedSprite;
    [SerializeField] private TileBase seeded;
    [SerializeField] private TileBase watered;
    [SerializeField] private Tilemap targetTilemap;
    [SerializeField] private Tilemap seedTilemap;
    [SerializeField] private Tilemap wateredTilemap;
    [SerializeField] private Tilemap overlayTilemap;
    [SerializeField] private TileBase[] growthTiles;

    private Dictionary<Vector3Int, Crops> crops;
    private TileBase plowedTile;
    private Transform playerRef;
    private float lastPlayerY;

    private void Start()
    {
        crops = new Dictionary<Vector3Int, Crops>();

        if (plowed != null)
            plowedTile = plowed;
        else if (plowedSprite != null)
            plowedTile = CreateTileFromSprite(plowedSprite);

        if (playerRef == null && GameManager.Instance != null && GameManager.Instance.player != null)
            playerRef = GameManager.Instance.player.transform;
    }

    private void Update()
    {
        if (overlayTilemap == null || playerRef == null) return;

        float playerY = playerRef.position.y;
        if (Mathf.Abs(playerY - lastPlayerY) < 0.01f) return;
        lastPlayerY = playerY;

        foreach (var kvp in crops)
        {
            if (!kvp.Value.seeded) continue;

            Vector3Int pos = kvp.Key;
            Tilemap srcMap = seedTilemap ?? targetTilemap;
            if (srcMap == null) continue;

            Vector3 worldPos = srcMap.GetCellCenterWorld(pos);
            bool belowOrSame = worldPos.y <= playerY;

            TileBase tile = srcMap.GetTile(pos);
            if (tile == null) continue;

            if (belowOrSame)
            {
                if (overlayTilemap.GetTile(pos) != tile)
                {
                    if (seedTilemap != null) seedTilemap.SetTile(pos, null);
                    overlayTilemap.SetTile(pos, tile);
                }
            }
            else
            {
                if (overlayTilemap.GetTile(pos) != null)
                {
                    overlayTilemap.SetTile(pos, null);
                    if (seedTilemap != null) seedTilemap.SetTile(pos, tile);
                }
            }
        }
    }

    private TileBase CreateTileFromSprite(Sprite sprite)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.color = Color.white;
        tile.transform = Matrix4x4.identity;
        tile.flags = TileFlags.None;
        return tile;
    }

    public bool Check(Vector3Int position)
    {
        return crops.ContainsKey(position);
    }

    public bool IsWatered(Vector3Int position)
    {
        return crops.TryGetValue(position, out Crops crop) && crop.isWatered;
    }

    public bool IsSeeded(Vector3Int position)
    {
        if (crops.TryGetValue(position, out Crops crop))
            return crop.seeded;
        return false;
    }

    public void Plow(Vector3Int position)
    {
        if (crops.ContainsKey(position))
            return;

        CreatePlowedTile(position);
    }

    public void Water(Vector3Int position)
    {
        if (!crops.TryGetValue(position, out Crops crop))
        {
            crop = new Crops();
            crops.Add(position, crop);
        }

        crop.isWatered = true;

        if (watered == null)
            Debug.LogError("Watered tile (TileBase) is not assigned in CropsManager!");
        if (wateredTilemap == null)
            Debug.LogError("Watered Tilemap is not assigned in CropsManager!");

        if (watered != null && wateredTilemap != null)
        {
            wateredTilemap.SetTile(position, watered);
            Debug.Log($"Watered tile set at {position} on wateredTilemap");
        }

        Debug.Log($"Watered tile at {position}");
    }

    private void SetCropTile(Vector3Int position, TileBase tile)
    {
        if (seedTilemap != null)
            seedTilemap.SetTile(position, null);
        if (overlayTilemap != null)
            overlayTilemap.SetTile(position, null);

        Tilemap map = seedTilemap ?? targetTilemap;
        if (map != null)
            map.SetTile(position, tile);
    }

    public void Seed(Vector3Int position)
    {
        if (!crops.ContainsKey(position))
            return;

        if (crops[position].seeded)
            return;

        crops[position].seeded = true;
        crops[position].growthStage = 0;

        SetCropTile(position, seeded);
    }

    public void GrowAll()
    {
        foreach (var kvp in crops)
        {
            if (kvp.Value.isWatered)
            {
                kvp.Value.isWatered = false;
                if (wateredTilemap != null)
                    wateredTilemap.SetTile(kvp.Key, null);
            }

            if (!kvp.Value.seeded) continue;
            if (kvp.Value.growthStage >= growthTiles.Length) continue;

            SetCropTile(kvp.Key, growthTiles[kvp.Value.growthStage]);

            kvp.Value.growthStage++;
        }
    }

    public bool IsFullyGrown(Vector3Int position)
    {
        if (!crops.ContainsKey(position)) return false;
        if (!crops[position].seeded) return false;
        return crops[position].growthStage >= growthTiles.Length;
    }

    public bool Harvest(Vector3Int position)
    {
        if (!IsFullyGrown(position)) return false;

        crops[position].seeded = false;
        crops[position].growthStage = 0;

        if (seedTilemap != null)
            seedTilemap.SetTile(position, null);
        if (overlayTilemap != null)
            overlayTilemap.SetTile(position, null);
        if (targetTilemap != null && plowedTile != null)
            targetTilemap.SetTile(position, plowedTile);

        return true;
    }

    private void CreatePlowedTile(Vector3Int position)
    {
        Crops crop = new Crops();

        crops.Add(position, crop);
        Debug.Log($"CreatePlowedTile: plowedTile={(plowedTile != null ? plowedTile.name : "NULL")} targetTilemap={targetTilemap != null}");
        if (plowedTile != null && targetTilemap != null)
            targetTilemap.SetTile(position, plowedTile);
    }
}