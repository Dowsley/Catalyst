using Catalyst.Core;
using Catalyst.Core.Fsm;
using Microsoft.Xna.Framework;

namespace Catalyst.Entities.Player.States;

public class FallState(Entity owner) : BaseMoveAirState(owner)
{
    public override void Enter(World worldRef, GameTime gameTime)
    {
		// We override any anim calls
		// because it only starts AFTER the transition ends
    }

    public override BaseState<Entity>? Update(World worldRef, GameTime gameTime)
    {
        var newState = base.Update(worldRef, gameTime);
        if (newState != null)
            return newState;
        
		// TODO: Fall animation		
		
		// var exit_transition = Utils.is_number_in_range(
		// 	entity.velocity.y,
		// 	-entity.jump_and_fall_transition_threshold,
		// 	entity.jump_and_fall_transition_threshold
		// )
		// if exit_transition:
		// 	entity.animations.play('fall')
        
        
        return null;
    }
}
