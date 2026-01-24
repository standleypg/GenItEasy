using GenItEasy.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TypeFilter = GenItEasy.Filters.TypeFilter;

namespace GenItEasy.Discovery;

public class TypeDiscovery(TypeScriptGenConfig config, ILogger logger)
{
    private readonly TypeFilter _typeFilter = new(config, logger);

    public Assembly LoadAssembly(string assemblyName)
    {
        var fileName = assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
            ? assemblyName
            : assemblyName + ".dll";

        var baseDir = config.BaseDirectory ?? Directory.GetCurrentDirectory();
        var fullPath = Path.GetFullPath(Path.Combine(baseDir, fileName));

        logger.LogDebug("Loading assembly from: {AssemblyPath}", fullPath);

        if (!File.Exists(fullPath))
        {
            throw new InvalidOperationException(
                $"Failed to load assembly '{assemblyName}'. File not found at: {fullPath}");
        }

        try
        {
            return Assembly.LoadFrom(fullPath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to load assembly '{assemblyName}' from: {fullPath}", ex);
        }
    }

    public List<Type> DiscoverTypes(Assembly assembly)
    {
        var discoveredTypes = new List<Type>();

        foreach (var nsConfig in config.Namespaces)
        {
            logger.LogDebug("Scanning namespace: {Namespace}", nsConfig.Namespace);

            var types = assembly.GetTypes()
                .Where(t => TypeFilter.IsInTargetNamespace(t, nsConfig) &&
                            t is { IsNested: false, IsPublic: true } &&
                            this._typeFilter.IsNotStaticClass(t) &&
                            this._typeFilter.IsTypeIncluded(t, nsConfig))
                .ToList();

            logger.LogDebug("Found {Count} types in namespace {Namespace}", types.Count, nsConfig.Namespace);
            discoveredTypes.AddRange(types);
        }

        return discoveredTypes;
    }
}