using System;
using System.IO;
using System.Xml.Serialization;
using Catalyst.Data.DTOs;
using Catalyst.Graphics;
using Microsoft.Xna.Framework;

namespace Catalyst.Tiles;

public static class TileTypeLoader
{
    public static void LoadTileTypesFromDirectory(string relativeDirectoryPath, TileRegistry registry)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string fullDirectoryPath = Path.Combine(baseDirectory, relativeDirectoryPath);

        if (!Directory.Exists(fullDirectoryPath))
        {
            Console.WriteLine($"Warning: Tile types directory not found: {fullDirectoryPath}");
            return;
        }

        string[] xmlFiles = Directory.GetFiles(fullDirectoryPath, "*.xml");
        XmlSerializer serializer = new XmlSerializer(typeof(TileTypeDTO)); 
        foreach (string filePath in xmlFiles)
        {
            try
            {
                using FileStream fileStream = new FileStream(filePath, FileMode.Open);
                if (serializer.Deserialize(fileStream) is TileTypeDTO dto)
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
                    
                    registry.Register(tileType.Id, tileType);
                }
                else
                {
                    Console.WriteLine($"Error: Failed to deserialize tile type from {filePath} into DTO.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading tile type from {filePath}: {ex.Message}");
            }
        }
    }
}