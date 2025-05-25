using System.Collections.Generic;
using Catalyst.Core.WorldGeneration.Passes;

namespace Catalyst.Core.WorldGeneration;

public class WorldGenerator
{
    private readonly List<Pass> _passes = [];

    public WorldGenerator(World world, int seed)
    {
        WorldGenRNG.SetWorldSeed(seed);
        
        _passes.Add(new InitialTerrainPass(world));

        // Stone Patches
        FastNoiseLite stoneNoise = WorldGenRNG.GenNoise();
        stoneNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        stoneNoise.SetFrequency(0.07f/2f);
        stoneNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        stoneNoise.SetFractalOctaves(3);
        stoneNoise.SetFractalLacunarity(2.0f);
        stoneNoise.SetFractalGain(0.5f);
        _passes.Add(new PatchesPass(world, stoneNoise, "STONE", "surface", 0.4f));
        _passes.Add(new PatchesPass(world, stoneNoise, "STONE", "underground", 0.0f));
        _passes.Add(new PatchesPass(world, stoneNoise, "STONE", "cavern", -0.4f));
        
        // Clay Patches
        FastNoiseLite clayNoise = new();
        clayNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        clayNoise.SetFrequency(0.02f);
        clayNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        clayNoise.SetFractalOctaves(3);
        clayNoise.SetFractalLacunarity(2.0f);
        clayNoise.SetFractalGain(0.5f);
        const float placementThreshold = 0.7f;
        _passes.Add(new PatchesPass(world, clayNoise, "RED_CLAY", "surface", placementThreshold));
        _passes.Add(new PatchesPass(world, clayNoise, "RED_CLAY", "underground", placementThreshold));
        _passes.Add(new PatchesPass(world, clayNoise, "RED_CLAY", "cavern", placementThreshold));

        _passes.Add(new SmallCaveCarvingPass(world));
        _passes.Add(new CaveCarvingPass(world));
        _passes.Add(new LongCaveCarving(world));
        _passes.Add(new BedrockLayerPass(world));
    }

    public void Generate()
    {
        foreach (var pass in _passes)
            pass.Apply();
    }
}