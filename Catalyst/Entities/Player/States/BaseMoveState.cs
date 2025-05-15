using Catalyst.Core;
using Catalyst.Core.Fsm;
using Catalyst.Entities.Fsm;
using Catalyst.Globals;
using Catalyst.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Catalyst.Entities.Player.States;

public class BaseMoveState(Entity owner) : EntityBaseState(owner)
{
	
	public override EntityBaseState Input()
	{
		var kState = Keyboard.GetState();
		if (kState.IsKeyDown(Keys.Space))
			return new JumpState(Owner);
		
		return null;
	}
	
	public override BaseState<Entity> Update(World worldRef, GameTime gameTime)
	{
		var motion = GetMovementInput();
		// TODO: Set sprite direction
		// if (motion < 0)
		// 	entity.set_sprite_dir(entity.SPRITE_DIRS.LEFT)
		// elif motion > 0:
		// entity.set_sprite_dir(entity.SPRITE_DIRS.RIGHT)

		var realPlayerSpeed = Owner.GetRealSpeed() * TimeUtils.GetDelta(gameTime);
		Owner.Velocity.Y += Settings.Gravity * TimeUtils.GetDelta(gameTime);
		Owner.Velocity.X = MathHelper.Lerp(
			Owner.Velocity.X,
			motion * realPlayerSpeed,
			motion != 0 ? Owner.Acceleration : Settings.GroundFriction
		);
		worldRef.CollisionSystem.MoveAndSlide(Owner);

		if (motion == 0)
			return new IdleState(Owner);

		if (!worldRef.CollisionSystem.IsOnFloor(Owner))
			return new FallState(Owner);

		return null;
	}

	protected static int GetMovementInput()
	{
		var kState = Keyboard.GetState();
		if (kState.IsKeyDown(Keys.A))
			return -1;
		return kState.IsKeyDown(Keys.D) ? 1 : 0;
	}
}


