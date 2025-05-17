using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    public CollisionSystem CollisionSystem;
    public TileRegistry TileRegistry;
    
    public readonly Queue<Point> DebugCollidedTiles = [];
    public readonly Queue<Point> DebugCheckedTiles = [];
    public float DebugVanishTimeSecs = 0.5f;
    public float DebugTimer = 0f;
    
    private const int SpawnAreaSize = 5;
    
    private readonly Point _worldSize;
    private readonly Tile[,] _tiles;
    private readonly FastNoiseLite _noise = new();
    private Random _random;
    
    private Player _playerRef;
    private readonly List<Entity> _npcs = [];

    private bool _debug;

    public World(int sizeX, int sizeY, TileRegistry tileRegistry, bool debug=false)
    {
        _debug = debug;
        _tiles = new Tile[sizeX, sizeY];
        _worldSize = new Point(sizeX, sizeY);
        CollisionSystem = new CollisionSystem(this, _debug);
        TileRegistry = tileRegistry;
        SetupRandom();
    }
    
    public void Update(GameTime gameTime, KeyboardState kState)
    {
        DebugTimer += TimeUtils.GetDelta(gameTime);
        if (DebugTimer >= DebugVanishTimeSecs)
        {
            DebugCollidedTiles.Clear();
            DebugCheckedTiles.Clear();
            DebugTimer = 0f;
        }
        UpdateAllEntities(gameTime, kState);
    }

    public void UpdateAllEntities(GameTime gameTime, KeyboardState kState)
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

    public Point WorldToGrid(Vector2 worldPos)
    {
        return new Point(
            (int)(worldPos.X / Settings.TileSize),
            (int)(worldPos.Y / Settings.TileSize)
        );
    }
    
    /* Returns the world position on the top-left most point of the grid coordinate */
    public Vector2 GridToWorld(Point gridPos)
    {
        return new Vector2(
            gridPos.X * Settings.TileSize,
            gridPos.Y * Settings.TileSize
        );
    }

    public void SetTileAt(int x, int y, TileType type)
    {
        if (IsWithinBounds(x, y))
        {
            _tiles[x, y] = new Tile(type, type.GetRandomSpriteIndex(_random));
        }
    }
}