namespace GenItEasy.TypeScript.Models;

/// <summary>
/// Represents a TypeScript enum member.
/// </summary>
public class TsEnumValue(string name, object value)
{
    /// <summary>
    /// Gets the enum member name.
    /// </summary>
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

    /// <summary>
    /// Gets the enum member value.
    /// </summary>
    public object Value { get; } = value ?? throw new ArgumentNullException(nameof(value));
}
