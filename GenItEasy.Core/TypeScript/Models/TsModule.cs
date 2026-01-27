namespace GenItEasy.TypeScript.Models;

/// <summary>
/// Represents a TypeScript module/namespace.
/// </summary>
public class TsModule(string name)
{
    /// <summary>
    /// Gets or sets the module name (namespace).
    /// </summary>
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

    /// <summary>
    /// Gets the classes in this module.
    /// </summary>
    public List<TsClass> Classes { get; } = [];

    /// <summary>
    /// Gets the enums in this module.
    /// </summary>
    public List<TsEnum> Enums { get; } = [];

    /// <summary>
    /// Gets all members (classes and enums) in this module.
    /// </summary>
    public IEnumerable<ITsType> Members
    {
        get
        {
            foreach (var cls in Classes)
                yield return cls;
            foreach (var enm in Enums)
                yield return enm;
        }
    }
}
