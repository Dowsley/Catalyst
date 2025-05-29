using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace Catalyst.Data.DTO;

[XmlRoot("TileType")]
public class TileTypeDTO
{
    [XmlElement("Id")]
    public string Id { get; set; } = string.Empty;

    [XmlElement("Name")]
    public string Name { get; set; } = string.Empty;

    [XmlElement("Description")]
    public string Description { get; set; } = string.Empty;

    [XmlElement("Durability")]
    public int Durability { get; set; }

    [XmlElement("IsSolid")]
    public bool IsSolid { get; set; }

    [XmlElement("MapColor")]
    public Color MapColor { get; set; } = Color.Magenta;
    
    [XmlElement("Glow")]
    public float Glow { get; set; } = 0f;

    [XmlArray("SpriteVariants")]
    [XmlArrayItem("Sprite2D")]
    public List<SpriteDTO> SpriteVariants { get; set; } = [];
}