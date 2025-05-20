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

    private struct LayerBoundaryData
    {
        public float NominalStart;
        public float NominalEnd;
    }

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
        float noiseOffset = 0f;

        if (_boundaryNoise != null && _boundaryNoiseAmplitude > 0)
        {
            noiseOffset = _boundaryNoise.GetNoise(currentX, 0f) * _boundaryNoiseAmplitude;
        }

        float noisyStart = Math.Clamp(nominalStart + noiseOffset, 0f, 1f);
        float noisyEnd = Math.Clamp(nominalEnd + noiseOffset, 0f, 1f);

        if (noisyStart >= noisyEnd)
        {
            return false;
        }

        return yRatio >= noisyStart && yRatio < noisyEnd;
    }
}