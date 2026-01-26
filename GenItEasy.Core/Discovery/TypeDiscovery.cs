using GenItEasy.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TypeFilter = GenItEasy.Filters.TypeFilter;

namespace GenItEasy.Discovery;

public class TypeDiscovery(TypeScriptGenConfig config, ILogger logger)
{
    private readonly TypeFilter _typeFilter = new(config, logger);
    private string? _assemblyDirectory;

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
            // Store the directory for dependency resolution
            _assemblyDirectory = Path.GetDirectoryName(fullPath);

            // Register handler to resolve dependencies from the assembly's directory
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblyDependency;

            return Assembly.LoadFrom(fullPath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to load assembly '{assemblyName}' from: {fullPath}", ex);
        }
    }

    private Assembly? ResolveAssemblyDependency(object? sender, ResolveEventArgs args)
    {
        if (string.IsNullOrEmpty(_assemblyDirectory))
            return null;

        var assemblyName = new AssemblyName(args.Name).Name;
        if (string.IsNullOrEmpty(assemblyName))
            return null;

        var assemblyPath = Path.Combine(_assemblyDirectory, assemblyName + ".dll");

        if (File.Exists(assemblyPath))
        {
            logger.LogDebug("Resolving dependency {AssemblyName} from {Path}", assemblyName, assemblyPath);
            return Assembly.LoadFrom(assemblyPath);
        }

        return null;
    }

    public List<Type> DiscoverTypes(Assembly assembly)
    {
        var discoveredTypes = new List<Type>();
        var allTypes = GetLoadableTypes(assembly);

        foreach (var nsConfig in config.Namespaces)
        {
            logger.LogDebug("Scanning namespace: {Namespace}", nsConfig.Namespace);

            var types = new List<Type>();
            foreach (var t in allTypes)
            {
                try
                {
                    if (TypeFilter.IsInTargetNamespace(t, nsConfig) &&
                        t is { IsNested: false, IsPublic: true } &&
                        _typeFilter.IsNotStaticClass(t) &&
                        _typeFilter.IsTypeIncluded(t, nsConfig))
                    {
                        types.Add(t);
                    }
                }
                catch (Exception ex) when (IsAssemblyLoadException(ex))
                {
                    logger.LogDebug("Skipping type {Type} due to: {Message}", t.FullName ?? t.Name, ex.Message);
                }
            }

            logger.LogDebug("Found {Count} types in namespace {Namespace}", types.Count, nsConfig.Namespace);
            discoveredTypes.AddRange(types);
        }

        return discoveredTypes;
    }

    /// <summary>
    /// Gets all types that can be loaded from the assembly, gracefully handling
    /// types that fail to load due to missing dependencies.
    /// </summary>
    private IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            LogTypeLoadWarning(ex.Types.Count(t => t != null));
            return ex.Types.Where(t => t != null)!;
        }
        catch (Exception ex) when (IsAssemblyLoadException(ex))
        {
            logger.LogWarning(
                "Assembly dependency issue ({ExceptionType}): {Message}. Attempting module-by-module loading.",
                ex.GetType().Name, ex.Message);
            return GetTypesFromModules(assembly);
        }
    }

    private static bool IsAssemblyLoadException(Exception ex)
    {
        return ex is FileNotFoundException
            || ex is FileLoadException
            || ex is TypeLoadException
            || ex is BadImageFormatException;
    }

    private void LogTypeLoadWarning(int loadedCount)
    {
        logger.LogWarning(
            "Some types could not be loaded from assembly. This is normal if the assembly " +
            "references framework types (e.g., ASP.NET Core) not available in the current context. " +
            "Continuing with {LoadedCount} loadable types.",
            loadedCount);
    }

    /// <summary>
    /// Attempts to load types module by module, handling exceptions for each module.
    /// </summary>
    private IEnumerable<Type> GetTypesFromModules(Assembly assembly)
    {
        var types = new List<Type>();

        foreach (var module in assembly.GetModules())
        {
            try
            {
                types.AddRange(module.GetTypes());
            }
            catch (ReflectionTypeLoadException ex)
            {
                var loadedTypes = ex.Types.Where(t => t != null).ToList();
                logger.LogDebug("Module {Module}: loaded {Count} types", module.Name, loadedTypes.Count);
                types.AddRange(loadedTypes!);
            }
            catch (Exception ex) when (IsAssemblyLoadException(ex))
            {
                // Skip this module, dependency issue
                logger.LogDebug("Skipping module {Module} due to: {Message}", module.Name, ex.Message);
            }
        }

        LogTypeLoadWarning(types.Count);
        return types;
    }
}