using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Catalyst.Core;
using Catalyst.Entities;
using Catalyst.Entities.Player;
using Catalyst.Globals;
using Catalyst.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Catalyst;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private RenderTarget2D _renderTarget;

    private Texture2D _charTex;
    private Texture2D _dirtTexAtlas;
    private readonly Rectangle _dirtCommonRect = new(0, 1, 8, 8);

    private Camera2D _camera;
    private World _world;
    private Player _player;

    private bool _debug = false;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = Settings.NativeWidth * Settings.ResScale;
        _graphics.PreferredBackBufferHeight = Settings.NativeHeight * Settings.ResScale;
        
        _world = new World(Settings.NativeWidth / Settings.TileSize, Settings.NativeHeight / Settings.TileSize, _debug);
        _world.GenerateTerrain();
    }

    protected override void Initialize()
    {
        base.Initialize();

        InitializePlayer();
        _world.SetPlayer(_player);
        _renderTarget = new RenderTarget2D(GraphicsDevice, Settings.NativeWidth, Settings.NativeHeight);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _dirtTexAtlas = Content.Load<Texture2D>("Graphics/Pack2/Tiles/Grass");
        _charTex = Content.Load<Texture2D>("Graphics/sample_char");
    }

    protected override void Update(GameTime gameTime)
    {
        var kState = Keyboard.GetState();
        var mState = Mouse.GetState();
        
        if (kState.IsKeyDown(Keys.Escape))
            Exit();

        var lmbPressed = mState.LeftButton == ButtonState.Pressed;
        var rmbPressed = mState.RightButton == ButtonState.Pressed;
        if (lmbPressed || rmbPressed)
        {
            var mousePos = new Vector2(mState.X, mState.Y);
            var worldPos = mousePos / Settings.ResScale + _camera.Position;
            var gridPos = _world.WorldToGrid(worldPos);
            _world.SetTileAt(gridPos.X, gridPos.Y, lmbPressed);
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

    private void InitializePlayer()
    {
        var spawningPos = _world.GetSpawningPosForPlayer();
        spawningPos.Y -= 5 * Settings.TileSize; //offset in blocks
        _player = new Player(spawningPos, new Vector2(_charTex.Width, _charTex.Height));
        _camera = new Camera2D(_player.Position);
    }

    /* Draw to Render Target at native resolution */
    private void RenderAtNativeRes()
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin(blendState: BlendState.NonPremultiplied, transformMatrix: _camera.GetViewMatrix());
        for (int i = 0; i < _world.GetWidth(); i++)
        {
            for (int j = 0; j < _world.GetHeight(); j++)
            {
                if (!_world.GetTileAt(i, j) )
                    continue;
                var worldPos = new Vector2(i, j) * Settings.TileSize;
                _spriteBatch.Draw(_dirtTexAtlas, worldPos, _dirtCommonRect, Color.White);
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
            var debugColor = new Color(255, 255, 255, 255/2);
            var debugTexture = new Texture2D(GraphicsDevice, 1, 1);
            debugTexture.SetData([Color.White]);
            _spriteBatch.Draw(debugTexture, new Rectangle(
                (int)_player.CollisionShape.Position.X, (int)_player.CollisionShape.Position.Y, (int)_player.CollisionShape.Size.X, (int)_player.CollisionShape.Size.Y)
            , debugColor);
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
            _spriteBatch.Draw(_dirtTexAtlas, worldPos, _dirtCommonRect, Color.Blue);
        }
    }
    
    private void DebugDrawCheckedTiles()
    {
        foreach (var worldPos in _world.DebugCheckedTiles.Select(tilePos => new Vector2(tilePos.X, tilePos.Y) * Settings.TileSize))
        {
            _spriteBatch.Draw(_dirtTexAtlas, worldPos, _dirtCommonRect, Color.Yellow);
        }
    }
}
