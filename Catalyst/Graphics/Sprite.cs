using Microsoft.Xna.Framework;

namespace Catalyst.Graphics;

public class Sprite(string textureId, Rectangle sourceRect, Color? modulate = null)
{
    public readonly string TextureId = textureId;
    public Rectangle SourceRect { get; private set; } = sourceRect;
    public Color Modulate = modulate ?? Color.White;
    
    /// <summary>
    /// Gets the size of the source rectangle
    /// </summary>
    public Point Size => SourceRect.Size;

    /// <summary>
    /// Sets the source rectangle using pixel units.
    /// </summary>
    public void SetSourceRect(int x, int y, int width, int height)
    {
        SourceRect = new Rectangle(x, y, width, height);
    }

    /// <summary>
    /// Sets the source rectangle using grid units (tile-based), multiplied by <paramref name="tileSize"/>.
    /// </summary>
    public void SetSourceRect(int gridX, int gridY, int gridWidth, int gridHeight, int tileSize)
    {
        SourceRect = new Rectangle(
            gridX * tileSize,
            gridY * tileSize,
            gridWidth * tileSize,
            gridHeight * tileSize
        );
    }
}
