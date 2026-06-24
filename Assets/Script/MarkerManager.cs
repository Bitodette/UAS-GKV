using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// menandai posisi grid tertentu dengan tile marker (highlighter)
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
            targetTilemap.SetTile(oldCellPosition, null);   // hapus marker lama
        }

        targetTilemap.SetTile(markedCellPosition, tile);    // pasang marker baru

        oldCellPosition = markedCellPosition;
        hasOldPosition = true;
    }
}
