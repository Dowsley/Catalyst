using Catalyst.Core;

namespace Catalyst.Tiles;

public class TileRegistry : BaseRegistry<TileType>
{
    public TileRegistry()
    {
        var empty = new TileType("EMPTY", "Empty", "Just air", 0, false);
        empty.AddSpriteVariant(null);
        Register("EMPTY", empty);
    }
}
