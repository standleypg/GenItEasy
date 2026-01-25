using GenItEasy.Configuration;

namespace GenItEasy.Utilities;

public class PathResolver(TypeScriptGenConfig config)
{
    public string ResolveOutputPath()
    {
        var outputDir = config.OutputPath;

        // If path is relative, resolve it relative to the current working directory
        if (!Path.IsPathRooted(outputDir))
        {
            outputDir = Path.Combine(Directory.GetCurrentDirectory(), outputDir);
        }

        return Path.GetFullPath(Path.Combine(outputDir, config.OutputFileName));
    }
}