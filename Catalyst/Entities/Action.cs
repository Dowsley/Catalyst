using Catalyst.Core;

namespace Catalyst.Entities;

public abstract class Action(Entity entityRef)
{
    protected readonly Entity EntityRef = entityRef;

    public virtual bool CanPerform(World worldRef)
    {
        return false;
    }
    
    public virtual void Perform(World worldRef)
    {
        
    }
}