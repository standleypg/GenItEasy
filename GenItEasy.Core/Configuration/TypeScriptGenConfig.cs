using System.Text.Json.Serialization;

namespace GenItEasy.Configuration;

/// <summary>
/// Specifies how TypeScript enums should be generated.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EnumStyle
{
    /// <summary>
    /// Generate numeric enums: enum Status { Active = 0, Inactive = 1 }
    /// </summary>
    Numeric,

    /// <summary>
    /// Generate string enums: enum Status { Active = "Active", Inactive = "Inactive" }
    /// </summary>
    String,

    /// <summary>
    /// Generate string literal union types: type Status = "Active" | "Inactive"
    /// </summary>
    StringLiteral
}

public class TypeScriptGenConfig
{
    [JsonPropertyName("assemblyName")]
    public string AssemblyName { get; set; } = string.Empty;

    [JsonPropertyName("outputPath")]
    public string OutputPath { get; set; } = string.Empty;

    [JsonPropertyName("outputFileName")]
    public string OutputFileName { get; set; } = "models.gen.ts";

    [JsonPropertyName("outputNamespace")]
    public string? OutputNamespace { get; set; }

    [JsonPropertyName("baseDirectory")]
    public string? BaseDirectory { get; set; }

    [JsonPropertyName("includeStaticClasses")]
    public bool IncludeStaticClasses { get; set; }

    [JsonPropertyName("enumStyle")]
    public EnumStyle EnumStyle { get; set; } = EnumStyle.Numeric;

    [JsonPropertyName("namespaces")]
    public List<NamespaceConfig> Namespaces { get; set; } = [];
}

public class NamespaceConfig
{
    [JsonPropertyName("namespace")]
    public string Namespace { get; set; } = string.Empty;

    [JsonPropertyName("includeTypes")]
    public List<string>? IncludeTypes { get; set; }

    [JsonPropertyName("excludeTypes")]
    public List<string>? ExcludeTypes { get; set; }

    [JsonPropertyName("includeGenericTypes")]
    public List<string>? IncludeGenericTypes { get; set; }

    [JsonPropertyName("excludeGenericTypes")]
    public List<string>? ExcludeGenericTypes { get; set; }

    [JsonPropertyName("includeNested")]
    public bool IncludeNested { get; set; } = false;
}