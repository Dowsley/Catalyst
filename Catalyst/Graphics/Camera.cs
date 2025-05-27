using Microsoft.Xna.Framework;

namespace Catalyst.Graphics;

public class Camera
{
    public Vector2 Position { get; set; }
    public float Zoom { get; set; }
    public float Rotation { get; set; }

    public Camera(Vector2 pos, float zoom=1f, float rot=0f)
    {
        Position = pos;
        Zoom = zoom;
        Rotation = rot;
    }
    
    public Matrix GetViewMatrix()
    {
        return
            Matrix.CreateTranslation(new Vector3(-Position, 0f))
                * Matrix.CreateRotationZ(Rotation)
                * Matrix.CreateScale(Zoom, Zoom, 1f);
    }
}