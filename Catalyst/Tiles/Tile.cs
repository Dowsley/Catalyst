using Catalyst.Graphics;

namespace Catalyst.Tiles;

public struct Tile(TileType type, int spriteVariantIndex)
{
    public TileType Type = type;
    public int SpriteVariantIndex = spriteVariantIndex;
    public Sprite2D Sprite => Type.GetSprite(SpriteVariantIndex);
};


