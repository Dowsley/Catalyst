using Catalyst.Core.WorldGeneration.Masks;
using Catalyst.Tiles;

namespace Catalyst.Core.WorldGeneration.Passes;

public class SmallCaveCarvingPass : Pass
{
    private const int BoundaryNoiseSeedOffset = 200;

    public SmallCaveCarvingPass(World world, int seed) : base(world, seed + 1)
    {
        Noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        Noise.SetFrequency(0.015f);
        Noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        Noise.SetFractalOctaves(6);
        Noise.SetFractalLacunarity(1.4f);
        Noise.SetFractalGain(0.91f);
        Noise.SetFractalWeightedStrength(-0.43f);

        PassMasks.Clear();
        PassMasks.Add(new LayerMask(
            world.WorldSize, 
            ["space"], 
            allowList: false, 
            boundaryNoiseSeed: seed + BoundaryNoiseSeedOffset
            ));
    }

    protected override Tile? GetTileTransformation(int x, int y, float maskValue)
    {
        if (Noise.GetNoise(x, y) > 0.5f) 
        {
            return CreateEmptyTile();
        }
        
        return null;
    }
}