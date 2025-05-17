using Catalyst.Graphics;

namespace Catalyst.Tiles;

public struct Tile(TileType type, int spriteVariantIndex)
{
    public readonly TileType Type = type;
    public readonly int SpriteVariantIndex = spriteVariantIndex;
    public Sprite2D Sprite => Type.GetSprite(SpriteVariantIndex);
};


