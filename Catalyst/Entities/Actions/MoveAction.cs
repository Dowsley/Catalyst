using Catalyst.Core;
using Microsoft.Xna.Framework;

namespace Catalyst.Entities.Actions;

public class MoveAction(Entity entityRef, Vector2 moveOffset) : Action(entityRef)
{
    protected Vector2 MoveOffset = moveOffset;
    
    public override bool CanPerform(World worldRef)
    {
        // TODO IMPLEMENT COLLISION CHECKING
        return true;
    }

    public override void Perform(World worldRef)
    {
        EntityRef.Position += MoveOffset;
    }
}
