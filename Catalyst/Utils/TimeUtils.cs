using Microsoft.Xna.Framework;

namespace Catalyst.Utils;

public static class TimeUtils
{
    public static float GetDelta(GameTime gameTime)
    {
        return (float)gameTime.ElapsedGameTime.TotalSeconds;
    }
}