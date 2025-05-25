using Catalyst.Core;
using Catalyst.Core.Fsm;
using Microsoft.Xna.Framework;

namespace Catalyst.Entities.Fsm;

public abstract class EntityBaseState(Entity owner) : BaseState<Entity>(owner)
{
	public override void Enter(World worldRef, GameTime gameTime)
	{
		// TODO: Sprite/animation changes should happen here
	}
	
	public virtual EntityBaseState? Input()
	{
		return null;
	}
}
