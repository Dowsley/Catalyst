namespace Catalyst.Globals;

public static class Settings
{
    public const int TileSize = 8;
    public const int ResScale = 3;
    public const int NativeWidth = 320*2;
    public const int NativeHeight = 180*2;
    public const int WorldGenNoiseAmplitude = 5;
    public const float BaseRealPlayerSpeed = 60.0f;
    public const float Gravity = 15f;
    public const float PlayerJumpForce = 250f;
    
    // Physics constants
    public const float GroundFriction = 0.8f;    // Retain 80% of velocity on ground
    public const float AirResistance = 0.5f;     // Retain 50% of velocity in air
    public const float MinimumVelocity = 0.1f;   // Below this, velocity snaps to zero
}