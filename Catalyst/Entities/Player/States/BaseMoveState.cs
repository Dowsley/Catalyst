using Catalyst.Core;
using Catalyst.Core.Fsm;
using Catalyst.Entities.Fsm;
using Catalyst.Globals;
using Catalyst.Systems;
using Catalyst.Utils;
using Microsoft.Xna.Framework;

namespace Catalyst.Entities.Player.States;

public class BaseMoveState(Entity owner) : EntityBaseState(owner)
{
	
	public override EntityBaseState? Input()
	{
		if (InputSystem.IsActionPressed("jump"))
			return new JumpState(Owner);
		
		return null;
	}
	
	public override BaseState<Entity>? Update(World worldRef, GameTime gameTime)
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
		if (InputSystem.IsActionPressed("left"))
			return -1;
		return InputSystem.IsActionPressed("right") ? 1 : 0;
	}
}


