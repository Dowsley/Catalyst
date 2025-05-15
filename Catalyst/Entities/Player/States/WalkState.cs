using Catalyst.Entities.Fsm;

namespace Catalyst.Entities.Player.States;

public class WalkState(Entity owner) : BaseMoveState(owner)
{
	public override EntityBaseState Input()
	{
		var newState = base.Input();
		return newState;
	}
}
