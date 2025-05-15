using Catalyst.Entities;
using Catalyst.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Catalyst.Core;

public class Player(Vector2 pos, Vector2 colliderSize, float speedFactor = 1.0f)
    : Entity(pos, colliderSize, speedFactor)
{
    public override void Update(GameTime gameTime, KeyboardState kState)
    {
        var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var realPlayerSpeed = GetRealSpeed() * delta;
        if (kState.IsKeyDown(Keys.Left))
            Position.X -= realPlayerSpeed;
        if (kState.IsKeyDown(Keys.Right))
            Position.X += realPlayerSpeed;
        
        base.Update(gameTime, kState);
    }
}