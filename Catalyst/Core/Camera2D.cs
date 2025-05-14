using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace Catalyst.Core;

public class Camera2D
{
    public Vector2 Position = Vector2.Zero;
    public float Zoom = 1f;
    public float Rotation = 0f;

    public Matrix GetViewMatrix()
    {
        return
            Matrix.CreateTranslation(new Vector3(-Position, 0f)) *
            Matrix.CreateRotationZ(Rotation) *
            Matrix.CreateScale(Zoom, Zoom, 1f);
    }
}