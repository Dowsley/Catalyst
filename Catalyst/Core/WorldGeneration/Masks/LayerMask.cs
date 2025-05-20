using System;
using System.Collections.Generic;
using System.Linq;
using Catalyst.Globals;
using Microsoft.Xna.Framework;

namespace Catalyst.Core.WorldGeneration.Masks;

public class LayerMask : PassMask
{
    private readonly FastNoiseLite? _boundaryNoise;
    private readonly float _boundaryNoiseAmplitude;

    public LayerMask(Point size, List<string> layersToAffect, bool allowList = true, 
                     int boundaryNoiseSeed = 0, float boundaryNoiseFrequency = 0.03f, float boundaryNoiseAmplitude = 0.015f)
        : base(size)
    {
        _boundaryNoiseAmplitude = boundaryNoiseAmplitude;
        if (_boundaryNoiseAmplitude > 0 && boundaryNoiseFrequency > 0)
        {
            _boundaryNoise = new FastNoiseLite(boundaryNoiseSeed);
            _boundaryNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _boundaryNoise.SetFrequency(boundaryNoiseFrequency);
        }

        for (int x = 0; x < size.X; x++)
        {
            for (int y = 0; y < size.Y; y++)
            {
                var pos = new Point(x, y);
                bool isInAtLeastOneSpecifiedLayer = layersToAffect.Any(layerName => IsInLayer(pos, size, layerName, x));

                if (allowList)
                {
                    Allowed[x, y] = isInAtLeastOneSpecifiedLayer ? 1.0f : 0.0f;
                }
                else
                {
                    Allowed[x, y] = !isInAtLeastOneSpecifiedLayer ? 1.0f : 0.0f;
                }
            }
        }
    }

    protected bool IsInLayer(Point gridPos, Point currentWorldSize, string layerIdToMatch, int currentX)
    {
        float yRatio = (float)gridPos.Y / currentWorldSize.Y;
        float noiseOffset = 0f;
        (float nominalStart, float nominalEnd) layerBoundaries;

        try
        {
            layerBoundaries = WorldGenSettings.GetLayerBoundaryRatios(layerIdToMatch);
        }
        catch (ArgumentException)
        {
            return false;
        }

        if (layerBoundaries.nominalStart >= layerBoundaries.nominalEnd) 
        {
            return false; 
        }

        if (_boundaryNoise != null && _boundaryNoiseAmplitude > 0)
        {
            noiseOffset = _boundaryNoise.GetNoise(currentX, 0f) * _boundaryNoiseAmplitude;
        }

        float noisyStart = Math.Clamp(layerBoundaries.nominalStart + noiseOffset, 0f, 1f);
        float noisyEnd = Math.Clamp(layerBoundaries.nominalEnd + noiseOffset, 0f, 1f);

        if (noisyStart >= noisyEnd) 
        {
            return false; 
        }

        return yRatio >= noisyStart && yRatio < noisyEnd;
    }
}