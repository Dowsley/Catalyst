using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Catalyst.Core;
using Catalyst.Entities.Player;
using Catalyst.Globals;
using Catalyst.Graphics;
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
    private Dictionary<string, Texture2D> _textures = new();

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
    private readonly List<string> _placeableTypes = ["GRASS", "STONE", "OAK_LOG"];
    private int _currType = 0;
    private bool _canPressAgain = true;
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = Settings.NativeWidth;
        _graphics.PreferredBackBufferHeight = Settings.NativeHeight;
    }

    protected override void Initialize()
    {
        base.Initialize();
        _renderTarget = new RenderTarget2D(GraphicsDevice, Settings.NativeWidth, Settings.NativeHeight);
        
        InitializeTileTypes();
        _world = new World(
            Settings.NativeWidth / Settings.TileSize, 
            Settings.NativeHeight / Settings.TileSize,
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
        
        _debugTexture = new Texture2D(GraphicsDevice, 1, 1);
        _debugTexture.SetData([Color.White]);
        _charTex = Content.Load<Texture2D>("Graphics/sample_char");
        _mainFont = Content.Load<SpriteFont>("Fonts/Andy Bold");
    }

    protected override void Update(GameTime gameTime)
    {
        var kState = Keyboard.GetState();
        if (kState.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        if (kState.IsKeyDown(Keys.Tab) && _canPressAgain)
        {
            _canPressAgain = false;
            _currType += 1;
            if (_currType >= _placeableTypes.Count)
                _currType = 0;
        }
        if (kState.IsKeyUp(Keys.Tab))
            _canPressAgain = true;

        _world.Update(gameTime, kState); 
        _camera.Position = _player.Position + new Vector2(_charTex.Width / 2f, _charTex.Height / 2f);

        var mState = Mouse.GetState();
        var mouseScreenPos = new Vector2(mState.X, mState.Y);

        Matrix worldViewTransformMatrix =
            Matrix.CreateTranslation(-_camera.Position.X, -_camera.Position.Y, 0) * // Center on camera's focus (player's center)
            Matrix.CreateScale(Settings.ResScale, Settings.ResScale, 1.0f) *         // Apply zoom
            Matrix.CreateTranslation(Settings.NativeWidth / 2f, Settings.NativeHeight / 2f, 0); // Translate to screen center

        var lmbPressed = mState.LeftButton == ButtonState.Pressed;
        var rmbPressed = mState.RightButton == ButtonState.Pressed;
        if (lmbPressed || rmbPressed)
        {
            var worldPos = Vector2.Transform(mouseScreenPos, Matrix.Invert(worldViewTransformMatrix));
            var gridPos = _world.WorldToGrid(worldPos);
            _world.SetTileAt(gridPos.X, gridPos.Y, lmbPressed ? _tileRegistry.Get(_placeableTypes[_currType]) : _tileRegistry.Get("EMPTY"));
        }
        
        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        MainRender();
        ScaleResolution();
        DrawUI();

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
        var spawningPos = _world.GetSpawningPosForPlayer();
        spawningPos.Y -= 5 * Settings.TileSize; //offset in blocks
        _player = new Player(spawningPos, new Vector2(_charTex.Width, _charTex.Height));
        _camera = new Camera2D(_player.Position);
        
        _world.SetPlayer(_player);
    }

    private void InitializeTileTypes()
    {
        // TODO: Implement data-driven approach. All types should be XMLs, and loaded by a loader inside tileRegistry.
        var grassTileType = new TileType("GRASS", "Grass", "Just some grass", 100, true);
        grassTileType.AddSpriteVariant(new Sprite2D("Grass", new Rectangle(0, 1, Settings.TileSize, Settings.TileSize)));
        
        var stoneTileType = new TileType("STONE", "Stone", "Just stone", 500, true);
        stoneTileType.AddSpriteVariant(new Sprite2D("Stone", new Rectangle(0, 1, Settings.TileSize, Settings.TileSize)));
        
        var oakLogType = new TileType("OAK_LOG", "Oak Log", "Just oak log", 200, true);
        oakLogType.AddSpriteVariant(new Sprite2D("Oak Logs", new Rectangle(0, 1, Settings.TileSize, Settings.TileSize)));
        
        var emptyType = new TileType("EMPTY", "Empty", "Just air", 0, false);
        emptyType.AddSpriteVariant(new Sprite2D("Empty", new Rectangle(0, 0, Settings.TileSize, Settings.TileSize)));

        _tileRegistry.Register(grassTileType.Id, grassTileType);
        _tileRegistry.Register(stoneTileType.Id, stoneTileType);
        _tileRegistry.Register(oakLogType.Id, oakLogType);
        _tileRegistry.Register(emptyType.Id, emptyType);
    }

    /* Draws to Render Target at native resolution */
    private void MainRender()
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);

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
        
        _worldSpriteBatch.Draw(_charTex, _player.Position, Color.White);
        _worldSpriteBatch.End();
    }

    private void DrawTiles()
    {
        for (int i = 0; i < _world.GetWidth(); i++)
        {
            for (int j = 0; j < _world.GetHeight(); j++)
            {
                Sprite2D sprite = _world.GetTileSpriteAt(i, j);
                DrawTile(i, j, sprite);
            }
        }
    }

    private void DrawUI()
    {
        _uiSpriteBatch.Begin();

        var text = $"{_placeableTypes[_currType]}";
        var topLeftMargin = new Vector2(10, 10);
        _uiSpriteBatch.DrawString(
            _mainFont,
            text,
            topLeftMargin,
            Color.LightGreen,
            0f,
            Vector2.Zero,
            0.5f,
            SpriteEffects.None,
            0.5f
        );
        _uiSpriteBatch.End();
    }

    /* Draws Render Target scaled to window size */
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
                (int)_player.CollisionShape.Position.X, (int)_player.CollisionShape.Position.Y, (int)_player.CollisionShape.Size.X, (int)_player.CollisionShape.Size.Y)
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
}
