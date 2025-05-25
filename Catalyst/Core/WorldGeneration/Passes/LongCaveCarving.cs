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

    public LongCaveCarving(World world) : base(world)
    {
        _mainCaveNoise = WorldGenRNG.GenNoise();
        _mainCaveNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _mainCaveNoise.SetFrequency(MainFrequency);
        _mainCaveNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _mainCaveNoise.SetFractalLacunarity(2.0f);
        _mainCaveNoise.SetFractalGain(0.5f);
        _mainCaveNoise.SetFractalWeightedStrength(0f);

        _distortXNoise = WorldGenRNG.GenNoise();
        _distortXNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _distortXNoise.SetFrequency(DistortionFrequency);

        _distortYNoise = WorldGenRNG.GenNoise();
        _distortYNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _distortYNoise.SetFrequency(DistortionFrequency);

        PassMasks.Clear();
        PassMasks.Add(new LayerMask(
            world.WorldSize, 
            ["cavern", "underworld"],
            allowList: true, 
            boundaryNoiseSeed: WorldGenRNG.GenSeed()
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