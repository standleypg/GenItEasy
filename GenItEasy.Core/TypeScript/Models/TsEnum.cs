namespace GenItEasy.TypeScript.Models;

/// <summary>
/// Represents a TypeScript enum.
/// </summary>
public class TsEnum(Type clrType) : ITsType
{
    /// <summary>
    /// Gets or sets the enum name.
    /// </summary>
    public string Name { get; set; } = clrType.Name;

    /// <summary>
    /// Gets the enum values.
    /// </summary>
    public List<TsEnumValue> Values { get; } = [];
}
