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
    protected FastNoiseLite Noise;
    protected readonly Point WorldSize;
    protected readonly TileRegistry TileRegistry;
    protected List<PassMask> PassMasks;

    protected Pass(World worldRef)
    {
        World = worldRef;
        Noise = WorldGenRNG.GenNoise();
        WorldSize = worldRef.WorldSize;
        TileRegistry = worldRef.TileRegistry;
        PassMasks =
        [
            new DefaultMask(WorldSize)
        ];
    }

    // TODO optimize this without linq expressions that reallocate memory
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

    protected Tile CreateEmptyTile(TileType? wallType = null)
    {
        var emptyType = TileRegistry.Get("EMPTY");
        return new Tile(emptyType, wallType ?? emptyType, emptyType.GetRandomSpriteIndex(WorldGenRNG.GenRandomizer()));
    }
}