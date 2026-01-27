namespace GenItEasy.TypeScript.Models;

/// <summary>
/// Represents a TypeScript interface (generated from a C# class).
/// </summary>
public class TsClass : ITsType
{
    /// <summary>
    /// Gets or sets the interface name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets the properties of this interface.
    /// </summary>
    public List<TsProperty> Properties { get; } = [];

    /// <summary>
    /// Gets the underlying CLR type.
    /// </summary>
    public Type ClrType { get; }

    /// <summary>
    /// Gets the generic type parameter names (e.g., ["T", "TKey", "TValue"]).
    /// </summary>
    public List<string> TypeParameters { get; } = [];

    public TsClass(Type clrType)
    {
        ClrType = clrType ?? throw new ArgumentNullException(nameof(clrType));
        Name = clrType.Name;

        // Handle generic types - remove backtick and arity
        var backtickIndex = Name.IndexOf('`');
        
        if (backtickIndex <= 0)
        {
            return;
        }
        
        Name = Name[..backtickIndex];

        if (!clrType.IsGenericTypeDefinition)
        {
            return;
        }
        
        // Extract generic type parameter names
        foreach (var param in clrType.GetGenericArguments())
        {
            TypeParameters.Add(param.Name);
        }
    }
}
