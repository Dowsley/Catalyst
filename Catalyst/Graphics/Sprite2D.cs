namespace Catalyst.Graphics;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Sprite2D(string textureId, Rectangle sourceRect)
{
    public readonly string TextureId = textureId;
    public Rectangle SourceRect { get; private set; } = sourceRect;
    
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
