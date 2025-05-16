using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Catalyst.Entities;
using Catalyst.Entities.Player;
using Catalyst.Globals;
using Catalyst.Systems;
using Catalyst.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Catalyst.Core;

public class World
{
    public CollisionSystem CollisionSystem;
    
    public readonly Queue<Point> DebugCollidedTiles = [];
    public readonly Queue<Point> DebugCheckedTiles = [];
    public float DebugVanishTimeSecs = 0.5f;
    public float DebugTimer = 0f;
    
    private const int SpawnAreaSize = 5;
    
    private readonly bool[,] _tiles;
    private readonly Point _worldSize;
    private readonly FastNoiseLite _noise;
    
    private Player _playerRef;
    private readonly List<Entity> _npcs = [];

    private bool _debug;

    public World(int sizeX, int sizeY, bool debug=false)
    {
        _debug = debug;
        
        _tiles = new bool[sizeX, sizeY];
        _worldSize = new Point(sizeX, sizeY);
        _noise = new FastNoiseLite();
        CollisionSystem = new CollisionSystem(this, _debug);
        SetupNoise();
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
        for (int x = 0; x < _worldSize.X; x++)
        {
            float noiseValue = _noise.GetNoise(x, 0);
            int surfaceY = (int)(_worldSize.Y / 2 + noiseValue * Settings.WorldGenNoiseAmplitude);

            for (int y = 0; y < _worldSize.Y; y++)
            {
                _tiles[x, y] = y > surfaceY;
            }
        }
    }

    public void SetPlayer(Player playerRef)
    {
        _playerRef = playerRef;
    }

    public bool GetTileAt(int x, int y)
    {
        return _tiles[x, y];
    }
    
    public bool IsWithinBounds(int x, int y)
    {
        return x >= 0 && x < _worldSize.X && y >= 0 && y < _worldSize.Y;
    }
    
    public bool IsPositionSolid(int x, int y)
    {
        return !IsWithinBounds(x, y) || GetTileAt(x, y) == true;
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
                var tile = GetTileAt(x, y);
                if (tile == false)
                {
                    return new Vector2(x, y) * Settings.TileSize;
                }
            }
        }
        
        return Vector2.Zero;
    }

    private void SetupNoise()
    {
        _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _noise.SetFrequency(0.05f);
        
        var random = new Random();
        _noise.SetSeed(random.Next());
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

    public void SetTileAt(int x, int y, bool isSolid)
    {
        if (IsWithinBounds(x, y))
        {
            _tiles[x, y] = isSolid;
        }
    }
}