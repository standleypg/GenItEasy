namespace GenItEasy.TypeScript.Models;

/// <summary>
/// Represents the complete TypeScript model.
/// </summary>
public class TsModel
{
    /// <summary>
    /// Gets the modules in this model.
    /// </summary>
    public List<TsModule> Modules { get; } = [];
}
