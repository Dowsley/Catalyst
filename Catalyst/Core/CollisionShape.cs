using Microsoft.Xna.Framework;

namespace Catalyst.Core;

public struct CollisionShape(Vector2 pos, Vector2 size)
{
    public Vector2 Position = pos;
    public Vector2 Size = size;

    public bool IsCollidingWith(CollisionShape otherColShape)
    {
        return Position.X < otherColShape.Position.X + otherColShape.Size.X &&
               Position.X + Size.X > otherColShape.Position.X &&
               Position.Y < otherColShape.Position.Y + otherColShape.Size.Y &&
               Position.Y + Size.Y > otherColShape.Position.Y;
    }
}