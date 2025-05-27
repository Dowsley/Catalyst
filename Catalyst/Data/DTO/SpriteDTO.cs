using System.Xml.Serialization;

namespace Catalyst.Data.DTO;

public class SpriteDTO
{
    public class SourceCoordsDTO
    {
        [XmlElement("X")]
        public int X { get; set; }

        [XmlElement("Y")]
        public int Y { get; set; }

        public SourceCoordsDTO() {}
    }
    
    [XmlElement("TextureId")]
    public string TextureId { get; set; } = string.Empty;

    [XmlElement("SourceRect")]
    public SourceCoordsDTO SourceRectCoords { get; set; } = new();

    // Parameterless constructor for XML serialization
    public SpriteDTO() {}
}