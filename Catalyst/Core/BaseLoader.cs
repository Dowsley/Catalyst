using System;
using System.IO;
using System.Xml.Serialization;

namespace Catalyst.Core;

public abstract class BaseLoader<TDto, TDomain>
    where TDto : class, new()  // For XmlSerializer and potential instantiation
    where TDomain : class      // To match BaseRegistry constraint
{
    protected abstract TDomain MapDtoToDomain(TDto dto);
    protected abstract string GetDomainObjectId(TDomain domainObject);

    public void LoadFromDirectory(string relativeDirectoryPath, BaseRegistry<TDomain> registry)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string fullDirectoryPath = Path.Combine(baseDirectory, relativeDirectoryPath);

        if (!Directory.Exists(fullDirectoryPath))
        {
            Console.WriteLine($"Warning: Data directory not found: {fullDirectoryPath}");
            return;
        }

        string[] xmlFiles = Directory.GetFiles(fullDirectoryPath, "*.xml");
        XmlSerializer serializer = new XmlSerializer(typeof(TDto));

        foreach (string filePath in xmlFiles)
        {
            try
            {
                using FileStream fileStream = new FileStream(filePath, FileMode.Open);
                if (serializer.Deserialize(fileStream) is TDto dto)
                {
                    TDomain domainObject = MapDtoToDomain(dto);
                    string id = GetDomainObjectId(domainObject);
                    registry.Register(id, domainObject);
                }
                else
                {
                    Console.WriteLine($"Error: Failed to deserialize data from {filePath} into DTO of type {typeof(TDto).Name}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data from {filePath}: {ex.Message}");
            }
        }
    }
}
