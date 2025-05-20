using System;
using System.Collections.Generic;
using System.Linq;
using Catalyst.Core.WorldGeneration.Masks;
using Catalyst.Tiles;
using Microsoft.Xna.Framework;

namespace Catalyst.Core.WorldGeneration;

public abstract class Pass
{
    protected readonly World World;
    protected readonly Random Random;
    protected readonly FastNoiseLite Noise;
    protected readonly Point WorldSize;
    protected readonly TileRegistry TileRegistry;
    protected readonly int Seed;
    protected List<PassMask> PassMasks;

    protected Pass(World worldRef, int seed)
    {
        World = worldRef;
        Random = new Random(seed);
        Noise = new FastNoiseLite(seed);
        WorldSize = worldRef.WorldSize;
        TileRegistry = worldRef.TileRegistry;
        Seed = seed;
        PassMasks =
        [
            new DefaultMask(WorldSize)
        ];
    }

    public void Apply()
    {
        for (int x = 0; x < WorldSize.X; x++)
        {
            for (int y = 0; y < WorldSize.Y; y++)
            {
                // All masks must allow the operation at this point.
                if (!PassMasks.All(mask => mask.IsAllowed(x, y)))
                    continue;

                // Multiply values from all masks to get the combined strength.
                float combinedMaskValue = PassMasks.Aggregate(1.0f, (current, mask) => current * mask.GetValue(x, y));
                
                // If combined strength is zero or less, skip.
                if (combinedMaskValue <= 0.0f) 
                    continue;

                var newTile = GetTileTransformation(x, y, combinedMaskValue);
                if (newTile != null)
                    World.SetTileAt(x, y, (Tile)newTile);
            }
        }
    }

    protected abstract Tile? GetTileTransformation(int x, int y, float maskValue);

    protected Tile CreateEmptyTile()
    {
        var type = TileRegistry.Get("EMPTY");
        return new Tile(type, type.GetRandomSpriteIndex(Random));
    }
}