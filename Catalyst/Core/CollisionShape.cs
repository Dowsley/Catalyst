using Microsoft.Xna.Framework;

namespace Catalyst.Core;

public enum CollidingMode
{
    Vertically,
    Horizontally,
    Both,
    Any,
}

public struct CollisionShape(Vector2 pos, Vector2 size)
{
    public Vector2 Position = pos;
    public Vector2 Size = size;

    public Vector2 TopLeft => Vector2.Zero + Position; // origin
    public Vector2 TopRight => new Vector2(Size.X, 0) + Position;
    public Vector2 BottomLeft => new Vector2(0, Size.Y) + Position;
    public Vector2 BottomRight => Size + Position;

    public float Left => TopLeft.X;
    public float Right => TopRight.X;
    public float Top => TopLeft.Y;
    public float Bottom => BottomLeft.Y;

    public bool IsCollidingWith(CollisionShape otherColShape, CollidingMode collidingMode = CollidingMode.Both)
    {
        bool horizontalOverlap = Position.X < otherColShape.Position.X + otherColShape.Size.X &&
                                 Position.X + Size.X > otherColShape.Position.X;

        bool verticalOverlap = Position.Y < otherColShape.Position.Y + otherColShape.Size.Y &&
                               Position.Y + Size.Y > otherColShape.Position.Y;

        return collidingMode switch
        {
            CollidingMode.Both => horizontalOverlap && verticalOverlap,
            CollidingMode.Any => horizontalOverlap || verticalOverlap,
            CollidingMode.Horizontally => horizontalOverlap,
            CollidingMode.Vertically => verticalOverlap,
            _ => false
        };
    }
}