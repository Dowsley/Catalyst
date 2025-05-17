using System;
using Catalyst.Core;
using Catalyst.Core.Fsm;
using Microsoft.Xna.Framework;

namespace Catalyst.Entities.Fsm;

public class EntityStateMachine(Entity owner)
    : BaseStateMachine<Entity>(owner)
{
	public void Input(World worldRef, GameTime gameTime)
	{
		var currentState = CurrentState as EntityBaseState;
		var newState = currentState?.Input();
		if (newState != null)
		{
			Console.WriteLine($"Changing state to: {newState.GetType().Name}");
			ChangeState(newState, worldRef, gameTime);
		}
	}
}
