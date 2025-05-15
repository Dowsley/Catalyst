using Catalyst.Core;
using Microsoft.Xna.Framework;

namespace Catalyst.Entities.Actions;

public class IdleAction(Entity entityRef) : Action(entityRef)
{
    public override bool CanPerform(World worldRef)
    {
        return true;
    }
    
    public override void Perform(World worldRef) { }
}
