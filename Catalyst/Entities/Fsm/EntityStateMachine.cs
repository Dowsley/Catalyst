using System;
using System.Collections.Generic;
using Catalyst.Core;
using Catalyst.Core.Fsm;
using Microsoft.Xna.Framework;

namespace Catalyst.Entities.Fsm;

public class EntityStateMachine(Entity owner)
    : BaseStateMachine<Entity>(owner)
{
	public void Input(World worldRef, GameTime gameTime)
	{
		var currentState = (EntityBaseState)CurrentState;
		Console.WriteLine($"Current state: {currentState.GetType().Name}");
		var newState = currentState.Input();
		if (newState != null)
		{
			ChangeState(newState, worldRef, gameTime);
		}
	}
}
