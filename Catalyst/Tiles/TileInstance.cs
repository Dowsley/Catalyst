using System.Drawing;

namespace Catalyst.Tiles;

public class TileInstance(Point parentTilePos)
{
    public Point ParentTilePos = parentTilePos;
    public int Health;
};