using System;
using System.Collections.Generic;
using System.Linq;
using Catalyst.Globals;
using Microsoft.Xna.Framework;

namespace Catalyst.Core.WorldGeneration.Masks;

public class LayerMask : PassMask
{
    private readonly FastNoiseLite? _startBoundaryNoise;
    private readonly FastNoiseLite? _endBoundaryNoise;
    private readonly float _startBoundaryNoiseAmplitude;
    private readonly float _endBoundaryNoiseAmplitude;

    private struct LayerBoundaryData
    {
        public float NominalStart;
        public float NominalEnd;
    }

    public LayerMask(Point size, List<string> layersToAffect, bool allowList = true,
                     int boundaryNoiseSeed = 0, 
                     float startBoundaryNoiseFrequency = 0.03f, 
                     float startBoundaryNoiseAmplitude = 0.015f, 
                     int? endBoundaryNoiseSeedOffset = null, 
                     float? endBoundaryNoiseFrequency = null, 
                     float? endBoundaryNoiseAmplitude = null)
        : base(size)
    {
        _startBoundaryNoiseAmplitude = startBoundaryNoiseAmplitude;
        _endBoundaryNoiseAmplitude = endBoundaryNoiseAmplitude ?? _startBoundaryNoiseAmplitude;

        float actualEndBoundaryNoiseFrequency = endBoundaryNoiseFrequency ?? startBoundaryNoiseFrequency;

        if (_startBoundaryNoiseAmplitude > 0 && startBoundaryNoiseFrequency > 0)
        {
            _startBoundaryNoise = new FastNoiseLite(boundaryNoiseSeed);
            _startBoundaryNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _startBoundaryNoise.SetFrequency(startBoundaryNoiseFrequency);
        }
        
        if (_endBoundaryNoiseAmplitude > 0 && actualEndBoundaryNoiseFrequency > 0)
        {
            _endBoundaryNoise = new FastNoiseLite(boundaryNoiseSeed + (endBoundaryNoiseSeedOffset ?? 0));
            _endBoundaryNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _endBoundaryNoise.SetFrequency(actualEndBoundaryNoiseFrequency);
        }

        var affectedLayerBoundaries = new List<LayerBoundaryData>();
        foreach (var layerName in layersToAffect)
        {
            var (start, end) = WorldGenSettings.GetLayerBoundaryRatios(layerName);
            if (start < end) 
            {
                affectedLayerBoundaries.Add(new LayerBoundaryData { NominalStart = start, NominalEnd = end });
            }
        }

        for (int x = 0; x < size.X; x++)
        {
            for (int y = 0; y < size.Y; y++)
            {
                float yRatio = (float)y / size.Y;
                bool isInAtLeastOneSpecifiedLayer = false;

                if (affectedLayerBoundaries.Count > 0)
                {
                    if (affectedLayerBoundaries.Any(boundaryData => IsInLayer_PreCalculated(yRatio, boundaryData.NominalStart, boundaryData.NominalEnd, x)))
                    {
                        isInAtLeastOneSpecifiedLayer = true;
                    }
                }

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

    private bool IsInLayer_PreCalculated(float yRatio, float nominalStart, float nominalEnd, int currentX)
    {
        float startNoiseOffset = 0f;
        float endNoiseOffset = 0f;

        if (_startBoundaryNoise != null && _startBoundaryNoiseAmplitude > 0)
        {
            startNoiseOffset = _startBoundaryNoise.GetNoise(currentX, 0f) * _startBoundaryNoiseAmplitude;
        }
        
        if (_endBoundaryNoise != null && _endBoundaryNoiseAmplitude > 0)
        {
            endNoiseOffset = _endBoundaryNoise.GetNoise(currentX, 0f) * _endBoundaryNoiseAmplitude; 
        }

        float noisyStart = Math.Clamp(nominalStart + startNoiseOffset, 0f, 1f);
        float noisyEnd = Math.Clamp(nominalEnd + endNoiseOffset, 0f, 1f);

        if (noisyStart >= noisyEnd)
        {
            return false;
        }

        return yRatio >= noisyStart && yRatio < noisyEnd;
    }
}