using System;
using System.Collections.Generic;
using Catalyst.Entities;
using Catalyst.Entities.Player;
using Catalyst.Globals;
using Catalyst.Graphics;
using Catalyst.Systems;
using Catalyst.Tiles;
using Catalyst.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Catalyst.Core;

public class World
{
    public readonly CollisionSystem CollisionSystem;
    public readonly TileRegistry TileRegistry;
    
    public readonly Queue<Point> DebugCollidedTiles = [];
    public readonly Queue<Point> DebugCheckedTiles = [];
    public const float DebugVanishTimeSecs = 0.5f;
    public float DebugTimer = 0f;
    
    private const int SpawnAreaSize = 5;
    
    private readonly Point _worldSize;
    private readonly Tile[,] _tiles;
    private readonly FastNoiseLite _noise = new();
    private Random _random = null!;
    
    private Player _playerRef = null!;
    private readonly List<Entity> _npcs = [];

    private readonly bool _debug;

    public World(int sizeX, int sizeY, TileRegistry tileRegistry, bool debug=false)
    {
        _debug = debug;
        _tiles = new Tile[sizeX, sizeY];
        _worldSize = new Point(sizeX, sizeY);
        CollisionSystem = new CollisionSystem(this, _debug);
        TileRegistry = tileRegistry;
        SetupRandom();
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
        // TODO: Implement world generation.
        for (int x = 0; x < _worldSize.X; x++)
        {
            float noiseValue = _noise.GetNoise(x, 0);
            int surfaceY = (int)(_worldSize.Y / 2 + noiseValue * Settings.WorldGenNoiseAmplitude);
            for (int y = 0; y < _worldSize.Y; y++)
            {
                var type = y > surfaceY ? TileRegistry.Get("GRASS") : TileRegistry.Get("EMPTY");
                var tile = new Tile(type, type.GetRandomSpriteIndex(_random));
                _tiles[x, y] = tile;
            }
        }
    }

    public void SetPlayer(Player playerRef)
    {
        _playerRef = playerRef;
    }

    public TileType GetTileTypeAt(int x, int y)
    {
        return _tiles[x, y].Type;
    }
    
    public Sprite2D GetTileSpriteAt(int x, int y)
    {
        return _tiles[x, y].Sprite;
    }
    
    public bool IsWithinBounds(int x, int y)
    {
        return x >= 0 && x < _worldSize.X && y >= 0 && y < _worldSize.Y;
    }
    
    public bool IsPositionSolid(int x, int y)
    {
        return !IsWithinBounds(x, y) || GetTileTypeAt(x, y).IsSolid;
    }
    
    public int GetWidth()
    {
        return _worldSize.X;
    }
    
    public int GetHeight()
    {
        return _worldSize.Y;
    }

    public Vector2 GetSpawningPosForPlayer()
    {
        int middle = GetWidth() / 2;
        for (int x = int.Max(middle-SpawnAreaSize, 0); x < int.Min(middle+SpawnAreaSize+1, GetWidth()-1); x++)
        {
            // Find first empty block to stand on
            for (int y = GetHeight() - 1; y >= 0; y--)
            {
                var tile = GetTileTypeAt(x, y);
                if (tile.Id == "EMPTY")
                {
                    return new Vector2(x, y) * Settings.TileSize;
                }
            }
        }
        
        return Vector2.Zero;
    }

    private void SetupRandom()
    {
        _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _noise.SetFrequency(0.05f);
        
        var seed = new Random().Next();
        _noise.SetSeed(seed);
        _random = new Random(seed);
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
}