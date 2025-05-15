using System.Collections.Generic;
using System.Linq;
using Catalyst.Entities;
using Catalyst.Entities.Actions;
using Catalyst.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Catalyst.Core;

public class Player(Vector2 pos, Vector2 colliderSize, float speedFactor = 1.0f)
    : Entity(pos, colliderSize, speedFactor)
{
    public override IEnumerable<Action> Update(GameTime gameTime, KeyboardState kState)
    {
        List<Action> actions = [];
        
        var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var realPlayerSpeed = GetRealSpeed() * delta;
        var moveOffset = Vector2.Zero;
        if (kState.IsKeyDown(Keys.Left))
            moveOffset.X -= realPlayerSpeed;
        if (kState.IsKeyDown(Keys.Right))
            moveOffset.X += realPlayerSpeed;
        actions.Add(new MoveAction(this, moveOffset));
        
        return actions;
    }
}