using Catalyst.Graphics;

namespace Catalyst.Tiles;

public struct Tile(TileType type, TileType wallType, int spriteVariantIndex)
{
    // TODO: Rework this to use ushort pointing to IDs
    // And consider having id 0 point to a null entry (TileType) 
    public TileType Type = type;
    public TileType WallType = wallType;
    public int SpriteVariantIndex = spriteVariantIndex;
    public Sprite Sprite => Type.GetSprite(SpriteVariantIndex);
    public Sprite WallSprite => WallType.GetSprite(SpriteVariantIndex);
};
