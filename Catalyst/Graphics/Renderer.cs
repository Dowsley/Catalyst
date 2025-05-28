using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Catalyst.Core;
using Catalyst.Entities.Player;
using Catalyst.Globals;
using Catalyst.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Catalyst.Graphics;

public class Renderer(GraphicsDevice graphicsDevice, ContentManager content)
{
    private SpriteBatch _worldSpriteBatch = null!;
    private SpriteBatch _uiSpriteBatch = null!;
    private RenderTarget2D _renderTarget = null!;
    private SpriteFont _mainFont = null!;
    private Texture2D _charTex = null!;
    private readonly Dictionary<string, Texture2D> _textures = new();
    private Effect _gradientSkyEffect = null!;
    private const float WallDarkeningFactor = 0.5f;

    private readonly Sprite _defaultCloudSprite = new("Cloud Background", new Rectangle(0, 0, 128, 64)); 

    private readonly Color _debugColor = new(255, 255, 255, 255/2);
    private Texture2D _debugTexture = null!;
    private Texture2D _pixelTexture = null!;
    
    private const float MapTileRenderSize = 1f;

    // Cloud system fields
    private readonly Random _random = new();
    private record CloudData(Vector2 Position, Vector2 Velocity);
    private readonly List<CloudData> _activeClouds = [];
    private const int MaxCloudsOnScreen = 5;
    private const float AttemptCloudSpawnIntervalSecs = 15f; 
    private const float MinCloudSpeedX = 10f; // pixels/sec
    private const float MaxCloudSpeedX = 20f; // pixels/sec
    private float _timeSinceLastCloudSpawn = 0f;

    public Texture2D CharacterTexture => _charTex;

    public void LoadContent()
    {
        _worldSpriteBatch = new SpriteBatch(graphicsDevice);
        _uiSpriteBatch = new SpriteBatch(graphicsDevice);
        _renderTarget = new RenderTarget2D(graphicsDevice, Settings.NativeWidth, Settings.NativeHeight);
        
        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        _pixelTexture.SetData([Color.White]);

        LoadTextures();
        _gradientSkyEffect = content.Load<Effect>("Shaders/GradientEffect");
        
        _debugTexture = new Texture2D(graphicsDevice, 1, 1);
        _debugTexture.SetData([Color.White]);
        _charTex = content.Load<Texture2D>("Graphics/sample_char");
        _mainFont = content.Load<SpriteFont>("Fonts/Andy Bold");
    }

    private void LoadTextures()
    {
        string graphicsPath = Path.Combine(content.RootDirectory, "Graphics/Pack2");
        if (!Directory.Exists(graphicsPath))
        {
            throw new Exception($"Directory not found: {graphicsPath}");
        }

        var textureFiles = Directory.GetFiles(graphicsPath, "*.png", SearchOption.AllDirectories);

        foreach (var filePath in textureFiles)
        {
            using var stream = File.OpenRead(filePath);
            var texture = Texture2D.FromStream(graphicsDevice, stream);
            var textureKey = Path.GetFileNameWithoutExtension(filePath);
            _textures[textureKey] = texture;
        }
        
        var emptyTexture = new Texture2D(graphicsDevice, 1, 1);
        emptyTexture.SetData([Color.Transparent]);
        _textures["Empty"] = emptyTexture;
    }

    private Texture2D LookupTexture(string texId)
    {
        return _textures[texId];
    }

    public void Draw(
        GameTime gameTime, 
        bool isMapOpen, 
        Camera camera, 
        World world, 
        Player player, 
        bool debug, 
        Vector2 mapCameraPosition, 
        float mapCameraZoom,
        IReadOnlyList<string> placeableTypes,
        int currentPlaceableTypeIndex,
        bool toggleWallMode
    )
    {
        if (isMapOpen)
        {
            DrawMapMode(world, player, mapCameraPosition, mapCameraZoom);
        }
        else
        {
            MainRender(camera, world, player, debug, gameTime);
            ScaleResolution();
            DrawUI(player, world, placeableTypes, currentPlaceableTypeIndex, toggleWallMode);
        }
    }
    
    private void DrawTile(World world, int x, int y)
    {
        var attemptDrawingWall = false;
        var tile = world.GetTileAt(x, y);
        if (tile.Type.Id == "EMPTY")
        {
            attemptDrawingWall = true;
        }
        var sprite = attemptDrawingWall ? tile.WallSprite : tile.Sprite;
        float lightValue = world.GetLightValueForRenderingAt(x, y);
        var modulate = sprite.Modulate;
        var factor = lightValue * (attemptDrawingWall ? WallDarkeningFactor : 1f);
        Color tileColor = new Color(
            modulate.R / 255f * factor,
            modulate.G / 255f * factor,
            modulate.B / 255f * factor
        );
        DrawSprite(sprite, new Vector2(x, y) * Settings.TileSize, tileColor); // normal type
    }

    private void DrawSprite(Sprite sprite, Vector2 pos, Color? color = null)
    {
        Texture2D tex = LookupTexture(sprite.TextureId);
        _worldSpriteBatch.Draw(
            tex,
            pos,
            sprite.SourceRect,
            color ?? Color.White
        );
    }

    /// <summary>
    /// Draws to Render Target at native resolution
    /// </summary>
    private void MainRender(Camera camera, World world, Player player, bool debug, GameTime gameTime)
    {
        UpdateClouds(gameTime);

        graphicsDevice.SetRenderTarget(_renderTarget);
        DrawSkybox();

        var transformMatrixForRender =
            Matrix.CreateTranslation(-camera.Position.X, -camera.Position.Y, 0) *
            Matrix.CreateScale(Settings.ResScale, Settings.ResScale, 1.0f) *
            Matrix.CreateTranslation(Settings.NativeWidth / 2f, Settings.NativeHeight / 2f, 0); 
        
        _worldSpriteBatch.Begin(
            samplerState: SamplerState.PointClamp,      // No filtering, pixel art. Prevents blending at edges.
            blendState: BlendState.NonPremultiplied,    // Allows transparent pixels to be drawn
            transformMatrix: transformMatrixForRender
        );
        
        DrawTiles(camera, world);
        if (debug)
        {
            DebugDrawCollidedTiles(world);
            DebugDrawCheckedTiles(world);
            DebugDrawPlayerHitBox(player);
        }

        var playerPosTile = World.WorldToGrid(player.Position);
        float lightValue = world.GetLightValueForRenderingAt(playerPosTile.X, playerPosTile.Y); // TODO: Average out the light value from the blocks the player occupies instead
        Color playerColor = new Color(
            Color.White.R / 255f * lightValue,
            Color.White.G / 255f * lightValue,
            Color.White.B / 255f * lightValue
        );
        _worldSpriteBatch.Draw( // draw player
            _charTex,
            player.Position,
            null,
            playerColor,
            0f,
            Vector2.Zero,
            1f,
            player.SpriteInverted ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
            0f
        );
        _worldSpriteBatch.End();
    }

    private void DrawSkybox()
    {
        DrawSkyGradient();
        DrawClouds();
    }

    private void DrawSkyGradient()
    {
        _gradientSkyEffect.Parameters["ColorA"].SetValue(new Color(24, 101, 255).ToVector4());
        _gradientSkyEffect.Parameters["ColorB"].SetValue(new Color(132, 170, 248).ToVector4());
        
        _worldSpriteBatch.Begin(effect: _gradientSkyEffect, samplerState: SamplerState.PointClamp);
        _worldSpriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, Settings.NativeWidth, Settings.NativeHeight), Color.White);
        _worldSpriteBatch.End();
    }
    
    private void DrawClouds()
    {
        _worldSpriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            blendState: BlendState.NonPremultiplied
        );

        Texture2D cloudTexture = LookupTexture(_defaultCloudSprite.TextureId);
        foreach (var cloud in _activeClouds)
        {
            _worldSpriteBatch.Draw(
                cloudTexture,
                cloud.Position,
                _defaultCloudSprite.SourceRect,
                new Color(255, 255, 255, 255/3) // 1/3rd transparency
            );
        }
        _worldSpriteBatch.End();
    }
    
    /// <summary>
    /// Renders everything that falls under the camera's view frustrum
    /// </summary>
    private void DrawTiles(Camera camera, World world)
    {
        const float viewHalfWidthWorld = Settings.NativeWidth / 2f / Settings.ResScale;
        const float viewHalfHeightWorld = Settings.NativeHeight / 2f / Settings.ResScale;

        float viewLeftWorld = camera.Position.X - viewHalfWidthWorld;
        float viewRightWorld = camera.Position.X + viewHalfWidthWorld;
        float viewTopWorld = camera.Position.Y - viewHalfHeightWorld;
        float viewBottomWorld = camera.Position.Y + viewHalfHeightWorld;

        int startTileX = World.WorldToGrid(viewLeftWorld);
        int endTileX = World.WorldToGrid(viewRightWorld);
        int startTileY = World.WorldToGrid(viewTopWorld);
        int endTileY = World.WorldToGrid(viewBottomWorld);

        startTileX = Math.Max(0, startTileX);
        endTileX = Math.Min(world.GetWidth() - 1, endTileX);
        startTileY = Math.Max(0, startTileY);
        endTileY = Math.Min(world.GetHeight() - 1, endTileY);

        for (int i = startTileX; i <= endTileX; i++)
        {
            for (int j = startTileY; j <= endTileY; j++)
            {
                DrawTile(world, i, j);
            }
        }
    }

    // TODO: Move this to Game1
    private void UpdateClouds(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _timeSinceLastCloudSpawn += deltaTime;
        if (_activeClouds.Count < MaxCloudsOnScreen && _timeSinceLastCloudSpawn >= AttemptCloudSpawnIntervalSecs)
        {
            _timeSinceLastCloudSpawn = 0f;

            // Spawn clouds in the top 1/3 of the screen
            float spawnY = (float)_random.NextDouble() * (Settings.NativeHeight / 3f);
            float speedX = MinCloudSpeedX + (float)_random.NextDouble() * (MaxCloudSpeedX - MinCloudSpeedX);
            
            float spawnX;
            if (_random.Next(2) == 0) 
            {
                spawnX = -_defaultCloudSprite.SourceRect.Width;
            }
            else
            {
                spawnX = Settings.NativeWidth;
                speedX *= -1;
            }
            
            _activeClouds.Add(new CloudData(new Vector2(spawnX, spawnY), new Vector2(speedX, 0)));
        }

        for (int i = _activeClouds.Count - 1; i >= 0; i--)
        {
            var cloud = _activeClouds[i];
            var newPosition = cloud.Position + cloud.Velocity * deltaTime;
            _activeClouds[i] = cloud with { Position = newPosition };

            bool isOffScreenLeft = newPosition.X + _defaultCloudSprite.SourceRect.Width < 0;
            bool isOffScreenRight = newPosition.X > Settings.NativeWidth;
            if ((cloud.Velocity.X < 0 && isOffScreenLeft) || (cloud.Velocity.X > 0 && isOffScreenRight))
            {
                _activeClouds.RemoveAt(i);
            }
        }
    }

    private void DrawUI(Player player, World world, IReadOnlyList<string> placeableTypes, int currentPlaceableTypeIndex, bool toggleWallMode)
    {
        _uiSpriteBatch.Begin();

        var selectedTileText = $"{placeableTypes[currentPlaceableTypeIndex]}";
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

        var playerGridY = World.WorldToGrid(player.Position.Y);
        var heightText = $"Height: {playerGridY}";
        var heightTextPosition = new Vector2(10, 10 + _mainFont.MeasureString(selectedTileText).Y * 0.5f + 5);
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

        var mode = toggleWallMode ? "Wall" : "Tile";
        var modeText = $"Mode: {mode}";
        var modeTextPosition = new Vector2(10, 10 + _mainFont.MeasureString(modeText).Y + 5);
        _uiSpriteBatch.DrawString(
            _mainFont,
            modeText,
            modeTextPosition,
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
        graphicsDevice.SetRenderTarget(null);
        graphicsDevice.Clear(Color.Black);
        _worldSpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _worldSpriteBatch.Draw(
            _renderTarget, 
            destinationRectangle: new Rectangle(0, 0, Settings.NativeWidth, Settings.NativeHeight),
            Color.White
        );
        _worldSpriteBatch.End();
    }
    
    private void DebugDrawPlayerHitBox(Player player)
    {
        _worldSpriteBatch.Draw(_debugTexture, new Rectangle(
                (int)player.CollisionShape.Position.X, (int)player.CollisionShape.Position.Y,
                (int)player.CollisionShape.Size.X, (int)player.CollisionShape.Size.Y)
            , _debugColor);
    }

    private void DebugDrawCollidedTiles(World world)
    {
        foreach (var worldPos in world.DebugCollidedTiles.Select(tilePos => new Vector2(tilePos.X, tilePos.Y) * Settings.TileSize))
        {
            _worldSpriteBatch.Draw(_debugTexture, worldPos, new Rectangle(0, 1, 8, 8), Color.Blue);
        }
    }
    
    private void DebugDrawCheckedTiles(World world)
    {
        foreach (var worldPos in world.DebugCheckedTiles.Select(tilePos => new Vector2(tilePos.X, tilePos.Y) * Settings.TileSize))
        {
            _worldSpriteBatch.Draw(_debugTexture, worldPos, new Rectangle(0, 1, 8, 8), Color.Yellow);
        }
    }

    private void DrawMapMode(World world, Player player, Vector2 mapCameraPosition, float mapCameraZoom)
    {
        graphicsDevice.SetRenderTarget(_renderTarget);
        graphicsDevice.Clear(Color.Black);

        Matrix mapTransformMatrix =
            Matrix.CreateTranslation(-mapCameraPosition.X / Settings.TileSize, -mapCameraPosition.Y / Settings.TileSize, 0) * 
            Matrix.CreateScale(mapCameraZoom) * 
            Matrix.CreateTranslation(Settings.NativeWidth / 2f, Settings.NativeHeight / 2f, 0); 

        _worldSpriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: mapTransformMatrix
        );

        float viewHalfWidthInTiles = Settings.NativeWidth / (2f * mapCameraZoom * MapTileRenderSize); 
        float viewHalfHeightInTiles = Settings.NativeHeight / (2f * mapCameraZoom * MapTileRenderSize);
        
        Point mapCameraGridPos = World.WorldToGrid(mapCameraPosition);

        int startTileX = (int)Math.Max(0, mapCameraGridPos.X - viewHalfWidthInTiles);
        int endTileX = (int)Math.Min(world.GetWidth() -1, mapCameraGridPos.X + viewHalfWidthInTiles);
        int startTileY = (int)Math.Max(0, mapCameraGridPos.Y - viewHalfHeightInTiles);
        int endTileY = (int)Math.Min(world.GetHeight() -1, mapCameraGridPos.Y + viewHalfHeightInTiles);
        
        for (int x = startTileX; x <= endTileX; x++)
        {
            for (int y = startTileY; y <= endTileY; y++)
            {
                TileType tileType = world.GetTileTypeAt(x, y);
                if (tileType.Id != "EMPTY")
                {
                    _worldSpriteBatch.Draw(
                        _pixelTexture,
                        new Vector2(x, y),
                        null,
                        tileType.MapColor,
                        0f,
                        Vector2.Zero,
                        1f, 
                        SpriteEffects.None,
                        0f
                    );
                }
            }
        }
        
        Vector2 playerMapPos = new Vector2(player.GridPosition.X, player.GridPosition.Y);
        _worldSpriteBatch.Draw(
            _pixelTexture,
            playerMapPos,
            null,
            Color.Red,
            0f,
            Vector2.Zero,
            1f, 
            SpriteEffects.None,
            1f
        );

        _worldSpriteBatch.End();
        ScaleResolution(); // Common step for both modes
    }
}