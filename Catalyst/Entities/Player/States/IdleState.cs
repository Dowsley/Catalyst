using Catalyst.Core;
using Catalyst.Core.Fsm;
using Catalyst.Entities.Fsm;
using Catalyst.Globals;
using Catalyst.Systems;
using Catalyst.Utils;
using Microsoft.Xna.Framework;

namespace Catalyst.Entities.Player.States;

public class IdleState(Entity owner) : EntityBaseState(owner)
{
	public override EntityBaseState? Input()
	{
		if (InputSystem.IsActionPressed("left") || InputSystem.IsActionPressed("right"))
			return new WalkState(Owner);
		if (InputSystem.IsActionPressed("jump"))
			return new JumpState(Owner);
		
		return null;
	}

	public override BaseState<Entity>? Update(World worldRef, GameTime gameTime)
	{
		Owner.Velocity.Y += Settings.Gravity * TimeUtils.GetDelta(gameTime);
		Owner.Velocity.X = MathHelper.Lerp(Owner.Velocity.X, 0f, Settings.GroundFriction);
		worldRef.CollisionSystem.MoveAndSlide(Owner);

		if (!worldRef.CollisionSystem.IsOnFloor(Owner))
			return new FallState(Owner);
		
		return null;
	}
}
