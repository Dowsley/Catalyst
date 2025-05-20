using System.Collections.Generic;
using System.Linq;
using Catalyst.Globals;
using Microsoft.Xna.Framework;

namespace Catalyst.Core.WorldGeneration.Masks;

public class LayerMask : PassMask
{
    public LayerMask(Point size, List<string> layers, bool allowList = true) : base(size)
    {
        for (int x = 0; x < size.X; x++)
        {
            for (int y = 0; y < size.Y; y++)
            {
                var pos = new Point(x, y);

                Allowed[x, y] = allowList
                    ? layers.Any(layer => IsInLayer(pos, size, layer))
                    : layers.All(layer => !IsInLayer(pos, size, layer));
            }
        }
    }

    protected static bool IsInLayer(Point gridPos, Point size, string layerId)
    {
        float yRatio = (float)gridPos.Y / size.Y;

        for (int i = 0; i < Settings.Layers.Count; i++)
        {
            var (id, end) = Settings.Layers[i];
            float start = i == 0 ? 0f : Settings.Layers[i - 1].Item2;

            if (id == layerId && yRatio >= start && yRatio < end)
                return true;
        }

        return false;
    }
}