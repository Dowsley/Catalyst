using Catalyst.Core;
using Catalyst.Entities.Fsm;
using Catalyst.Entities.Player.States;
using Catalyst.Globals;
using Microsoft.Xna.Framework;

namespace Catalyst.Entities;

public class Entity
{
    protected readonly Vector2 ColliderSize;
    protected readonly EntityStateMachine States;
    
    public Vector2 Position;
    public Vector2 Velocity;
    public float SpeedFactor { get; set; }
    public readonly float Acceleration = 1f;
    public CollisionShape CollisionShape => new(Position, ColliderSize);

    public Entity(Vector2 pos, Vector2 colliderSize, float speedFactor=1.0f)
    {
        Position = pos;
        SpeedFactor = speedFactor;
        ColliderSize = colliderSize;
        States = new EntityStateMachine(this);
    }

    public virtual void Update(World worldRef, GameTime gameTime)
    {
        if (States.CurrentState == null)
            States.ChangeState(new IdleState(this), worldRef, gameTime);
        States.Update(worldRef, gameTime);
    }
    
    /* Gets speed in pixels (before multiplying by delta) */
    public float GetRealSpeed()
    {
        return Settings.BaseRealPlayerSpeed * SpeedFactor;
    }
}