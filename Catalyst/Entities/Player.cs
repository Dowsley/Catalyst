using System.Collections.Generic;
using System.Linq;
using Catalyst.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Catalyst.Entities;

public class Player(Vector2 pos, Vector2 colliderSize, float speedFactor = 1.0f)
    : Entity(pos, colliderSize, speedFactor)
{
    public override IEnumerable<Action> Update(GameTime gameTime, KeyboardState kState)
    {
        var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var realPlayerSpeed = GetRealSpeed() * delta;
        if (kState.IsKeyDown(Keys.Left))
            Velocity.X -= realPlayerSpeed;
        if (kState.IsKeyDown(Keys.Right))
            Velocity.X += realPlayerSpeed;
        if (kState.IsKeyDown(Keys.Space))
            Velocity.Y -= Settings.PlayerJumpForce * delta;
        
        List<Action> actions = base.Update(gameTime, kState).ToList(); // will automatically add gravity+velocity movement
        return actions;
    }
}