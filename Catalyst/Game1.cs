using System.Collections.Generic;
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
    private SpriteBatch _spriteBatch = null!;
    private RenderTarget2D _renderTarget = null!;
    private Texture2D _charTex = null!;

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

    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = Settings.NativeWidth * Settings.ResScale;
        _graphics.PreferredBackBufferHeight = Settings.NativeHeight * Settings.ResScale;
    }

    protected override void Initialize()
    {
        base.Initialize();
        _renderTarget = new RenderTarget2D(GraphicsDevice, Settings.NativeWidth, Settings.NativeHeight);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _debugTexture = new Texture2D(GraphicsDevice, 1, 1);
        _debugTexture.SetData([Color.White]);
        _charTex = Content.Load<Texture2D>("Graphics/sample_char");

        InitializeTileTypes();
        
        _world = new World(
            Settings.NativeWidth / Settings.TileSize, 
            Settings.NativeHeight / Settings.TileSize,
            _tileRegistry,
            _debug
        );
        _world.GenerateTerrain();
        
        InitializePlayer();
        _world.SetPlayer(_player);
    }

    protected override void Update(GameTime gameTime)
    {
        var kState = Keyboard.GetState();
        var mState = Mouse.GetState();
        
        if (kState.IsKeyDown(Keys.Escape))
            Exit();
        if (kState.IsKeyDown(Keys.Tab))
        {
            _currType += 1;
            if (_currType >= _placeableTypes.Count)
                _currType = 0;
        }

        var lmbPressed = mState.LeftButton == ButtonState.Pressed;
        var rmbPressed = mState.RightButton == ButtonState.Pressed;
        if (lmbPressed || rmbPressed)
        {
            var mousePos = new Vector2(mState.X, mState.Y);
            var worldPos = mousePos / Settings.ResScale + _camera.Position;
            var gridPos = _world.WorldToGrid(worldPos);
            _world.SetTileAt(gridPos.X, gridPos.Y, lmbPressed ? _tileRegistry.Get(_placeableTypes[_currType]) : _tileRegistry.Get("EMPTY"));
        }

        _world.Update(gameTime, kState);
        _camera.Position = _player.Position - new Vector2(Settings.NativeWidth, Settings.NativeHeight) / 2f;
        
        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        RenderAtNativeRes();
        RenderAtScaledRes();

        base.Draw(gameTime);
    }

    private void DrawTile(int x, int y, Sprite2D sprite)
    {
        _spriteBatch.Draw(
            sprite.Texture,
            new Vector2(x, y) * Settings.TileSize,
            sprite.SourceRect,
            Color.White
        );
    }

    private void InitializePlayer()
    {
        var spawningPos = _world.GetSpawningPosForPlayer();
        spawningPos.Y -= 5 * Settings.TileSize; //offset in blocks
        _player = new Player(spawningPos, new Vector2(_charTex.Width, _charTex.Height));
        _camera = new Camera2D(_player.Position);
    }

    private void InitializeTileTypes()
    {
        var dirtTexture = Content.Load<Texture2D>("Graphics/Pack2/Tiles/Grass");
        var stoneTexture = Content.Load<Texture2D>("Graphics/Pack2/Tiles/Stone");
        var oakLogTexture = Content.Load<Texture2D>("Graphics/Pack2/Tiles/Oak Logs");
        var emptyTexture = new Texture2D(GraphicsDevice, 1, 1);
        emptyTexture.SetData([Color.Transparent]);
        
        // TODO: Implement data-driven approach. All types should be XMLs, and loaded by a loader inside tileRegistry.
        var grassTileType = new TileType("GRASS", "Grass", "Just some grass", 100, true);
        grassTileType.AddSpriteVariant(new Sprite2D(dirtTexture, new Rectangle(0, 1, Settings.TileSize, Settings.TileSize)));
        
        var stoneTileType = new TileType("STONE", "Stone", "Just stone", 500, true);
        stoneTileType.AddSpriteVariant(new Sprite2D(stoneTexture, new Rectangle(0, 1, Settings.TileSize, Settings.TileSize)));
        
        var oakLogType = new TileType("OAK_LOG", "Oak Log", "Just oak log", 200, true);
        oakLogType.AddSpriteVariant(new Sprite2D(oakLogTexture, new Rectangle(0, 1, Settings.TileSize, Settings.TileSize)));
        
        var emptyType = new TileType("EMPTY", "Empty", "Just air", 0, false);
        emptyType.AddSpriteVariant(new Sprite2D(emptyTexture, new Rectangle(0, 0, Settings.TileSize, Settings.TileSize)));
        
        

        _tileRegistry.Register(grassTileType.Id, grassTileType);
        _tileRegistry.Register(stoneTileType.Id, stoneTileType);
        _tileRegistry.Register(oakLogType.Id, oakLogType);
        _tileRegistry.Register(emptyType.Id, emptyType);
    }

    /* Draw to Render Target at native resolution */
    private void RenderAtNativeRes()
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,   // No filtering, pixel art. Prevents blending at edges.
            blendState: BlendState.NonPremultiplied, // Allows transparent pixels to be drawn
            transformMatrix: _camera.GetViewMatrix()
        );
        for (int i = 0; i < _world.GetWidth(); i++)
        {
            for (int j = 0; j < _world.GetHeight(); j++)
            {
                Sprite2D sprite = _world.GetTileSpriteAt(i, j);
                DrawTile(i, j, sprite);
            }
        }

        if (_debug)
        {
            DebugDrawCollidedTiles();
            DebugDrawCheckedTiles();
        }

        _spriteBatch.Draw(_charTex, _player.Position, Color.White);
        
        if (_debug)
        {
            _spriteBatch.Draw(_debugTexture, new Rectangle(
                (int)_player.CollisionShape.Position.X, (int)_player.CollisionShape.Position.Y, (int)_player.CollisionShape.Size.X, (int)_player.CollisionShape.Size.Y)
            , _debugColor);
        }
        _spriteBatch.End();
    }

    /* Draws Render Target scaled to window size */
    private void RenderAtScaledRes()
    {
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(
            _renderTarget, destinationRectangle: new Rectangle(
                0, 0, Settings.NativeWidth * Settings.ResScale, Settings.NativeHeight * Settings.ResScale), Color.White);
        _spriteBatch.End();
    }

    private void DebugDrawCollidedTiles()
    {
        foreach (var worldPos in _world.DebugCollidedTiles.Select(tilePos => new Vector2(tilePos.X, tilePos.Y) * Settings.TileSize))
        {
            _spriteBatch.Draw(_debugTexture, worldPos, new Rectangle(0, 1, 8, 8), Color.Blue);
        }
    }
    
    private void DebugDrawCheckedTiles()
    {
        foreach (var worldPos in _world.DebugCheckedTiles.Select(tilePos => new Vector2(tilePos.X, tilePos.Y) * Settings.TileSize))
        {
            
            _spriteBatch.Draw(_debugTexture, worldPos, new Rectangle(0, 1, 8, 8), Color.Yellow);
        }
    }
}
