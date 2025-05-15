using System;
using Catalyst.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Catalyst.Core;

public class World
{
    private const int SpawnAreaSize = 5;
    
    private readonly bool[,] _tiles;
    private readonly Point _worldSize;
    private readonly FastNoiseLite _noise;
    private Player _playerRef;    

    public World(int sizeX, int sizeY)
    {
        _tiles = new bool[sizeX, sizeY];
        _worldSize = new Point(sizeX, sizeY);
        _noise = new FastNoiseLite();
        SetupNoise();
    }
    
    public void Update(GameTime gameTime, KeyboardState kState)
    {
        UpdatePlayer(gameTime, kState);
        UpdateEntities(gameTime);
    }

    public void UpdatePlayer(GameTime gameTime, KeyboardState kState)
    {
        _playerRef.Update(gameTime, kState);
    }

    public void UpdateEntities(GameTime gameTime)
    {
        
    }
    
    public void GenerateTerrain()
    {
        for (int x = 0; x < _worldSize.X; x++)
        {
            float noiseValue = _noise.GetNoise(x, 0);
            int surfaceY = (int)(_worldSize.Y / 2 + noiseValue * Settings.WorldGenNoiseAmplitude);

            for (int y = 0; y < _worldSize.Y; y++)
            {
                _tiles[x, y] = y > surfaceY;
            }
        }
    }

    public void SetPlayer(Player playerRef)
    {
        _playerRef = playerRef;
    }

    public bool GetTileAt(int x, int y)
    {
        return _tiles[x, y];
    }
    
    public int GetWidth()
    {
        return _worldSize.X;
    }
    
    public int GetHeight()
    {
        return _worldSize.Y;
    }

    public Vector2 GetSpawningPosForPlayer()
    {
        int middle = GetWidth() / 2;
        for (int x = int.Max(middle-SpawnAreaSize, 0); x < int.Min(middle+SpawnAreaSize+1, GetWidth()-1); x++)
        {
            // Find first empty block to stand on
            for (int y = GetHeight() - 1; y >= 0; y--)
            {
                var tile = GetTileAt(x, y);
                if (tile == false)
                {
                    return new Vector2(x, y) * Settings.TileSize;
                }
            }
        }
        
        return Vector2.Zero;
    }

    private void SetupNoise()
    {
        _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _noise.SetFrequency(0.05f);
        
        var random = new Random();
        _noise.SetSeed(random.Next());
    }
}