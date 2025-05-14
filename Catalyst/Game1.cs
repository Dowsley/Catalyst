using System;
using System.IO;
using Catalyst.Core;
using Catalyst.Globals;
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
    
    private readonly Camera2D _camera = new();
    private const float CameraSpeed = 2.0f;

    private World _world = null;

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
        _world = new World(Settings.NativeWidth / Settings.TileSize, Settings.NativeHeight / Settings.TileSize);
        _world.GenerateTerrain();
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
        
        if (kState.IsKeyDown(Keys.Escape))
            Exit();

        if (kState.IsKeyDown(Keys.R))
        {
            _world.RandomizeSeed();
            _world.GenerateTerrain();
        }

        if (kState.IsKeyDown(Keys.Left))
            _camera.Position.X -= CameraSpeed;
        if (kState.IsKeyDown(Keys.Right))
            _camera.Position.X += CameraSpeed;
        if (kState.IsKeyDown(Keys.Up))
            _camera.Position.Y -= CameraSpeed;
        if (kState.IsKeyDown(Keys.Down))
            _camera.Position.Y += CameraSpeed;

        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        RenderAtNativeRes();
        RenderAtScaledRes();

        base.Draw(gameTime);
    }

    /* Draw to Render Target at native resolution */
    private void RenderAtNativeRes()
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin(transformMatrix: _camera.GetViewMatrix());
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
}
