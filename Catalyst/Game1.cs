using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Catalyst;

public class Game1 : Game
{
    private struct Ball(Vector2 position, float speed, Texture2D texture, Color color)
    {
        public Texture2D Texture = texture;
        public Vector2 Position = position;
        public float Speed = speed;
        public Color Color = color;
    }
    
    private Texture2D _mainBallTexture;
    private Ball _ball;
    
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
        _ball = new Ball(
            new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2),
            100.0f, _mainBallTexture, Color.White);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _mainBallTexture = Content.Load<Texture2D>("Graphics/ball");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        float updatedBallSpeed = _ball.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

        var kState = Keyboard.GetState();
        
        if (kState.IsKeyDown(Keys.W))
        {
            _ball.Position.Y -= updatedBallSpeed;
        }
        
        if (kState.IsKeyDown(Keys.S))
        {
            _ball.Position.Y += updatedBallSpeed;
        }
        
        if (kState.IsKeyDown(Keys.A))
        {
            _ball.Position.X -= updatedBallSpeed;
        }
        
        if (kState.IsKeyDown(Keys.D))
        {
            _ball.Position.X += updatedBallSpeed;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        _spriteBatch.Draw(
            _ball.Texture,
            _ball.Position,
            null,
            Color.White,
            0f,
            new Vector2(_ball.Texture.Width / 2, _ball.Texture.Height / 2),
            Vector2.One,
            SpriteEffects.None,
            0f
        );
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}