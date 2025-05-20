using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Catalyst.Globals;

public static class Settings
{
    // Graphics
    public const int TileSize = 8;
    public const int ResScale = 2;
    public const int NativeWidth = 800;//*2;
    public const int NativeHeight = 600;//*2;
    
    // Physics constants
    public const float BaseRealPlayerSpeed = 60.0f;
    public const float Gravity = 15f;
    public const float PlayerJumpForce = 250f;
    public const float GroundFriction = 0.8f;    // Retain 80% of velocity on ground
    public const float AirResistance = 0.5f;     // Retain 50% of velocity in air
    public const float MinimumVelocity = 0.1f;   // Below this, velocity snaps to zero

    public static Point Up = new(0, -1);
    public static Point Down = new(0, 1);
    public static Point Left = new(-1, 0);
    public static Point Right = new(1, 0);
    
    // World gen
    // Layers: Works in ranges [[ ranges. From 0, to LAYER_NUM1-1. Then from LAYER_NUM1 to to LAYER_NUM2-1 etc.  
    public static List<(string, float)> Layers =
    [
        ("space", 0.05f),
        ("surface", 0.15f),
        ("underground", 0.5f),
        ("cavern", 0.9f),
        ("underworld", 1f),
    ];
}