using Microsoft.Xna.Framework;

namespace Catalyst.Core.Fsm;

public abstract class BaseState<T>(T owner)
{
    protected T Owner = owner;

    public virtual void Enter(World worldRef, GameTime gameTime) { }
    public virtual void Exit(World worldRef, GameTime gameTime) { }

    public virtual BaseState<T>? Update(World worldRef, GameTime gameTime)
    {
        return null;
    }
}