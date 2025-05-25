using Catalyst.Core;
using Catalyst.Entities.Fsm;
using Catalyst.Entities.Player.States;
using Catalyst.Globals;
using Microsoft.Xna.Framework;

namespace Catalyst.Entities;

public class Entity
{
    private const HorizontalDir DefaultSpriteHDir = HorizontalDir.Right; // by default, all "faced" sprites should face right. that means inverted faces left.
    
    public enum HorizontalDir
    {
        Left,
        Right,
    }
    
    protected readonly Vector2 ColliderSize;
    protected readonly EntityStateMachine States;
    protected HorizontalDir HDir = HorizontalDir.Right;
    
    public Vector2 Position;
    public Vector2 Velocity;
    public float SpeedFactor { get; set; }
    public readonly float Acceleration = 1f;
    public CollisionShape CollisionShape => new(Position, ColliderSize);
    public bool SpriteInverted => HDir != DefaultSpriteHDir;
    
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

    public void SetHorizontalDirection(HorizontalDir dir)
    {
        HDir = dir;
    }
}