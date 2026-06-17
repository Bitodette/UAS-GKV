using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapReadController : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GetTileBase(Input.mousePosition);
        } 
    }
    public TileBase GetTileBase(Vector2 mousePosition)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        
        Vector3Int gridPosition = tilemap.WorldToCell(worldPosition);
        
        TileBase tileBase = tilemap.GetTile(gridPosition);

        Debug.Log("Tile in position = " + gridPosition + " is " + tileBase);
        return tileBase;
    }
}
