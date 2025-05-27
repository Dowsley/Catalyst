using System;
using System.Collections.Generic;
using Catalyst.Core.WorldGeneration;
using Catalyst.Entities;
using Catalyst.Entities.Player;
using Catalyst.Globals;
using Catalyst.Graphics;
using Catalyst.Systems;
using Catalyst.Tiles;
using Catalyst.Utils;
using Microsoft.Xna.Framework;

namespace Catalyst.Core;

public class World
{
    public readonly CollisionSystem CollisionSystem;
    public readonly TileRegistry TileRegistry;
    public readonly WorldGenerator Generator;
    
    public readonly Queue<Point> DebugCollidedTiles = [];
    public readonly Queue<Point> DebugCheckedTiles = [];
    public const float DebugVanishTimeSecs = 0.5f;
    public float DebugTimer = 0f;
    
    private const int SpawnAreaSize = 5;

    public readonly Point WorldSize;
    private readonly Tile[,] _tiles;
    
    private Player _playerRef = null!;
    private readonly List<Entity> _npcs = [];

    private readonly bool _debug;
    private const int Seed = 0;
    private readonly Random _random = new(Seed);

    public World(int sizeX, int sizeY, TileRegistry tileRegistry, bool debug=false)
    {
        _debug = debug;
        _tiles = new Tile[sizeX, sizeY];
        WorldSize = new Point(sizeX, sizeY);
        CollisionSystem = new CollisionSystem(this, _debug);
        TileRegistry = tileRegistry;
        Generator = new WorldGenerator(this, Seed);
    }
    
    public void Update(GameTime gameTime)
    {
        DebugTimer += TimeUtils.GetDelta(gameTime);
        if (DebugTimer >= DebugVanishTimeSecs)
        {
            DebugCollidedTiles.Clear();
            DebugCheckedTiles.Clear();
            DebugTimer = 0f;
        }
        UpdateAllEntities(gameTime);
    }

    public void UpdateAllEntities(GameTime gameTime)
    {
        _playerRef.Update(this, gameTime);
        foreach (var npc in _npcs)
            npc.Update(this, gameTime);
    }
    
    public void GenerateTerrain()
    {
        Generator.Generate();
    }

    public void SetPlayer(Player playerRef)
    {
        _playerRef = playerRef;
    }

    public TileType GetTileTypeAt(int x, int y)
    {
        return _tiles[x, y].Type;
    }
    
    public Sprite GetTileSpriteAt(int x, int y)
    {
        return _tiles[x, y].Sprite;
    }
    
    public bool IsWithinBounds(int x, int y)
    {
        return x >= 0 && x < WorldSize.X && y >= 0 && y < WorldSize.Y;
    }
    
    public bool IsPositionSolid(int x, int y)
    {
        return !IsWithinBounds(x, y) || GetTileTypeAt(x, y).IsSolid;
    }
    
    public int GetWidth()
    {
        return WorldSize.X;
    }
    
    public int GetHeight()
    {
        return WorldSize.Y;
    }

    /// <summary>
    /// Finds a random spawning position for player.
    /// </summary>
    /// <returns>Position in grid space</returns>
    public Point GetSpawningPosForPlayer()
    {
        int middleHorizontal = GetWidth() / 2;
        for (int x = int.Max(middleHorizontal-SpawnAreaSize, 0); x < int.Min(middleHorizontal+SpawnAreaSize+1, GetWidth()-1); x++)
        {
            for (int y = 0; y < GetHeight(); y++)
            {
                var tile = GetTileTypeAt(x, y);
                if (tile.IsSolid)
                {
                    return new Point(x, y-5); // 5 blocks up to give space to the player
                }
            }
        }
        
        return new Point(middleHorizontal, 5); // 5 below origin so player does not get stuck in skybox
    }

    /// <summary>
    /// Returns that world vector converted to grid space (in tiles)
    /// </summary>
    public static Point WorldToGrid(Vector2 vec)
    {
        return new Point(
            WorldToGrid(vec.X),
            WorldToGrid(vec.Y)
        );
    }
    
    /// <summary>
    /// Returns that world scalar converted to grid space (in tiles)
    /// </summary>
    public static int WorldToGrid(float value)
    {
        return (int)(value / Settings.TileSize);
    }
    
    /// <summary>
    /// Returns that grid vector converted to world space (in pixels)
    /// </summary>
    /// <remarks>
    /// The origin of a grid coordinate is on its top-left.
    /// </remarks>
    public static Vector2 GridToWorld(Point vec)
    {
        return new Vector2(
            GridToWorld(vec.X),
            GridToWorld(vec.Y)
        );
    }

    /// <summary>
    /// Returns that grid scalar converted to world space (in pixels)
    /// </summary>
    /// <remarks>
    /// The origin of a grid coordinate is on its top-left.
    /// </remarks>
    public static float GridToWorld(int val)
    {
        return val * Settings.TileSize;
    }

    public void SetTileAt(int x, int y, TileType type)
    {
        if (IsWithinBounds(x, y))
        {
            _tiles[x, y] = new Tile(type, type.GetRandomSpriteIndex(_random));
        }
    }
    
    public void SetTileAt(int x, int y, Tile tile)
    {
        if (IsWithinBounds(x, y))
        {
            _tiles[x, y] = tile;
        }
    }
}