using System.Reflection;

namespace GenItEasy.TypeScript.Models;

/// <summary>
/// Represents a TypeScript property.
/// </summary>
public class TsProperty
{
    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets the underlying CLR member (PropertyInfo or FieldInfo).
    /// </summary>
    public MemberInfo MemberInfo { get; }

    /// <summary>
    /// Gets the property type.
    /// </summary>
    public Type PropertyType { get; }

    /// <summary>
    /// Creates a new TsProperty from a PropertyInfo.
    /// </summary>
    public TsProperty(PropertyInfo propertyInfo)
    {
        MemberInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
        Name = propertyInfo.Name;
        PropertyType = propertyInfo.PropertyType;
    }

    /// <summary>
    /// Creates a new TsProperty from a FieldInfo.
    /// </summary>
    public TsProperty(FieldInfo fieldInfo)
    {
        MemberInfo = fieldInfo ?? throw new ArgumentNullException(nameof(fieldInfo));
        Name = fieldInfo.Name;
        PropertyType = fieldInfo.FieldType;
    }
}
