using System.Linq;
using Catalyst.Globals;
using Catalyst.Tiles;

namespace Catalyst.Core.WorldGeneration.Passes;

public class InitialTerrainPass : Pass
{
    private const int Amplitude = 5; // how tall (or low) the hills can be
    
    public InitialTerrainPass(World world) : base(world)
    {
        Noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        Noise.SetFrequency(0.05f);
    }

    protected override Tile? GetTileTransformation(int x, int y, float maskValue)
    {
        float noiseValue = Noise.GetNoise(x, 0);
        int surfaceY = (int)(WorldGenSettings.ComputeSurfaceBaseLine(WorldSize) + noiseValue * Amplitude);
        var type = y > surfaceY ? TileRegistry.Get("DIRT") : TileRegistry.Get("EMPTY");
        return new Tile(type, type.GetRandomSpriteIndex(WorldGenRNG.GenRandomizer()));
    }
}
