using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// data class tiap tanaman (bukan MonoBehaviour — cuma wadah data)
public class Crops
{
    public bool isWatered;           // sudah di-siram
    public bool seeded;              // sudah ditanam
    public int growthStage;          // tahap pertumbuhan (0 = baru ditanam, max = siap panen)
}

public class CropsManager : MonoBehaviour
{
    [SerializeField] private TileBase plowed;
    [SerializeField] private Sprite plowedSprite;
    [SerializeField] private TileBase seeded;
    [SerializeField] private TileBase watered;
    [SerializeField] private Tilemap targetTilemap;       // tilemap tanah
    [SerializeField] private Tilemap seedTilemap;         // tilemap tanaman (sorting order rendah)
    [SerializeField] private Tilemap wateredTilemap;      // tilemap air
    [SerializeField] private Tilemap overlayTilemap;      // tilemap overlay (sorting order tinggi)
    [SerializeField] private TileBase[] growthTiles;      // tile tiap tahap tumbuh
    [SerializeField] private Transform playerRef;
    [SerializeField] private float playerCenterOffset = 0f;

    private Dictionary<Vector3Int, Crops> crops;          // semua tanaman yg ada
    private TileBase plowedTile;

    private void Start()
    {
        crops = new Dictionary<Vector3Int, Crops>();

        if (plowed != null)
            plowedTile = plowed;
        else if (plowedSprite != null)
            plowedTile = CreateTileFromSprite(plowedSprite);  // bikin tile dari sprite procedural

        if (playerRef == null && GameManager.Instance != null && GameManager.Instance.player != null)
            playerRef = GameManager.Instance.player.transform;
        if (playerRef == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null) playerRef = playerGO.transform;
        }
    }

    private void Update()
    {
        if (overlayTilemap == null) return;
        if (playerRef == null) return;

        Tilemap srcMap = seedTilemap ?? targetTilemap;
        if (srcMap == null) return;

        Vector3Int playerGrid = srcMap.WorldToCell(playerRef.position + Vector3.up * playerCenterOffset);
        int moved = 0;

        foreach (var kvp in crops)
        {
            if (!kvp.Value.seeded) continue;

            Vector3Int pos = kvp.Key;
            int dx = Mathf.Abs(pos.x - playerGrid.x);
            int dy = pos.y - playerGrid.y;                // BUKAN Mathf.Abs! — negatif = di depan player

            TileBase tile = srcMap.GetTile(pos);
            if (tile == null) tile = overlayTilemap.GetTile(pos);
            if (tile == null) continue;

            bool thisIsLargeCrop = false;
            if (growthTiles != null)
            {
                for (int i = 1; i < growthTiles.Length; i++)
                {
                    if (tile == growthTiles[i])
                    {
                        thisIsLargeCrop = true;
                        break;
                    }
                }
            }

            // dy < 0 = crop di bawah player (lebih depan secara visual)
            bool cropInFront = dy < 0 || (dy == 0 && dx <= 1);
            if (thisIsLargeCrop && dy == 1 && dx <= 1)
                cropInFront = true;

            if (cropInFront)
            {
                // pindahin ke overlay tilemap (sorting order tinggi) biar di depan player
                if (overlayTilemap.GetTile(pos) != tile)
                {
                    srcMap.SetTile(pos, null);
                    overlayTilemap.SetTile(pos, tile);
                    moved++;
                }
            }
            else
            {
                // pindahin balik ke seed tilemap (sorting order rendah) biar di belakang player
                if (overlayTilemap.GetTile(pos) != null)
                {
                    overlayTilemap.SetTile(pos, null);
                    srcMap.SetTile(pos, tile);
                    moved++;
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

    public Dictionary<Vector3Int, Crops> CropsData => crops;

    public void ClearAllCrops()
    {
        foreach (var pos in crops.Keys)
        {
            if (targetTilemap != null) targetTilemap.SetTile(pos, null);
            if (seedTilemap != null) seedTilemap.SetTile(pos, null);
            if (wateredTilemap != null) wateredTilemap.SetTile(pos, null);
            if (overlayTilemap != null) overlayTilemap.SetTile(pos, null);
        }
        crops.Clear();
    }

    public void RestoreCrop(Vector3Int pos, Crops crop)   // restore dari save data
    {
        crops[pos] = crop;

        if (plowedTile != null && targetTilemap != null)
            targetTilemap.SetTile(pos, plowedTile);

        if (crop.isWatered && watered != null && wateredTilemap != null)
            wateredTilemap.SetTile(pos, watered);

        if (crop.seeded)
        {
            if (crop.growthStage == 0)
            {
                if (seeded != null)
                    SetCropTile(pos, seeded);
            }
            else if (growthTiles != null && crop.growthStage - 1 < growthTiles.Length)
            {
                SetCropTile(pos, growthTiles[crop.growthStage - 1]);
            }
        }
    }

    public bool Check(Vector3Int position)
    {
        return crops.ContainsKey(position);                 // apakah tile ini sudah di-cangkul?
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

    public void Plow(Vector3Int position)                    // cangkul tanah
    {
        if (crops.ContainsKey(position))
            return;

        CreatePlowedTile(position);
    }

    public void Water(Vector3Int position)                   // siram tanaman
    {
        if (!crops.TryGetValue(position, out Crops crop))
        {
            crop = new Crops();
            crops.Add(position, crop);
        }

        crop.isWatered = true;

        if (watered != null && wateredTilemap != null)
        {
            wateredTilemap.SetTile(position, watered);
        }
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

    public void Seed(Vector3Int position)                    // tanam benih
    {
        if (!crops.ContainsKey(position)) return;
        if (crops[position].seeded) return;

        crops[position].seeded = true;
        crops[position].growthStage = 0;

        SetCropTile(position, seeded);
    }

    public void GrowAll()                                    // tumbuhkan semua tanaman (panggil tiap hari baru)
    {
        foreach (var kvp in crops)
        {
            if (!kvp.Value.seeded) continue;
            if (kvp.Value.growthStage >= growthTiles.Length) continue;
            if (!kvp.Value.isWatered) continue;              // cuma tanaman yg di-siram yg tumbuh

            kvp.Value.isWatered = false;
            if (wateredTilemap != null)
                wateredTilemap.SetTile(kvp.Key, null);

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

    public bool Harvest(Vector3Int position)                 // panen
    {
        if (!IsFullyGrown(position)) return false;

        crops[position].seeded = false;
        crops[position].growthStage = 0;

        if (seedTilemap != null) seedTilemap.SetTile(position, null);
        if (overlayTilemap != null) overlayTilemap.SetTile(position, null);
        if (targetTilemap != null && plowedTile != null)
            targetTilemap.SetTile(position, plowedTile);     // balikin ke tanah cangkul

        return true;
    }

    private void CreatePlowedTile(Vector3Int position)
    {
        Crops crop = new Crops();
        crops.Add(position, crop);
        if (plowedTile != null && targetTilemap != null)
            targetTilemap.SetTile(position, plowedTile);
    }
}
