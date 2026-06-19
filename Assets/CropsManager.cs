using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Crops
{
}

public class CropsManager : MonoBehaviour
{
    [SerializeField] private TileBase plowed;
    [SerializeField] private TileBase seeded;
    [SerializeField] private Tilemap targetTilemap;

    private Dictionary<Vector3Int, Crops> crops;

    private void Start()
    {
        crops = new Dictionary<Vector3Int, Crops>();
    }

    public bool Check(Vector3Int position)
    {
        return crops.ContainsKey(position);
    }

    public void Plow(Vector3Int position)
    {
        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
        Debug.Log($"Plow DIPANGGIL pos=({position.x},{position.y})\n" + st.ToString());

        if (crops.ContainsKey(position))
            return;

        CreatePlowedTile(position);
    }

    public void Seed(Vector3Int position)
    {
        targetTilemap.SetTile(position, seeded);
    }

    private void CreatePlowedTile(Vector3Int position)
    {
        Crops crop = new Crops();

        crops.Add(position, crop);
        targetTilemap.SetTile(position, plowed);
    }
}