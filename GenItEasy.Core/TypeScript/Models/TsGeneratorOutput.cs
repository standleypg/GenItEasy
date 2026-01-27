namespace GenItEasy.TypeScript.Models;

/// <summary>
/// Flags for controlling TypeScript generator output.
/// </summary>
[Flags]
public enum TsGeneratorOutput
{
    /// <summary>
    /// Generate interface properties.
    /// </summary>
    Properties,

    /// <summary>
    /// Generate enums.
    /// </summary>
    Enums
}
