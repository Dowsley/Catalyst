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
    public bool WorldGenerating = true;
    
    public readonly Queue<Point> DebugCollidedTiles = [];
    public readonly Queue<Point> DebugCheckedTiles = [];
    public const float DebugVanishTimeSecs = 0.5f;
    public float DebugTimer = 0f;
    
    private const int SpawnAreaSize = 5;

    public readonly Point WorldSize;
    private readonly Tile[,] _tiles;
    private readonly LightingSystem _lightingSystem;
    private readonly float[,] _lightValues;
    private const int UpdateRadius = 20;
    
    public Player? PlayerRef = null!;
    private readonly List<Entity> _npcs = [];

    private readonly bool _debug;
    private const int Seed = 0;
    private readonly Random _random = new(Seed);

    private const float ViewHalfWidthWorld = Settings.NativeWidth / (2f * Settings.ResScale);
    private const float ViewHalfHeightWorld = Settings.NativeHeight / (2f * Settings.ResScale);

    public World(int sizeX, int sizeY, TileRegistry tileRegistry, bool debug=false)
    {
        _debug = debug;
        _tiles = new Tile[sizeX, sizeY];
        _lightValues = new float[sizeX, sizeY]; 
        WorldSize = new Point(sizeX, sizeY);
        CollisionSystem = new CollisionSystem(this, _debug);
        TileRegistry = tileRegistry;
        Generator = new WorldGenerator(this, Seed);
        _lightingSystem = new LightingSystem(this);
    }
    
    public void InitializeLighting()
    {
        _lightingSystem.InitializeEntireWorldLighting();
    }
    
    public float GetLightValueAt(int x, int y)
    {
        return !IsWithinBounds(x, y) ? 0.0f : _lightValues[x, y];
    }
    
    public float GetLightValueForRenderingAt(int x, int y)
    {
        return Math.Clamp(GetLightValueAt(x, y), 0f, 1f);
    }

    public void SetLightValue(int x, int y, float value)
    {
        if (IsWithinBounds(x,y))
        {
            _lightValues[x, y] = value;
        }
    }

    private Rectangle CalculateViewRectInTiles(Camera camera)
    {
        float viewLeftWorld = camera.Position.X - ViewHalfWidthWorld;
        float viewRightWorld = camera.Position.X + ViewHalfWidthWorld;
        float viewTopWorld = camera.Position.Y - ViewHalfHeightWorld;
        float viewBottomWorld = camera.Position.Y + ViewHalfHeightWorld;

        const int padding = 20; 
        int startTileX = WorldToGrid(viewLeftWorld) - padding;
        int endTileX = WorldToGrid(viewRightWorld) + padding;
        int startTileY = WorldToGrid(viewTopWorld) - padding;
        int endTileY = WorldToGrid(viewBottomWorld) + padding;

        startTileX = Math.Max(0, startTileX);
        startTileY = Math.Max(0, startTileY);
        endTileX = Math.Min(WorldSize.X, endTileX); 
        endTileY = Math.Min(WorldSize.Y, endTileY);

        return new Rectangle(startTileX, startTileY, Math.Max(0, endTileX - startTileX), Math.Max(0, endTileY - startTileY));
    }

    public void Update(GameTime gameTime, Camera camera) 
    {
        DebugTimer += TimeUtils.GetDelta(gameTime);
        if (DebugTimer >= DebugVanishTimeSecs)
        {
            DebugCollidedTiles.Clear();
            DebugCheckedTiles.Clear();
            DebugTimer = 0f;
        }
        UpdateAllEntities(gameTime);

        if (WorldGenerating)
            return;
        Rectangle viewRect = CalculateViewRectInTiles(camera);
        if (viewRect is { Width: > 0, Height: > 0 })
        {
            _lightingSystem.RequestLightingUpdate(viewRect);
        }
    }
 
    private bool ForceLightUpdate() // This might move to LightingSystem or be used by it
    {
        return false; 
    }

    public Tile GetTileAt(int x, int y)
    {
        var emptyType = TileRegistry.Get("EMPTY");
        return !IsWithinBounds(x,y)
            ? new Tile(emptyType, emptyType, 0)
            : _tiles[x,y];
    }

    public void GenerateTerrain()
    {
        WorldGenerating = true;
        Generator.Generate();
        WorldGenerating = false;
    }

    public void SetTileAt(int x, int y, TileType tileType, TileType? wallType = null)
    {
        SetTileAt(x, y, new Tile(
            tileType,
            wallType ?? TileRegistry.Get("EMPTY"),
            tileType.GetRandomSpriteIndex(_random))
        );
    }
    
    public void SetTileTypeAt(int x, int y, TileType? tileType, TileType? wallType = null)
    {
        if (tileType != null)
            _tiles[x, y].Type = tileType;
        if (wallType != null)
            _tiles[x, y].WallType = wallType;
        
        if (WorldGenerating)
            return;
        
        Rectangle changedArea = new Rectangle(
            x - UpdateRadius,
            y - UpdateRadius,
            UpdateRadius * 2,
            UpdateRadius * 2
        );
        _lightingSystem.RequestLightingUpdate(changedArea);
    }

    // TODO: We should stop doing this, and just set TileType instead. Tile is merely a flyweight container.
    public void SetTileAt(int x, int y, Tile tile)
    {
        if (!IsWithinBounds(x, y))
             return;
         
        _tiles[x, y] = tile;
         
        if (WorldGenerating)
             return;
         
        Rectangle changedArea = new Rectangle(
             x - UpdateRadius,
             y - UpdateRadius,
             UpdateRadius * 2,
             UpdateRadius * 2
        );
        _lightingSystem.RequestLightingUpdate(changedArea);
    }

    private void UpdateAllEntities(GameTime gameTime)
    {
        PlayerRef?.Update(this, gameTime);
        foreach (var npc in _npcs)
            npc.Update(this, gameTime);
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

    public void SetPlayer(Player playerRef)
    {
        PlayerRef = playerRef;
    }
}