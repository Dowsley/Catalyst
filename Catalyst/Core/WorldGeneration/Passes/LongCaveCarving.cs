using Catalyst.Core.WorldGeneration.Masks;
using Catalyst.Tiles;
using System;

namespace Catalyst.Core.WorldGeneration.Passes;

public class LongCaveCarving : Pass
{
    private const float MainFrequency = 0.0025f;
    private const float StretchFactor = 3.0f;
    private const float CaveThreshold = 0.15f;

    private const float DistortionFrequency = 0.04f;
    private const float DistortionStrength = 15.0f;

    private readonly FastNoiseLite _mainCaveNoise;
    private readonly FastNoiseLite _distortXNoise;
    private readonly FastNoiseLite _distortYNoise;

    private const int BoundaryNoiseSeedOffset = 200;

    public LongCaveCarving(World world, int seed) : base(world, seed + 1)
    {
        _mainCaveNoise = new FastNoiseLite(seed + 2);
        _mainCaveNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _mainCaveNoise.SetFrequency(MainFrequency);
        _mainCaveNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _mainCaveNoise.SetFractalLacunarity(2.0f);
        _mainCaveNoise.SetFractalGain(0.5f);
        _mainCaveNoise.SetFractalWeightedStrength(0f);

        _distortXNoise = new FastNoiseLite(seed + 3);
        _distortXNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _distortXNoise.SetFrequency(DistortionFrequency);

        _distortYNoise = new FastNoiseLite(seed + 4);
        _distortYNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _distortYNoise.SetFrequency(DistortionFrequency);

        PassMasks.Clear();
        PassMasks.Add(new LayerMask(
            world.WorldSize, 
            ["cavern", "underworld"],
            allowList: true, 
            boundaryNoiseSeed: seed + BoundaryNoiseSeedOffset
            ));
    }

    protected override Tile? GetTileTransformation(int x, int y, float maskValue)
    {
        float offsetX = _distortXNoise.GetNoise(x, y) * DistortionStrength;
        float offsetY = _distortYNoise.GetNoise(x, y) * DistortionStrength;

        float warpedX = x + offsetX;
        float warpedY = y + offsetY;

        float noiseValue = _mainCaveNoise.GetNoise(warpedX, warpedY * StretchFactor);
        if (Math.Abs(noiseValue) < CaveThreshold)
        {
            return CreateEmptyTile();
        }

        return null;
    }
}