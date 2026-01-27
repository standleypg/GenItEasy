namespace GenItEasy.Formatters;

/// <summary>
/// Handles type visibility (which types to show/hide).
/// </summary>
public static class TypeVisibilityFormatter
{
    /// <summary>
    /// Determines if a type should be visible in the output.
    /// </summary>
    public static bool IsTypeVisible(string typeName)
    {
        // Hide Guid in all its variations
        return !typeName.Contains("Guid", StringComparison.OrdinalIgnoreCase);
    }
}