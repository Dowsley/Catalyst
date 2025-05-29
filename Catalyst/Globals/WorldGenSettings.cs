using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Catalyst.Globals;

public static class WorldGenSettings
{
    // TODO: Switch this for an enum and complex object
    
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
        float surfaceMidPointYPercent = (surfaceStartPercent + surfaceEndPercent) / 2f;
        return worldSize.Y * surfaceMidPointYPercent;
    }
    
    public static bool IsInLayer(int y, string layerId, int worldHeightInTiles)
    {
        float startYRatio = 0f;

        int layerIndex = Layers.FindIndex(l => l.Id == layerId);
        if (layerIndex == -1) 
        {
            return false; 
        }

        var endYRatio = Layers[layerIndex].EndYRatio;
        if (layerIndex > 0)
        {
            startYRatio = Layers[layerIndex - 1].EndYRatio;
        }

        float startYAbsolute = startYRatio * worldHeightInTiles;
        float endYAbsolute = endYRatio * worldHeightInTiles;

        // A tile at y is in the layer if its y-coordinate is >= the layer's start and < the layer's end.
        return y >= startYAbsolute && y < endYAbsolute;
    }
}