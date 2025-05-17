namespace Catalyst.Graphics;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Sprite2D(Texture2D texture, Rectangle? sourceRect = null)
{
    public readonly Texture2D Texture = texture;
    public Rectangle? SourceRect { get; private set; } = sourceRect;
    
    /// <summary>
    /// Gets the size of the source rectangle or the full texture if no source is defined.
    /// </summary>
    public Point Size => SourceRect?.Size ?? new Point(Texture.Width, Texture.Height);

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
