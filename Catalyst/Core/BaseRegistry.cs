using System.Collections.Generic;
using System.Linq;

namespace Catalyst.Core;

public abstract class BaseRegistry<T> where T : class
{
    private readonly Dictionary<string, T> _types = [];
    
    public BaseRegistry() { }

    public T Get(string id)
    {
        _types.TryGetValue(id, out var value);
        return value;
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

