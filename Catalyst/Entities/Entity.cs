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
    private Vector2 _colliderSize;
    
    public Vector2 Position;
    public Vector2 Velocity;
    public float SpeedFactor { get; set; }
    public CollisionShape CollisionShape => new(Position, _colliderSize);
    public bool IsAffectedByGravity;

    public Entity(Vector2 pos, Vector2 colliderSize, float speedFactor=1.0f, bool isAffectedByGravity=true)
    {
        Position = pos;
        SpeedFactor = speedFactor;
        _colliderSize = colliderSize;
        IsAffectedByGravity = isAffectedByGravity;
    }

    public virtual IEnumerable<Action> Update(GameTime gameTime, KeyboardState kState)
    {
        var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        List<Action> actions = [];
        if (IsAffectedByGravity)
        {
            Velocity.Y += Settings.Gravity * delta;
        }
        actions.Add(new WalkAction(this, Velocity));
        return actions;
    }
    
    /* Gets speed in pixels (before multiplying by delta) */
    public float GetRealSpeed()
    {
        return Settings.BaseRealPlayerSpeed * SpeedFactor;
    }
}