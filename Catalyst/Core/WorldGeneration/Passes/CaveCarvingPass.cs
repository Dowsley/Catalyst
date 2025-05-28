using Catalyst.Core.WorldGeneration.Masks;
using Catalyst.Tiles;

namespace Catalyst.Core.WorldGeneration.Passes;

public class CaveCarvingPass : Pass
{
    private const float BaseThreshold = -0.7f;
    private const float MaskInfluenceOnThreshold = 0.2f;

    public CaveCarvingPass(World world) : base(world)
    {
        Noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        Noise.SetFrequency(0.01f);
        Noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        Noise.SetFractalOctaves(6);
        Noise.SetFractalLacunarity(1.27f);
        Noise.SetFractalGain(1.1f);
        Noise.SetFractalWeightedStrength(-0.5f);

        PassMasks.Clear();
        PassMasks.Add(new LayerMask(
            world.WorldSize, 
            ["space", "surface"], 
            allowList: false,
            boundaryNoiseSeed: WorldGenRNG.GenSeed()
            ));
    }

    protected override Tile? GetTileTransformation(int x, int y, float maskValue)
    {
        // When maskValue = 0, threshold = BaseThreshold (e.g., -0.7f, harder to carve).
        // When maskValue = 1, threshold = BaseThreshold + MaskInfluenceOnThreshold (e.g., -0.7f + 0.2f = -0.5f, original ease).
        float dynamicThreshold = BaseThreshold + (maskValue * MaskInfluenceOnThreshold);

        if (Noise.GetNoise(x, y) <= dynamicThreshold) 
        {
            return CreateEmptyTile(TileRegistry.Get("DIRT"));
        }

        return null;
    }
}