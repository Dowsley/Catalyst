using Catalyst.Core.WorldGeneration.Masks;
using Catalyst.Tiles;

namespace Catalyst.Core.WorldGeneration.Passes;

public class DebugPass : Pass
{
    public DebugPass(World world, int seed) : base(world, seed + 1)
    {
        PassMask = new LayerMask(world.WorldSize, ["space", "surface"], allowList: false);
    }

    protected override Tile? GetTileTransformation(int x, int y)
    {
        var type = TileRegistry.Get("STONE");
        return new Tile(type, type.GetRandomSpriteIndex(Random));
    }
}