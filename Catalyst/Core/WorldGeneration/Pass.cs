using System;
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
    protected PassMask PassMask; // TODO: make this a group/list of Masks.

    protected Pass(World worldRef, int seed)
    {
        World = worldRef;
        Random = new Random(seed);
        Noise = new FastNoiseLite(seed);
        WorldSize = worldRef.WorldSize;
        TileRegistry = worldRef.TileRegistry;
        Seed = seed;
        PassMask = new DefaultMask(WorldSize);
    }

    public void Apply()
    {
        for (int x = 0; x < WorldSize.X; x++)
        {
            for (int y = 0; y < WorldSize.Y; y++)
            {
                if (!PassMask.IsAllowed(x, y))
                    continue;
                var newTile = GetTileTransformation(x, y);
                if (newTile != null)
                    World.SetTileAt(x, y, (Tile)newTile);
            }
        }
    }

    protected abstract Tile? GetTileTransformation(int x, int y);
}