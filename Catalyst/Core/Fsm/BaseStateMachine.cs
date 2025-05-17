using System;
using Microsoft.Xna.Framework;

namespace Catalyst.Core.Fsm;

public class BaseStateMachine<T>
{
	public BaseState<T>? CurrentState;
	protected T Owner;

	public BaseStateMachine(T owner)
	{
		Owner = owner;
	}

	public void ChangeState(BaseState<T> newState, World worldRef, GameTime gameTime)
	{
		CurrentState?.Exit(worldRef, gameTime);
		CurrentState = newState;
		CurrentState?.Enter(worldRef, gameTime);
	}

	public void Update(World worldRef, GameTime gameTime)
	{
		var newState = CurrentState?.Update(worldRef, gameTime);
		if (newState != null)
		{
			Console.WriteLine($"Changing state to: {newState.GetType().Name}");
			ChangeState(newState, worldRef, gameTime);
		}
	}
}
