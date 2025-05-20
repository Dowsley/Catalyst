using System.Linq;
using Catalyst.Globals;
using Catalyst.Tiles;

namespace Catalyst.Core.WorldGeneration.Passes;

public class InitialTerrainPass : Pass
{
    public const int Amplitude = 5; // how tall (or low) the hills can be
    private readonly float _surfaceMidPointYPercent;
    
    public InitialTerrainPass(World world, int seed) : base(world, seed)
    {
        Noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        Noise.SetFrequency(0.05f);

        float surfaceStartPercent = 0.0f;
        float surfaceEndPercent = 0.0f;

        int index = Settings.Layers.FindIndex(layer => layer.Item1 == "surface");
        surfaceEndPercent = Settings.Layers[index].Item2;
        surfaceStartPercent = Settings.Layers[index-1].Item2; // End of the previous layer

        _surfaceMidPointYPercent = (surfaceStartPercent + surfaceEndPercent) / 2.0f;
    }

    protected override Tile? GetTileTransformation(int x, int y)
    {
        float noiseValue = Noise.GetNoise(x, 0);
        int surfaceY = (int)(WorldSize.Y * _surfaceMidPointYPercent + noiseValue * Amplitude);
        var type = y > surfaceY ? TileRegistry.Get("GRASS") : TileRegistry.Get("EMPTY");
        return new Tile(type, type.GetRandomSpriteIndex(Random));
    }
}
