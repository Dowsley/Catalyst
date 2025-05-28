using System;
using Catalyst.Core;
using Catalyst.Globals;
using Microsoft.Xna.Framework;

namespace Catalyst.Systems;

/// <summary>
/// Manages the lighting calculations for the game world. This system is responsible for determining how much light
/// each tile in the world receives. It supports light values potentially exceeding 1.0f internally for brighter sources,
/// which should be clamped to 1.0f by the renderer for display.
/// </summary>
/// <remarks>
/// <p>
/// The lighting process is primarily handled in two main phases for any given area that needs an update:
/// </p>
/// <p>
/// 1. <c>ScanPhase</c>: This initial phase scans each column of tiles from top to bottom. 
///    It seeds initial light values based on several criteria:
///    <list type="bullet">
///        <item>
///            <description><c>Player Torch</c>: If the player has their torch on, a temporary light source is created
///            around the player, with intensity falling off with distance.
///            </description>
///        </item>
///        <item>
///            <description><c>Tile Glow</c>: Tiles can have an intrinsic <c>Glow</c> property. If a tile glows (Glow > 0),
///            it contributes light equal to its <c>Glow</c> value. This value can exceed 1.0f to represent
///            sources brighter than standard sky light for propagation purposes.
///            </description>
///        </item>
///        <item>
///            <description><c>Direct Sky Light</c>: If a tile has an unobstructed vertical path to the absolute top of the world
///            (tracked by <c>inOpenSkyColumn</c>), it receives light from the sky. This direct sky light starts at
///            <c>SkyLightIntensity</c> (typically 1.0f) and begins to fade linearly as it penetrates deeper into the world, specifically
///            starting from 3/4 of the way down through the "surface" layer (as defined by <c>WorldGenSettings</c>)
///            until it reaches zero intensity at the bottom of the surface layer.
///            </description>
///        </item>
///        <item>
///            <description><c>Surface Air Brightness</c>: Air tiles that fall within the defined "surface" layer
///            are intrinsically set to <c>SkyLightIntensity</c>. This ensures that the general surface area of the world
///            is bright, simulating ambient light in outdoor, ground-level areas.
///            </description>
///        </item>
///    </list>
///    A tile's final light value from this phase is the maximum of these considerations if applicable, or 0.0f if none apply or occluded.
///    Solid tiles encountered during the top-down scan will block the <c>inOpenSkyColumn</c> for subsequent tiles below them, regardless of their glow.
/// </p>
/// <p>
/// 2. <c>BlurPhase</c>: After initial light values are seeded by <c>ScanPhase</c>, the <c>BlurPhase</c> propagates these
///    light values (which can be > 1.0f). This phase consists of multiple <c>BlurPass</c> calls. Each <c>BlurPass</c> iterates over the area
///    horizontally and vertically. During propagation, light spreads from brighter tiles to dimmer adjacent tiles, reduced by decay factors.
///    Light below <c>MinLightThreshold</c> is culled. This allows super-bright sources to influence a larger area or maintain higher intensity further out.
/// </p>
/// <p>
/// The system processes lighting updates for rectangular areas. The renderer is responsible for clamping light values to a 0-1 range for display.
/// </p>
/// </remarks>
public class LightingSystem
{
    private readonly World _world;

    public const float SkyLightIntensity = 1.0f; // Standard intensity for sky/surface air.
    public const float LightDecayThroughAir = 0.91f;
    public const float LightDecayThroughSolid = 0.56f;
    private const float MinLightThreshold = 0.0185f; // Light below this is considered black
    private const int PlayerTorchRadius = 3; // Tiles away from player center
    private static readonly float[] PlayerTorchFalloff = { 1.0f, 0.8f, 0.5f, 0.2f }; // Intensity by distance: 0, 1, 2, 3

    private Rectangle _lastProcessedLightingArea = Rectangle.Empty;

    public LightingSystem(World world)
    {
        _world = world;
    }

    /// <summary>
    /// Initializes the lighting for the entire world. Typically called once on world generation.
    /// </summary>
    public void InitializeEntireWorldLighting()
    {
        DoLightingUpdate(new Rectangle(0, 0, _world.WorldSize.X, _world.WorldSize.Y));
    }

    /// <summary>
    /// Requests a lighting update for a specific rectangular area of the world.
    /// </summary>
    /// <param name="areaToUpdate">The rectangular area (in tile coordinates) that needs its lighting recomputed.</param>
    public void RequestLightingUpdate(Rectangle areaToUpdate)
    {
        // TODO: Basic check to avoid recomputing the exact same area unless forced can be improved.
        // More sophisticated logic (like ForceLightUpdate) can be added here or in World
        // if (areaToUpdate == _lastProcessedLightingArea && !ShouldForceUpdate()) 
        // {
        //     // Potentially add a check to see if areaToUpdate is mostly contained within _lastProcessedLightingArea
        //     // and if no significant changes have occurred. For now, if it's the same rect, we might skip
        //     // if not forced. However, player movement usually means a new rect.
        //     // Let's assume for now if it's requested, we do it, but clamping/merging logic could go here.
        // }
        DoLightingUpdate(areaToUpdate);
    }
    
    /// <summary>
    /// Placeholder for logic to determine if a lighting update should be forced.
    /// </summary>
    /// <returns>True if an update should be forced, false otherwise.</returns>
    private bool ShouldForceUpdate() 
    {
        // e.g., time since last full update, significant game event like time of day change
        return false; 
    }

    /// <summary>
    /// Performs the lighting update for the specified area, encompassing both scan and blur phases.
    /// </summary>
    /// <param name="areaToUpdate">The rectangular area to process.</param>
    private void DoLightingUpdate(Rectangle areaToUpdate)
    {
        areaToUpdate.X = Math.Max(0, areaToUpdate.X);
        areaToUpdate.Y = Math.Max(0, areaToUpdate.Y);
        areaToUpdate.Width = Math.Min(_world.WorldSize.X - areaToUpdate.X, areaToUpdate.Width);
        areaToUpdate.Height = Math.Min(_world.WorldSize.Y - areaToUpdate.Y, areaToUpdate.Height);

        if (areaToUpdate.Width <= 0 || areaToUpdate.Height <= 0)
            return;

        ScanPhase(areaToUpdate);
        BlurPhase(areaToUpdate);
        
        _lastProcessedLightingArea = areaToUpdate;
    }

    /// <summary>
    /// First phase of lighting: Scans columns to seed initial light values. 
    /// Light values can exceed 1.0f if tile Glow is high.
    /// This includes player torch, intrinsic tile glow, direct sky light, and surface air brightness.
    /// </summary>
    /// <param name="areaToUpdate">The rectangular area to scan.</param>
    private void ScanPhase(Rectangle areaToUpdate)
    {
        var (surfaceStartYRatio, surfaceEndYRatio) = WorldGenSettings.GetLayerBoundaryRatios("surface");
        int surfaceLayerAbsYStart = (int)(_world.WorldSize.Y * surfaceStartYRatio);
        int surfaceLayerAbsYEnd = (int)(_world.WorldSize.Y * surfaceEndYRatio);

        float skyLightFadeStartY = surfaceLayerAbsYStart + (surfaceLayerAbsYEnd - surfaceLayerAbsYStart) * 0.75f; // will start fading by about 3/4 of the surface
        float skyLightFadeEndY = Math.Max(skyLightFadeStartY, surfaceLayerAbsYEnd); 
        skyLightFadeEndY = Math.Min(skyLightFadeEndY, _world.WorldSize.Y - 1);

        Point playerGridPos = _world.PlayerRef?.GridPosition ?? new Point(-1, -1); // Default if player is null
        bool isTorchOn = _world.PlayerRef is { IsTorchOn: true };

        for (int x = areaToUpdate.Left; x < areaToUpdate.Left + areaToUpdate.Width; x++)
        {
            bool inOpenSkyColumn = true;
            for (int y = 0; y < _world.WorldSize.Y; y++) 
            {
                if (!_world.IsWithinBounds(x, y)) continue;
                
                if (y >= areaToUpdate.Bottom && !inOpenSkyColumn && y >= skyLightFadeEndY)
                    break;

                var currentTile = _world.GetTileAt(x, y);
                bool isCurrentTileSolid = currentTile.Type.IsSolid;
                bool hasSolidWall = currentTile.WallType.IsSolid;
                bool isCurrentTileSurfaceAir = !isCurrentTileSolid && !hasSolidWall && y >= surfaceLayerAbsYStart && y < surfaceLayerAbsYEnd;
                
                float lightValueToSet = 0.0f;

                // 1. Check for player torch light
                if (isTorchOn)
                {
                    int distSq = (x - playerGridPos.X) * (x - playerGridPos.X) + (y - playerGridPos.Y) * (y - playerGridPos.Y);
                    // Using Chebyshev distance (max of abs diff in coords) for square/diamond shape, or Euclidean for circle
                    int distChebyshev = Math.Max(Math.Abs(x - playerGridPos.X), Math.Abs(y - playerGridPos.Y));

                    if (distChebyshev < PlayerTorchFalloff.Length)
                    {
                        lightValueToSet = Math.Max(lightValueToSet, PlayerTorchFalloff[distChebyshev]);
                    }
                }

                // 2. Check for intrinsic tile glow. This can be > 1.0f.
                if (currentTile.Type.Glow > 0)
                {
                    lightValueToSet = Math.Max(lightValueToSet, currentTile.Type.Glow);
                }

                // 3. Check for surface air brightness (typically 1.0f)
                if (isCurrentTileSurfaceAir)
                {
                    lightValueToSet = Math.Max(lightValueToSet, SkyLightIntensity);
                }

                // 4. Check for direct sky light (typically 1.0f, fading with depth)
                if (inOpenSkyColumn)
                {
                    float directFadedSkyLight = 0f;
                    if (y < skyLightFadeStartY)
                    {
                        directFadedSkyLight = SkyLightIntensity;
                    }
                    else if (y < skyLightFadeEndY)
                    {
                        float fadeRange = skyLightFadeEndY - skyLightFadeStartY;
                        if (fadeRange <= 0) 
                        {
                            directFadedSkyLight = (y < skyLightFadeEndY) ? SkyLightIntensity : 0f; 
                        }
                        else
                        {
                            float progressInFade = (y - skyLightFadeStartY) / fadeRange;
                            directFadedSkyLight = SkyLightIntensity * (1f - progressInFade);
                        }
                    }
                    directFadedSkyLight = Math.Max(0f, directFadedSkyLight);
                    lightValueToSet = Math.Max(lightValueToSet, directFadedSkyLight);

                    if (isCurrentTileSolid)
                    {
                        inOpenSkyColumn = false;
                    }
                }

                if (y >= areaToUpdate.Top && y < areaToUpdate.Bottom) 
                {
                    _world.SetLightValue(x, y, lightValueToSet);
                }
                else 
                {
                     if (inOpenSkyColumn && isCurrentTileSolid) 
                     {
                        inOpenSkyColumn = false;
                     }
                }
            }
        }
    }

    /// <summary>
    /// Second phase of lighting: Propagates light values from the ScanPhase throughout the area.
    /// Called multiple times to simulate light spreading.
    /// </summary>
    /// <param name="areaToUpdate">The rectangular area to apply blur passes to.</param>
    private void BlurPhase(Rectangle areaToUpdate)
    {
        BlurPass(areaToUpdate);
        BlurPass(areaToUpdate); 
    }

    /// <summary>
    /// Performs a single blur pass over the specified area, spreading light horizontally and vertically.
    /// </summary>
    /// <param name="area">The rectangular area for the blur pass.</param>
    private void BlurPass(Rectangle area)
    {
        // Horizontal pass
        for (int y = area.Top; y < area.Bottom; y++)
        {
            if (y < 0 || y >= _world.WorldSize.Y) continue;
            BlurLine(y, area.Left, area.Right, true, true, area); // L->R
            BlurLine(y, area.Left, area.Right, true, false, area); // R->L
        }
        // Vertical pass
        for (int x = area.Left; x < area.Right; x++)
        {
            if (x < 0 || x >= _world.WorldSize.X) continue;
            BlurLine(x, area.Top, area.Bottom, false, true, area); // T->B
            BlurLine(x, area.Top, area.Bottom, false, false, area); // B->T
        }
    }

    /// <summary>
    /// Propagates light along a single line (horizontal or vertical) in one direction.
    /// </summary>
    /// <param name="fixedCoord">The fixed coordinate (Y for horizontal, X for vertical).</param>
    /// <param name="mobileStart">The starting coordinate for the moving axis.</param>
    /// <param name="mobileEnd">The ending coordinate (exclusive) for the moving axis.</param>
    /// <param name="isHorizontal">True if blurring horizontally, false for vertically.</param>
    /// <param name="forwardDir">True if blurring in increasing coordinate order (L->R, T->B), false for reverse.</param>
    /// <param name="overallArea">The overall rectangular area being processed, for boundary checks.</param>
    private void BlurLine(int fixedCoord, int mobileStart, int mobileEnd, bool isHorizontal, bool forwardDir, Rectangle overallArea)
    {
        float currentPropagatingLight = 0.0f;
        int step = forwardDir ? 1 : -1;
        
        int currentMobile = forwardDir ? mobileStart : mobileEnd - 1;
        int limit = forwardDir ? mobileEnd : mobileStart -1;

        while(forwardDir ? currentMobile < limit : currentMobile > limit)
        {
            int x, y;
            if (isHorizontal) { x = currentMobile; y = fixedCoord; }
            else { x = fixedCoord; y = currentMobile; }

            if (!overallArea.Contains(x,y) || !_world.IsWithinBounds(x,y)) 
            {
                currentMobile += step;
                continue;
            }

            float tileLight = _world.GetLightValueAt(x, y);

            if (tileLight > currentPropagatingLight)
            {
                currentPropagatingLight = tileLight;
            }
            else if (currentPropagatingLight >= MinLightThreshold)
            {
               _world.SetLightValue(x, y, Math.Max(tileLight, currentPropagatingLight));
            }
            
            if (currentPropagatingLight > 0)
            {
                if (_world.GetTileAt(x, y).Type.IsSolid)
                {
                    currentPropagatingLight *= LightDecayThroughSolid;
                }
                else 
                {
                    currentPropagatingLight *= LightDecayThroughAir;
                }

                if (currentPropagatingLight < MinLightThreshold)
                {
                    currentPropagatingLight = 0.0f;
                }
            }
            currentMobile += step;
        }
    }
} 
