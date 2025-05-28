using System;
using System.Collections.Generic;
using Catalyst.Graphics;
using Microsoft.Xna.Framework;

namespace Catalyst.Tiles;

public class TileType(string id, string name, string description, int maxHealth, bool isSolid, float glow = 0f)
{
    // public static TileType Uninitialized;

    public string Id { get; set; } = id;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public int MaxHealth { get; set; } = maxHealth;
    public bool IsSolid { get; set; } = isSolid;
    public Color MapColor { get; set; } = Color.Magenta;
    public readonly List<Sprite> SpriteVariants = [];
    public float Glow = glow; // light source

    public Sprite GetSprite(int index)
    {
        return SpriteVariants[index];
    }

    public int GetRandomSpriteIndex(Random random)
    {
        return random.Next() % SpriteVariants.Count;
    }

    public void AddSpriteVariant(Sprite variant)
    {
        SpriteVariants.Add(variant);
    }
};