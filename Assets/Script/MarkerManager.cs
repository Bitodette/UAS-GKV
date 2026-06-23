using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MarkerManager : MonoBehaviour
{
    [SerializeField] private Tilemap targetTilemap;
    [SerializeField] private TileBase tile;

    public Vector3Int markedCellPosition;

    private Vector3Int oldCellPosition;
    private bool hasOldPosition = false;

    private void Update()
    {
        if (hasOldPosition && oldCellPosition != markedCellPosition)
        {
            targetTilemap.SetTile(oldCellPosition, null);
        }

        targetTilemap.SetTile(markedCellPosition, tile);

        oldCellPosition = markedCellPosition;
        hasOldPosition = true;
    }
}