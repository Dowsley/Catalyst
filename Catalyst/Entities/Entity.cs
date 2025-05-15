using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using Catalyst.Core;
using Catalyst.Entities.Actions;
using Catalyst.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Catalyst.Entities;

public class Entity
{
    public Vector2 Position;
    public float SpeedFactor { get; set; }
    public CollisionShape CollisionShape;
    
    public Entity(Vector2 pos, Vector2 colliderSize, float speedFactor=1.0f)
    {
        Position = pos;
        SpeedFactor = speedFactor;
        CollisionShape.Size = colliderSize;
    }

    public virtual IEnumerable<Action> Update(GameTime gameTime, KeyboardState kState)
    {
        return [new IdleAction(this)];
    }
    
    /* Gets speed in pixels (before multiplying by delta) */
    public float GetRealSpeed()
    {
        return Settings.BaseRealPlayerSpeed * SpeedFactor;
    }
}