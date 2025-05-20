using Catalyst.Core.WorldGeneration.Masks;
using Catalyst.Tiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Catalyst.Core.WorldGeneration.Passes;

public class PatchesPass : Pass
{
    private readonly float _placementThreshold;
    private readonly string _tileTypeId;
    private const int BoundaryNoiseSeedOffset = 456; // Unique offset

    public PatchesPass(World world, FastNoiseLite noise, string tileTypeId, string targetLayerName, float placementThreshold, int seed) 
        : base(world, seed)
    {
        _tileTypeId = tileTypeId;
        _placementThreshold = placementThreshold;

        Noise = noise;
        noise.SetSeed(seed);

        PassMasks.Clear();
        PassMasks.Add(new LayerMask(
            world.WorldSize,
            [targetLayerName],
            allowList: true,
            boundaryNoiseSeed: seed + BoundaryNoiseSeedOffset,
            startBoundaryNoiseAmplitude: 0.005f,
            endBoundaryNoiseAmplitude: 0.005f
        ));
    }

    protected override Tile? GetTileTransformation(int x, int y, float maskValue)
    {
        var id = World.GetTileTypeAt(x, y).Id;
        if (id != "DIRT" && id != "STONE")
            return null;
        
        float noiseValue = Noise.GetNoise(x, y);
        if (!(noiseValue > _placementThreshold))
            return null;
        
        var tileType = TileRegistry.Get(_tileTypeId);
        return new Tile(tileType, tileType.GetRandomSpriteIndex(Random));
    }
} 