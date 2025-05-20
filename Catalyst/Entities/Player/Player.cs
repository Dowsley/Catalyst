using Catalyst.Core;
using Catalyst.Entities.Player.States;
using Microsoft.Xna.Framework;

namespace Catalyst.Entities.Player;

public class Player(Vector2 pos, Vector2 colliderSize, float speedFactor = 1.0f)
    : Entity(pos, colliderSize, speedFactor)
{
    public Point GridPosition => World.WorldToGrid(Position);

    public override void Update(World worldRef, GameTime gameTime)
    {
        if (States.CurrentState == null)
            States.ChangeState(new IdleState(this), worldRef, gameTime);
        
        States.Input(worldRef, gameTime);
        base.Update(worldRef, gameTime);
    }
}