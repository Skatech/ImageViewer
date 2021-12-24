using System;
using System.IO;
using System.Reflection;

namespace Skatech.Extensions.Runtime;

static class AssemblyDetails {
    public static string GetAssemblyCompany(this Assembly assembly) {
        return assembly.GetAssemblyAttributeString(typeof(AssemblyCompanyAttribute),
                   nameof(AssemblyCompanyAttribute.Company));
    }

    public static string GetAssemblyProduct(this Assembly assembly) {
        return assembly.GetAssemblyAttributeString(typeof(AssemblyProductAttribute),
                   nameof(AssemblyProductAttribute.Product));
    }
    
    public static string CreateAppDataPath(this Assembly assembly, string optionalDirectoryOrFileName = "") {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            assembly.GetAssemblyCompany(), assembly.GetAssemblyProduct(), optionalDirectoryOrFileName);
    }

    public static string GetAssemblyAttributeString(this Assembly assembly, Type attributeType, string propertyName) {
        return assembly.GetAssemblyAttributeValue(attributeType, propertyName) as string ??
               throw new InvalidOperationException(
                   $"Assembly attribute value not set: \"{attributeType.Name}.{propertyName}\".");
    }

    public static object? GetAssemblyAttributeValue(this Assembly assembly, Type attributeType, string propertyName) {
        var attr = Attribute.GetCustomAttribute(assembly, attributeType) ??
            throw new InvalidOperationException($"Assembly has no attribute: \"{attributeType.Name}\".");
        var prop = attributeType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public) ??
            throw new InvalidOperationException(
                $"Assembly attribute has no property: \"{attributeType.Name}.{propertyName}\".");
        return prop.GetValue(attr);
    }
}
