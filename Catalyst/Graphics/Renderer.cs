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

    private readonly Color _debugColor = new(255, 255, 255, 255/2);
    private Texture2D _debugTexture = null!;
    private Texture2D _pixelTexture = null!;
    
    private const float MapTileRenderSize = 1f;

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
        Camera2D camera, 
        World world, 
        Player player, 
        bool debug, 
        Vector2 mapCameraPosition, 
        float mapCameraZoom,
        IReadOnlyList<string> placeableTypes,
        int currentPlaceableTypeIndex)
    {
        if (isMapOpen)
        {
            DrawMapMode(world, player, mapCameraPosition, mapCameraZoom);
        }
        else
        {
            MainRender(camera, world, player, debug);
            ScaleResolution();
            DrawUI(player, world, placeableTypes, currentPlaceableTypeIndex);
        }
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

    /// <summary>
    /// Draws to Render Target at native resolution
    /// </summary>
    private void MainRender(Camera2D camera, World world, Player player, bool debug)
    {
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
        
        _worldSpriteBatch.Draw(
            _charTex,
            player.Position,
            null,
            Color.White,
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
        _gradientSkyEffect.Parameters["ColorA"].SetValue(new Color(24, 101, 255).ToVector4());
        _gradientSkyEffect.Parameters["ColorB"].SetValue(new Color(132, 170, 248).ToVector4());
        
        _worldSpriteBatch.Begin(effect: _gradientSkyEffect);
        _worldSpriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, Settings.NativeWidth, Settings.NativeHeight), Color.White);
        _worldSpriteBatch.End();
    }

    /// <summary>
    /// Renders everything that falls under the camera's view frustrum
    /// </summary>
    private void DrawTiles(Camera2D camera, World world)
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
                Sprite2D sprite = world.GetTileSpriteAt(i, j);
                DrawTile(i, j, sprite);
            }
        }
    }

    private void DrawUI(Player player, World world, IReadOnlyList<string> placeableTypes, int currentPlaceableTypeIndex)
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
                if (tileType.Id != "EMPTY") // Use the default background color for empty tiles
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