using System.Collections.Generic;
using System.Linq;
using Catalyst.Core;
using Catalyst.Entities.Player.States;
using Catalyst.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Catalyst.Entities.Player;

public class Player(Vector2 pos, Vector2 colliderSize, float speedFactor = 1.0f)
    : Entity(pos, colliderSize, speedFactor)
{
    public override void Update(World worldRef, GameTime gameTime)
    {
        if (States.CurrentState == null)
            States.ChangeState(new IdleState(this), worldRef, gameTime);
        
        States.Input(worldRef, gameTime);
        base.Update(worldRef, gameTime);
    }
}