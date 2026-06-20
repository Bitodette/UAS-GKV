using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Crops
{
    public bool seeded;
}

public class CropsManager : MonoBehaviour
{
    [SerializeField] private TileBase plowed;
    [SerializeField] private Sprite plowedSprite;
    [SerializeField] private TileBase seeded;
    [SerializeField] private Tilemap targetTilemap;
    [SerializeField] private Tilemap seedTilemap;

    private Dictionary<Vector3Int, Crops> crops;
    private TileBase plowedTile;

    private void Start()
    {
        crops = new Dictionary<Vector3Int, Crops>();

        if (plowed != null)
            plowedTile = plowed;
        else if (plowedSprite != null)
            plowedTile = CreateTileFromSprite(plowedSprite);
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

    public void Seed(Vector3Int position)
    {
        if (!crops.ContainsKey(position))
            return;

        if (crops[position].seeded)
            return;

        crops[position].seeded = true;

        if (seedTilemap != null)
            seedTilemap.SetTile(position, seeded);
        else
            targetTilemap.SetTile(position, seeded);
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