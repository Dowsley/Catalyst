using System;

namespace Catalyst.Core.WorldGeneration;

/// <summary>
/// Random Iterator meant for deterministic world generation based on a seed.
/// </summary>
/// <remarks>
/// Calls to any "Gen" method will advance iterator. Given a seed, and an ordered set of calls, the outcome will always be the same.
/// </remarks>
public static class WorldGenRNG
{
    private static Random _random = new();
    
    public static void SetWorldSeed(int seed)
    {
        _random = new Random(seed);
    }

    public static int GenSeed() => _random.Next();
    public static FastNoiseLite GenNoise() => new(GenSeed());
    public static Random GenRandomizer() => new(GenSeed());
}