using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private SpriteBatch _worldSpriteBatch = null!;
    private SpriteBatch _uiSpriteBatch = null!;
    private RenderTarget2D _renderTarget = null!;
    private SpriteFont _mainFont = null!;
    private Texture2D _charTex = null!;
    private readonly Dictionary<string, Texture2D> _textures = new();
    private readonly Color _defaultBackgroundColor = Color.CornflowerBlue;
    private Effect _gradientSkyEffect = null!;

    /* Core */
    private readonly TileRegistry _tileRegistry = new();
    private Camera2D _camera = null!;
    private World _world = null!;
    private Player _player = null!;

    /* Debug */
    private bool _debug = false;
    private readonly Color _debugColor = new(255, 255, 255, 255/2);
    private Texture2D _debugTexture = null!;
    
    /* Test gameplay */
    private readonly List<string> _placeableTypes = ["DIRT", "STONE", "OAK_LOG"];
    private int _currType = 0;

    /* Map */
    private bool _isMapOpen = false;
    private Vector2 _mapCameraPosition = Vector2.Zero;
    private float _mapCameraZoom = 1.0f; // Initial zoom level for the map
    private Texture2D _pixelTexture = null!; // For drawing map tiles
    private const float MapZoomSpeed = 0.1f;
    private const float MapPanSpeed = 2000.0f;
    private const float MapTileRenderSize = 1f; // draw each tile as 1x1, the matrix will handle the zoom.
    
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
        base.Initialize();
        _renderTarget = new RenderTarget2D(GraphicsDevice, Settings.NativeWidth, Settings.NativeHeight);
        _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        _pixelTexture.SetData([Color.White]);
        
        InitializeTileTypes();
        _world = new World(
            1750, // Tiny sized world for now https://terraria.fandom.com/wiki/World_size
            900,
            _tileRegistry,
            _debug
        );
        _world.GenerateTerrain();
        InitializePlayer();
    }

    protected override void LoadContent()
    {
        _worldSpriteBatch = new SpriteBatch(GraphicsDevice);
        _uiSpriteBatch = new SpriteBatch(GraphicsDevice);
        LoadTextures();
        _gradientSkyEffect = Content.Load<Effect>("Shaders/GradientEffect");
        
        _debugTexture = new Texture2D(GraphicsDevice, 1, 1);
        _debugTexture.SetData([Color.White]);
        _charTex = Content.Load<Texture2D>("Graphics/sample_char");
        _mainFont = Content.Load<SpriteFont>("Fonts/Andy Bold");
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
            _camera.Position = _player.Position + new Vector2(_charTex.Width / 2f, _charTex.Height / 2f);
        }
        
        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        if (_isMapOpen)
        {
            DrawMapMode();
        }
        else
        {
            MainRender();
            ScaleResolution();
            DrawUI();
        }

        base.Draw(gameTime);
    }

    private void DrawTile(int x, int y, Sprite2D sprite)
    {
        Texture2D tex = LookupTexture(sprite.TextureId);
        _worldSpriteBatch.Draw(
            tex,
            new Vector2(x, y) * Settings.TileSize,
            sprite.SourceRect,
            Color.White
        );
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

    private Texture2D LookupTexture(string texId)
    {
        return _textures[texId];
    }

    private void LoadTextures()
    {
        string graphicsPath = Path.Combine(Content.RootDirectory, "Graphics/Pack2");

        if (!Directory.Exists(graphicsPath))
        {
            throw new Exception($"Directory not found: {graphicsPath}");
        }

        var textureFiles = Directory.GetFiles(graphicsPath, "*.png", SearchOption.AllDirectories);

        foreach (var filePath in textureFiles)
        {
            using var stream = File.OpenRead(filePath);
            var texture = Texture2D.FromStream(GraphicsDevice, stream);

            // Extract relative name without extension
            var textureKey = Path.GetFileNameWithoutExtension(filePath);

            _textures[textureKey] = texture;
        }
        
        // Special empty texture
        var emptyTexture = new Texture2D(GraphicsDevice, 1, 1);
        emptyTexture.SetData([Color.Transparent]);
        _textures["Empty"] = emptyTexture;
    }

    private void InitializePlayer()
    {
        var spawningPos = World.GridToWorld(_world.GetSpawningPosForPlayer());
        _player = new Player(spawningPos, new Vector2(_charTex.Width, _charTex.Height));
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
            { MapColor = _defaultBackgroundColor };
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
    
    /// <summary>
    /// Draws to Render Target at native resolution
    /// </summary>
    private void MainRender()
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        DrawSkybox();

        var transformMatrixForRender =
            Matrix.CreateTranslation(-_camera.Position.X, -_camera.Position.Y, 0) * // Center view on camera's focus point
            Matrix.CreateScale(Settings.ResScale, Settings.ResScale, 1.0f) *          // Apply zoom
            Matrix.CreateTranslation(Settings.NativeWidth / 2f, Settings.NativeHeight / 2f, 0); // Translate to screen center
        
        _worldSpriteBatch.Begin(
            samplerState: SamplerState.PointClamp,   // No filtering, pixel art. Prevents blending at edges.
            blendState: BlendState.NonPremultiplied, // Allows transparent pixels to be drawn
            transformMatrix: transformMatrixForRender
        );
        
        DrawTiles();
        if (_debug)
        {
            DebugDrawCollidedTiles();
            DebugDrawCheckedTiles();
            DebugDrawPlayerHitBox();
        }
        
        _worldSpriteBatch.Draw( // draw player
            _charTex,
            _player.Position,
            null,
            Color.White,
            0f,
            Vector2.Zero,
            1f,
            _player.SpriteInverted ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
            0f
        );
        _worldSpriteBatch.End();
    }

    private void DrawSkybox()
    {
        _gradientSkyEffect.Parameters["ColorA"].SetValue(new Color(24, 101, 255).ToVector4());
        _gradientSkyEffect.Parameters["ColorB"].SetValue(new Color(132, 170, 248).ToVector4());
        
        _worldSpriteBatch.Begin(effect: _gradientSkyEffect);
        _worldSpriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, Settings.NativeWidth, Settings.NativeHeight), Color.White);
        _worldSpriteBatch.End();
    }

    /// <summary>
    /// Renders everything that falls under the camera's view frustrum
    /// </summary>
    private void DrawTiles()
    {
        // Calculate the camera's view frustum in world coordinates
        const float viewHalfWidthWorld = Settings.NativeWidth / 2f / Settings.ResScale;
        const float viewHalfHeightWorld = Settings.NativeHeight / 2f / Settings.ResScale;

        float viewLeftWorld = _camera.Position.X - viewHalfWidthWorld;
        float viewRightWorld = _camera.Position.X + viewHalfWidthWorld;
        float viewTopWorld = _camera.Position.Y - viewHalfHeightWorld;
        float viewBottomWorld = _camera.Position.Y + viewHalfHeightWorld;

        int startTileX = World.WorldToGrid(viewLeftWorld);
        int endTileX = World.WorldToGrid(viewRightWorld);
        int startTileY = World.WorldToGrid(viewTopWorld);
        int endTileY = World.WorldToGrid(viewBottomWorld);

        // Clamp tile indices to world boundaries
        startTileX = Math.Max(0, startTileX);
        endTileX = Math.Min(_world.GetWidth() - 1, endTileX); // Max valid index is Width - 1
        startTileY = Math.Max(0, startTileY);
        endTileY = Math.Min(_world.GetHeight() - 1, endTileY); // Max valid index is Height - 1

        // Iterate over the visible range of tiles (inclusive for end indices)
        for (int i = startTileX; i <= endTileX; i++)
        {
            for (int j = startTileY; j <= endTileY; j++)
            {
                Sprite2D sprite = _world.GetTileSpriteAt(i, j);
                DrawTile(i, j, sprite);
            }
        }
    }

    private void DrawUI()
    {
        _uiSpriteBatch.Begin();

        var selectedTileText = $"{_placeableTypes[_currType]}";
        var topLeftMargin = new Vector2(10, 10);
        _uiSpriteBatch.DrawString(
            _mainFont,
            selectedTileText,
            topLeftMargin,
            Color.LightGreen,
            0f,
            Vector2.Zero,
            0.5f,
            SpriteEffects.None,
            0.5f
        );

        var playerGridY = World.WorldToGrid(_player.Position.Y);
        var heightText = $"Height: {playerGridY}";
        var heightTextPosition = new Vector2(10, 10 + _mainFont.MeasureString(selectedTileText).Y * 0.5f + 5); // Position below the first text with a small margin

        _uiSpriteBatch.DrawString(
            _mainFont,
            heightText,
            heightTextPosition,
            Color.LightCyan,
            0f,
            Vector2.Zero,
            0.5f,
            SpriteEffects.None,
            0.5f
        );

        _uiSpriteBatch.End();
    }

    /// <summary>
    /// Draws Render Target scaled to window size
    /// </summary>
    private void ScaleResolution()
    {
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);
        _worldSpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _worldSpriteBatch.Draw(
            _renderTarget, 
            destinationRectangle: new Rectangle(0, 0, Settings.NativeWidth, Settings.NativeHeight),
            Color.White
        );
        _worldSpriteBatch.End();
    }
    
    private void DebugDrawPlayerHitBox()
    {
        _worldSpriteBatch.Draw(_debugTexture, new Rectangle(
                (int)_player.CollisionShape.Position.X, (int)_player.CollisionShape.Position.Y,
                (int)_player.CollisionShape.Size.X, (int)_player.CollisionShape.Size.Y)
            , _debugColor);
    }

    private void DebugDrawCollidedTiles()
    {
        foreach (var worldPos in _world.DebugCollidedTiles.Select(tilePos => new Vector2(tilePos.X, tilePos.Y) * Settings.TileSize))
        {
            _worldSpriteBatch.Draw(_debugTexture, worldPos, new Rectangle(0, 1, 8, 8), Color.Blue);
        }
    }
    
    private void DebugDrawCheckedTiles()
    {
        foreach (var worldPos in _world.DebugCheckedTiles.Select(tilePos => new Vector2(tilePos.X, tilePos.Y) * Settings.TileSize))
        {
            
            _worldSpriteBatch.Draw(_debugTexture, worldPos, new Rectangle(0, 1, 8, 8), Color.Yellow);
        }
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

    private void DrawMapMode()
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black);

        Matrix mapTransformMatrix =
            Matrix.CreateTranslation(-_mapCameraPosition.X / Settings.TileSize, -_mapCameraPosition.Y / Settings.TileSize, 0) * 
            Matrix.CreateScale(_mapCameraZoom) * 
            Matrix.CreateTranslation(Settings.NativeWidth / 2f, Settings.NativeHeight / 2f, 0); 

        _worldSpriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: mapTransformMatrix
        );


        // Calculate the camera's view frustum in Tile coordinates for the map
        // Convert screen size to tile space view dimensions based on current zoom.
        float viewHalfWidthInTiles = Settings.NativeWidth / (2f * _mapCameraZoom * MapTileRenderSize); 
        float viewHalfHeightInTiles = Settings.NativeHeight / (2f * _mapCameraZoom * MapTileRenderSize);
        
        Point mapCameraGridPos = World.WorldToGrid(_mapCameraPosition);

        int startTileX = (int)Math.Max(0, mapCameraGridPos.X - viewHalfWidthInTiles);
        int endTileX = (int)Math.Min(_world.GetWidth() -1, mapCameraGridPos.X + viewHalfWidthInTiles);
        int startTileY = (int)Math.Max(0, mapCameraGridPos.Y - viewHalfHeightInTiles);
        int endTileY = (int)Math.Min(_world.GetHeight() -1, mapCameraGridPos.Y + viewHalfHeightInTiles);
        
        for (int x = startTileX; x <= endTileX; x++)
        {
            for (int y = startTileY; y <= endTileY; y++)
            {
                TileType tileType = _world.GetTileTypeAt(x, y);
                if (tileType.Id != "EMPTY")
                {
                    _worldSpriteBatch.Draw(
                        _pixelTexture, // A 1x1 white texture
                        new Vector2(x, y), // Position in tile coordinates
                        null,
                        tileType.MapColor,
                        0f,
                        Vector2.Zero,
                        1f, // each tile is 1x1 "pixel" on the map
                        SpriteEffects.None,
                        0f
                    );
                }
            }
        }
        
        Vector2 playerMapPos = new Vector2(_player.GridPosition.X, _player.GridPosition.Y); // Get player's grid position
        _worldSpriteBatch.Draw(
            _pixelTexture,
            playerMapPos,
            null,
            Color.Red,
            0f,
            Vector2.Zero,
            1f,
            SpriteEffects.None,
            1f // Ensure player is drawn on top
         );

        _worldSpriteBatch.End();
        ScaleResolution();
    }
}
