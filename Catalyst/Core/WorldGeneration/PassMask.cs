using Microsoft.Xna.Framework;

namespace Catalyst.Core.WorldGeneration;

/// <summary>
/// Defines what portions of a pass are allowed to be applied.
/// </summary>
public abstract class PassMask(Point size)
{
    protected float[,] Allowed = new float[size.X, size.Y];
    protected readonly Point Size = size;

    public bool IsAllowed(Point pos)
    {
        return IsAllowed(pos.X, pos.Y);
    }

    public bool IsAllowed(int x, int y)
    {
        if (!IsInBounds(x, y))
        {
            return false;
        }
        return Allowed[x, y] > 0.0f;
    }

    public float GetValue(int x, int y)
    {
        if (!IsInBounds(x, y))
        {
            return 0.0f;
        }
        return Allowed[x, y];
    }

    private bool IsInBounds(int x, int y)
    {
        return !(x < 0 || x >= Size.X || y < 0 || y >= Size.Y);
    }
}