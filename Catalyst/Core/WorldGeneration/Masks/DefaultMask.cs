using Microsoft.Xna.Framework;

namespace Catalyst.Core.WorldGeneration.Masks;

/// <summary>
/// All positions are allowed.
/// </summary>
public class DefaultMask : PassMask
{
    public DefaultMask(Point size) : base(size)
    {
        for (int x = 0; x < size.X; x++)
            for (int y = 0; y < size.Y; y++)
                Allowed[x, y] = true;
    }
}