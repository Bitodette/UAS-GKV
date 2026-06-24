using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileData", menuName = "ScriptableObjects/TileData")]
public class TileData : ScriptableObject
{
    public List<TileBase> tiles;       // daftar tile yang punya properti ini
    public bool plowable;              // true kalo tile ini bisa di-cangkul
}
