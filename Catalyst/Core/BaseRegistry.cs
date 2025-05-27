using System;
using System.Collections.Generic;

namespace Catalyst.Core;

public abstract class BaseRegistry<T> where T : class
{
    private readonly Dictionary<string, T> _types = [];

    public int Count => _types.Count;

    public T Get(string id)
    {
        _types.TryGetValue(id, out var value);
        return value ?? throw new InvalidOperationException($"TileType of Id '{id}' does not exist");
    }

    public void Register(string id, T type)
    {
        _types.Add(id, type);
    }
    
    public IEnumerable<T> GetAll()
    {
        return _types.Values;
    }
}

