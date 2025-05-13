using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Catalyst;

public class Camera2D
{
    public Vector2 Position = Vector2.Zero;
    public float Zoom = 1f;
    public float Rotation = 0f;

    public Matrix GetViewMatrix()
    {
        return
            Matrix.CreateTranslation(new Vector3(-Position, 0f)) *
            Matrix.CreateRotationZ(Rotation) *
            Matrix.CreateScale(Zoom, Zoom, 1f);
    }
}

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _dirtTexAtlas;
    private readonly Rectangle _dirtCommonRect = new(0, 1, 8, 8);
    private Texture2D _charTex;
    private readonly Camera2D _camera = new();
    private const float CameraSpeed = 2.0f;

    private const int TileSize = 8;
    private bool[,] _tiles;

    private const int Scale = 3;

    private const int NativeWidth = 320*2;
    private const int NativeHeight = 180*2;

    private RenderTarget2D _renderTarget;
    private FastNoiseLite _noise;
    private int _seed = 0;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Set *window size* to scaled resolution
        _graphics.PreferredBackBufferWidth = NativeWidth * Scale;
        _graphics.PreferredBackBufferHeight = NativeHeight * Scale;
    }

    protected override void Initialize()
    {
        base.Initialize();
        
        _noise = new FastNoiseLite();
        _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _noise.SetFrequency(0.05f);
        _noise.SetSeed(_seed);
        
        _renderTarget = new RenderTarget2D(GraphicsDevice, NativeWidth, NativeHeight);
        _tiles = new bool[NativeWidth / TileSize, NativeHeight / TileSize];

        GenerateTerrain();
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
            var random = new Random();
            _seed = random.Next();
            _noise.SetSeed(_seed);
            GenerateTerrain();
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
    
    private void GenerateTerrain()
    {
        const int tilesWide = NativeWidth / TileSize;
        const int tilesHigh = NativeHeight / TileSize;

        for (int x = 0; x < tilesWide; x++)
        {
            float noiseValue = _noise.GetNoise(x, 0);
            int surfaceY = (int)(tilesHigh / 2 + noiseValue * 10);

            for (int y = 0; y < tilesHigh; y++)
                _tiles[x, y] = y > surfaceY;
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        /* Draw to Render Target at native resolution */
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin(transformMatrix: _camera.GetViewMatrix());
        for (int i = 0; i < NativeWidth / TileSize; i++)
        {
            for (int j = 0; j < NativeHeight / TileSize; j++)
            {
                if (_tiles[i, j] == false)
                    continue;
                var worldPos = new Vector2(i * TileSize, j * TileSize);
                _spriteBatch.Draw(_dirtTexAtlas, worldPos, _dirtCommonRect, Color.White);
            }
        }
        _spriteBatch.End();

        /* Draw Render Target scaled to window size */
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_renderTarget, destinationRectangle: new Rectangle(0, 0, NativeWidth * Scale, NativeHeight * Scale), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
