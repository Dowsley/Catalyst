using Catalyst.Core.WorldGeneration.Masks;
using Catalyst.Tiles;

namespace Catalyst.Core.WorldGeneration.Passes;

public class BedrockLayerPass : Pass
{
    private const float BaseThreshold = -0.7f;
    private const float MaskInfluenceOnThreshold = 0.2f;
    private const int BoundaryNoiseSeedOffset = 123;

    public BedrockLayerPass(World world) : base(world)
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
            ["underworld"],
            allowList: true,
            boundaryNoiseSeed: WorldGenRNG.GenSeed(),
            startBoundaryNoiseAmplitude: 0.0025f,
            endBoundaryNoiseAmplitude: 0f 
        ));
    }

    protected override Tile? GetTileTransformation(int x, int y, float maskValue)
    {
        var type = TileRegistry.Get("SLATE");
        return new Tile(type, type.GetRandomSpriteIndex(WorldGenRNG.GenRandomizer()));
    }
}