using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Anim Tile", menuName = "Tiles/Anim Tile")]
public class AnimTile : TileBase
{
    public Sprite previewSprite;
    public Sprite[] sprites;
    public float speed = 1f;

    public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
    {
        if (sprites == null || sprites.Length == 0) return false;

        tileAnimationData.animatedSprites = sprites;
        tileAnimationData.animationSpeed = speed;
        tileAnimationData.animationStartTime = 0;
        return true;
    }
}
