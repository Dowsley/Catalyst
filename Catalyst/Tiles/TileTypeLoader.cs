using Catalyst.Graphics;
using Microsoft.Xna.Framework;
using Catalyst.Core;
using Catalyst.Data.DTO;

namespace Catalyst.Tiles;

public class TileTypeLoader : BaseLoader<TileTypeDTO, TileType>
{
    protected override TileType MapDtoToDomain(TileTypeDTO dto)
    {
        var tileType = new TileType(dto.Id, dto.Name, dto.Description, dto.Durability, dto.IsSolid)
        {
            MapColor = dto.MapColor
        };

        foreach (var spriteDto in dto.SpriteVariants)
        {
            var sourceRectangle = new Rectangle(
                spriteDto.SourceRectCoords.X,
                spriteDto.SourceRectCoords.Y,
                Globals.Settings.TileSize,
                Globals.Settings.TileSize
            );
            var sprite = new Sprite(spriteDto.TextureId, sourceRectangle);
            tileType.AddSpriteVariant(sprite);
        }
        return tileType;
    }

    protected override string GetDomainObjectId(TileType domainObject)
    {
        return domainObject.Id;
    }
}