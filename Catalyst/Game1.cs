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
    private Camera _camera = null!;
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
    private float _mapCameraZoom = 1.0f;
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
        _camera = new Camera(_player.Position);
        
        _world.SetPlayer(_player);
    }

    private void InitializeTileTypes()
    {
        TileTypeLoader.LoadTileTypesFromDirectory("Data/TileTypes", _tileRegistry);
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
        var factor = MapPanSpeed * deltaTime / _mapCameraZoom;
        if (InputSystem.IsActionPressed("map_up"))
            _mapCameraPosition.Y -= factor;
        if (InputSystem.IsActionPressed("map_down"))
            _mapCameraPosition.Y += factor;
        if (InputSystem.IsActionPressed("map_left"))
            _mapCameraPosition.X -= factor;
        if (InputSystem.IsActionPressed("map_right"))
            _mapCameraPosition.X += factor;

        // Zoom with mouse wheel
        var scrollDelta = InputSystem.GetMouseScrollDelta();
        factor = MapZoomSpeed * _mapCameraZoom;
        switch (scrollDelta)
        {
            case > 0:
                _mapCameraZoom += factor;
                break;
            case < 0:
                _mapCameraZoom -= factor;
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
