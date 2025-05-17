using Catalyst.Core;
using Catalyst.Core.Fsm;
using Catalyst.Entities.Fsm;
using Catalyst.Globals;
using Catalyst.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Catalyst.Entities.Player.States;

public class IdleState(Entity owner) : EntityBaseState(owner)
{
	public override EntityBaseState? Input()
	{
		var kState = Keyboard.GetState();
		if (kState.IsKeyDown(Keys.A) || kState.IsKeyDown(Keys.D))
			return new WalkState(Owner);
		if (kState.IsKeyDown(Keys.Space))
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
