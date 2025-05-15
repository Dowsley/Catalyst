namespace Catalyst.Core.Extensions;

using Microsoft.Xna.Framework;

public static class Vector2Extensions
{
    public static bool IsNearZero(this Vector2 v, float epsilon = 0.001f)
    {
        return v.LengthSquared() < epsilon * epsilon;
    }
}
