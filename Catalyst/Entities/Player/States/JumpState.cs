using System;
using Catalyst.Core;
using Catalyst.Core.Fsm;
using Catalyst.Globals;
using Catalyst.Utils;
using Microsoft.Xna.Framework;

namespace Catalyst.Entities.Player.States;

public class JumpState(Entity owner) : BaseMoveAirState(owner)
{
	public override void Enter(World worldRef, GameTime gameTime)
	{
		// This calls the base class enter function, which is necessary here
		// to make sure the animation switches
		base.Enter(worldRef, gameTime);
		Owner.Velocity.Y = -Settings.PlayerJumpForce * TimeUtils.GetDelta(gameTime);
	}

	public override BaseState<Entity> Update(World worldRef, GameTime gameTime)
	{
		var newState = base.Update(worldRef, gameTime);
		if (newState != null)
			return newState;

		// TODO: Jump animation.
		// var enter_transition = Utils.is_number_in_range(
		// 	entity.velocity.y,
		// 	-entity.jump_and_fall_transition_threshold,
		// 	entity.jump_and_fall_transition_threshold
		// )
		// if enter_transition:
		// entity.animations.play('jump_trans')
		
		return Owner.Velocity.Y > 0f ? new FallState(Owner) : null;
	}
}
