using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Catalyst.Globals;

public static class WorldGenSettings
{
    // Layers: Works in ranges [[ ranges. From 0, to LAYER_NUM1-1. Then from LAYER_NUM1 to to LAYER_NUM2-1 etc.  
    public static readonly List<(string Id, float EndYRatio)> Layers =
    [
        ("space", 0.05f),
        ("surface", 0.15f),
        ("underground", 0.5f),
        ("cavern", 0.9f),
        ("underworld", 1f),
    ];

    public static (float StartYRatio, float EndYRatio) GetLayerBoundaryRatios(string layerId)
    {
        float startYRatio = 0f;
        int layerIndex = Layers.FindIndex(l => l.Id == layerId);
        float endYRatio = Layers[layerIndex].EndYRatio;
        if (layerIndex > 0)
        {
            startYRatio = Layers[layerIndex - 1].EndYRatio; 
        }
        
        return (startYRatio, endYRatio);
    }
    
    public static float ComputeSurfaceBaseLine(Point worldSize)
    {
        var (surfaceStartPercent, surfaceEndPercent) = GetLayerBoundaryRatios("surface");
        float surfaceMidPointYPercent = (surfaceStartPercent + surfaceEndPercent) / 2.0f;
        return worldSize.Y * surfaceMidPointYPercent;
    }
} 