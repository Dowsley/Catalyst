using System.Collections.Generic;
using Catalyst.Core.WorldGeneration.Passes;

namespace Catalyst.Core.WorldGeneration;

public class WorldGenerator
{
    private readonly List<Pass> _passes = [];

    public WorldGenerator(World world, int seed)
    {
        _passes.Add(new InitialTerrainPass(world, seed));
        _passes.Add(new SmallCaveCarvingPass(world, seed));
        _passes.Add(new CaveCarvingPass(world, seed));
        _passes.Add(new LongCaveCarving(world, seed));
    }

    public void Generate()
    {
        foreach (var pass in _passes)
            pass.Apply();
    }
}