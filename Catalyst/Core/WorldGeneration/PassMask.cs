using Microsoft.Xna.Framework;

namespace Catalyst.Core.WorldGeneration;

/// <summary>
/// Defines what portions of a pass are allowed to be applied.
/// </summary>
public abstract class PassMask(Point size)
{
    protected bool[,] Allowed = new bool[size.X, size.Y]; // TODO: Consider making this store values between 0.0 and 1.0 (so we can add to noise values maybe?)
    protected readonly Point Size = size; // Store size for bounds checking

    public bool IsAllowed(Point pos)
    {
        return IsAllowed(pos.X, pos.Y);
    }

    public bool IsAllowed(int x, int y)
    {
        // Add bounds checking here
        if (x < 0 || x >= Size.X || y < 0 || y >= Size.Y)
        {
            return false;
        }
        return Allowed[x, y];
    }
}