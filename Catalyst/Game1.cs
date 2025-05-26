using System;
using System.Collections.Generic;
using Catalyst.Core;
using Catalyst.Entities.Player;
using Catalyst.Globals;
using Catalyst.Graphics;
using Catalyst.Systems;
using Catalyst.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Catalyst;

public class Game1 : Game
{
    /* Rendering */
    private readonly GraphicsDeviceManager _graphics;
    private Renderer _renderer = null!;

    /* Core */
    private readonly TileRegistry _tileRegistry = new();
    private Camera2D _camera = null!;
    private World _world = null!;
    private Player _player = null!;

    /* Debug */
    private bool _debug = false;
    
    /* Gameplay */
    private readonly List<string> _placeableTypes = ["DIRT", "STONE", "OAK_LOG"];
    private int _currType = 0;

    /* Map */
    private bool _isMapOpen = false;
    private Vector2 _mapCameraPosition = Vector2.Zero;
    private float _mapCameraZoom = 1.0f; // Initial zoom level for the map
    private const float MapZoomSpeed = 0.1f;
    private const float MapPanSpeed = 2000.0f;
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = Settings.NativeWidth;
        _graphics.PreferredBackBufferHeight = Settings.NativeHeight;
        
        RegisterActions();
    }

    protected override void Initialize()
    {
        _renderer = new Renderer(GraphicsDevice, Content);
        
        InitializeTileTypes();
        _world = new World(
            1750, // Tiny sized world for now https://terraria.fandom.com/wiki/World_size
            900,
            _tileRegistry,
            _debug
        );
        _world.GenerateTerrain();
        
        base.Initialize();
        InitializePlayer();
    }

    protected override void LoadContent()
    {
        _renderer.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        InputSystem.Update();

        if (InputSystem.IsActionJustPressed("toggle_map") || (_isMapOpen && InputSystem.IsActionJustPressed("exit_map")))
        {
            _isMapOpen = !_isMapOpen;
            if (_isMapOpen)
            {
                _mapCameraPosition = World.GridToWorld(_player.GridPosition);
                _mapCameraZoom = 1.0f;
            }
        }

        if (_isMapOpen)
        {
            UpdateMapControls(gameTime);
        }
        else
        {
            UpdateGameControls(gameTime);
            _world.Update(gameTime);
            _camera.Position = _player.Position + new Vector2(_renderer.CharacterTexture.Width / 2f, _renderer.CharacterTexture.Height / 2f);
        }
        
        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        _renderer.Draw(
            gameTime, 
            _isMapOpen, 
            _camera, 
            _world, 
            _player, 
            _debug, 
            _mapCameraPosition, 
            _mapCameraZoom,
            _placeableTypes,
            _currType
            );

        base.Draw(gameTime);
    }

    private static void RegisterActions()
    {
        InputSystem.CreateAction("left", [Keys.A]);
        InputSystem.CreateAction("right", [Keys.D]);
        InputSystem.CreateAction("jump", [Keys.Space]);
        InputSystem.CreateAction("next_tile", [Keys.Tab]);
        InputSystem.CreateAction("toggle_map", [Keys.M]);
        InputSystem.CreateAction("exit_map", [Keys.Escape]);
        InputSystem.CreateAction("map_up", [Keys.W]);
        InputSystem.CreateAction("map_down", [Keys.S]);
        InputSystem.CreateAction("map_left", [Keys.A]);
        InputSystem.CreateAction("map_right", [Keys.D]);
    }

    private void InitializePlayer()
    {
        var spawningPos = World.GridToWorld(_world.GetSpawningPosForPlayer());
        _player = new Player(spawningPos, new Vector2(_renderer.CharacterTexture.Width, _renderer.CharacterTexture.Height));
        _camera = new Camera2D(_player.Position);
        
        _world.SetPlayer(_player);
    }

    private void InitializeTileTypes()
    {
        // TODO: Implement data-driven approach. All types should be XMLs, and loaded by a loader inside tileRegistry.
        var dirtTileType = new TileType("DIRT", "Dirt", "Just some dirt", 100, true)
            { MapColor = new Color(151, 106, 76) };
        dirtTileType.AddSpriteVariant(new Sprite2D("Dirt", new Rectangle(0, 1, Settings.TileSize, Settings.TileSize)));
        
        var stoneTileType = new TileType("STONE", "Stone", "Just stone", 500, true)
            { MapColor = new Color(130, 127, 129) };
        stoneTileType.AddSpriteVariant(new Sprite2D("Stone", new Rectangle(0, 1, Settings.TileSize, Settings.TileSize)));
        
        var oakLogType = new TileType("OAK_LOG", "Oak Log", "Just oak log", 200, true)
            { MapColor = new Color(139, 69, 19) };
        oakLogType.AddSpriteVariant(new Sprite2D("Oak Logs", new Rectangle(0, 1, Settings.TileSize, Settings.TileSize)));
        
        var emptyType = new TileType("EMPTY", "Empty", "Just air", 0, false)
            { MapColor = Color.CornflowerBlue };
        emptyType.AddSpriteVariant(new Sprite2D("Empty", new Rectangle(0, 0, Settings.TileSize, Settings.TileSize)));
        
        var slateType = new TileType("SLATE", "Slate", "Hard like bedrock", 1000, true)
            { MapColor = Color.DarkSlateGray };
        slateType.AddSpriteVariant(new Sprite2D("Slate", new Rectangle(0, 0, Settings.TileSize, Settings.TileSize)));
        
        var redClayType = new TileType("RED_CLAY", "Red Clay", "Red like... clay?", 100, true)
            { MapColor = new Color(149, 81, 67) };
        redClayType.AddSpriteVariant(new Sprite2D("Red Clay", new Rectangle(0, 0, Settings.TileSize, Settings.TileSize)));

        _tileRegistry.Register(dirtTileType.Id, dirtTileType);
        _tileRegistry.Register(stoneTileType.Id, stoneTileType);
        _tileRegistry.Register(oakLogType.Id, oakLogType);
        _tileRegistry.Register(emptyType.Id, emptyType);
        _tileRegistry.Register(slateType.Id, slateType);
        _tileRegistry.Register(redClayType.Id, redClayType);
    }

    private void UpdateGameControls(GameTime gameTime)
    {
        if (InputSystem.IsActionJustPressed("next_tile"))
        {
            _currType += 1;
            if (_currType >= _placeableTypes.Count)
                _currType = 0;
        }

        var mouseScreenPos = InputSystem.GetMousePosition();

        Matrix worldViewTransformMatrix =
            Matrix.CreateTranslation(-_camera.Position.X, -_camera.Position.Y, 0) * // Center on camera's focus (player's center)
            Matrix.CreateScale(Settings.ResScale, Settings.ResScale, 1.0f) *          // Apply zoom
            Matrix.CreateTranslation(Settings.NativeWidth / 2f, Settings.NativeHeight / 2f, 0); // Translate to screen center

        var lmbPressed = InputSystem.IsMouseButtonPressed(InputSystem.MouseButton.Left);
        var rmbPressed = InputSystem.IsMouseButtonPressed(InputSystem.MouseButton.Right);
        if (lmbPressed || rmbPressed)
        {
            var worldPos = Vector2.Transform(mouseScreenPos, Matrix.Invert(worldViewTransformMatrix));
            var gridPos = World.WorldToGrid(worldPos);
            _world.SetTileAt(gridPos.X, gridPos.Y, lmbPressed ? _tileRegistry.Get(_placeableTypes[_currType]) : _tileRegistry.Get("EMPTY"));
        }
    }

    private void UpdateMapControls(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (InputSystem.IsActionPressed("map_up"))
            _mapCameraPosition.Y -= MapPanSpeed * deltaTime / _mapCameraZoom;
        if (InputSystem.IsActionPressed("map_down"))
            _mapCameraPosition.Y += MapPanSpeed * deltaTime / _mapCameraZoom;
        if (InputSystem.IsActionPressed("map_left"))
            _mapCameraPosition.X -= MapPanSpeed * deltaTime / _mapCameraZoom;
        if (InputSystem.IsActionPressed("map_right"))
            _mapCameraPosition.X += MapPanSpeed * deltaTime / _mapCameraZoom;

        // Zoom with mouse wheel
        var scrollDelta = InputSystem.GetMouseScrollDelta();
        switch (scrollDelta)
        {
            case > 0:
                _mapCameraZoom += MapZoomSpeed * _mapCameraZoom; // Zoom in, increase zoom factor
                break;
            case < 0:
                _mapCameraZoom -= MapZoomSpeed * _mapCameraZoom; // Zoom out, decrease zoom factor
                break;
        }
            
        _mapCameraZoom = Math.Max(0.1f, _mapCameraZoom); // Prevent zoom from becoming too small or zero

        // Clamp map camera position to world bounds (considering zoom)
        // The position is in world coordinates, so we need to convert the half-screen size to world coordinates.
        float halfViewWidthWorld = Settings.NativeWidth / (2f * _mapCameraZoom) ; 
        float halfViewHeightWorld = Settings.NativeHeight / (2f * _mapCameraZoom);

        _mapCameraPosition.X = MathHelper.Clamp(_mapCameraPosition.X, halfViewWidthWorld, (World.GridToWorld(_world.GetWidth())) - halfViewWidthWorld);
        _mapCameraPosition.Y = MathHelper.Clamp(_mapCameraPosition.Y, halfViewHeightWorld, (World.GridToWorld(_world.GetHeight())) - halfViewHeightWorld);
    }
}
