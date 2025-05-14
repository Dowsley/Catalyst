using System;
using Microsoft.Xna.Framework;

namespace Catalyst.Core;

public class World
{
    private readonly bool[,] _tiles;
    private readonly Vector2 _worldSize;
    private readonly FastNoiseLite _noise;

    public World(int sizeX, int sizeY)
    {
        _tiles = new bool[sizeX, sizeY];
        _worldSize = new Vector2(sizeX, sizeY);
        
        _noise = new FastNoiseLite();
        SetupNoise();
    }
    
    public void GenerateTerrain()
    {
        for (int x = 0; x < _worldSize.X; x++)
        {
            float noiseValue = _noise.GetNoise(x, 0);
            int surfaceY = (int)(_worldSize.Y / 2 + noiseValue * 10);

            for (int y = 0; y < _worldSize.Y; y++)
                _tiles[x, y] = y > surfaceY;
        }
    }

    public bool GetTileAt(int x, int y)
    {
        return _tiles[x, y];
    }
    
    public int GetWidth()
    {
        return (int)_worldSize.X;
    }
    
    public int GetHeight()
    {
        return (int)_worldSize.Y;
    }

    public void RandomizeSeed()
    {
        var random = new Random();
        _noise.SetSeed(random.Next());
    }

    private void SetupNoise()
    {
        _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _noise.SetFrequency(0.05f);
        _noise.SetSeed(0);
    }
}