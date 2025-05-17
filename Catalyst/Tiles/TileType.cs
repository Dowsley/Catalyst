using System;
using System.Collections.Generic;
using Catalyst.Graphics;

namespace Catalyst.Tiles;

public class TileType(string id, string name, string description, int maxHealth, bool isSolid)
{
    // public static TileType Uninitialized;

    public string Id { get; set; } = id;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public int MaxHealth { get; set; } = maxHealth;
    public bool IsSolid { get; set; } = isSolid;
    public List<Sprite2D> SpriteVariants = [];

    public Sprite2D GetSprite(int index)
    {
        return SpriteVariants[index];
    }

    public int GetRandomSpriteIndex(Random random)
    {
        return random.Next() % SpriteVariants.Count;
    }

    public void AddSpriteVariant(Sprite2D variant)
    {
        SpriteVariants.Add(variant);
    }
};