using Catalyst.Core.WorldGeneration.Masks;
using Catalyst.Tiles;

namespace Catalyst.Core.WorldGeneration.Passes;

public class CaveCarvingPass : Pass
{
    public CaveCarvingPass(World world, int seed) : base(world, seed + 1)
    {
        Noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        Noise.SetFrequency(0.07f);
        Noise.SetSeed(Seed);

        PassMask = new LayerMask(world.WorldSize, ["space", "surface"], allowList: false);
    }

    protected override Tile? GetTileTransformation(int x, int y)
    {
        if (Noise.GetNoise(x, y) > 0.25f)
        {
            return null;
        }

        var type = TileRegistry.Get("EMPTY");
        return new Tile(type, type.GetRandomSpriteIndex(Random));
    }
}