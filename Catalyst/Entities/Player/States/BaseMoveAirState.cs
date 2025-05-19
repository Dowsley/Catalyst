using Catalyst.Core;
using Catalyst.Core.Fsm;
using Catalyst.Entities.Fsm;
using Catalyst.Globals;
using Catalyst.Systems;
using Catalyst.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Catalyst.Entities.Player.States;

public class BaseMoveAirState(Entity owner) : EntityBaseState(owner)
{
	public override BaseState<Entity>? Update(World worldRef, GameTime gameTime)
	{
		var motion = 0;
		if (InputSystem.IsActionPressed("left"))
		{
			motion = -1;
			// TODO: Owner.SetSpriteDir(Owner.SpriteDirs.Left);
		}

		if (InputSystem.IsActionPressed("right"))
		{
			motion = 1;
			// TODO: Owner.SetSpriteDir(Owner.SpriteDirs.Right);
		}

		var realPlayerSpeed = Owner.GetRealSpeed() * TimeUtils.GetDelta(gameTime);
		Owner.Velocity.Y += Settings.Gravity * TimeUtils.GetDelta(gameTime);
		Owner.Velocity.X = MathHelper.Lerp(
			Owner.Velocity.X,
			motion * realPlayerSpeed,
			motion != 0? Owner.Acceleration : Settings.AirResistance
		);
		
		worldRef.CollisionSystem.MoveAndSlide(Owner);
		if (!worldRef.CollisionSystem.IsOnFloor(Owner))
			return null;
		
		if (motion != 0)
			return new WalkState(Owner);
		return new IdleState(Owner);

	}
}
