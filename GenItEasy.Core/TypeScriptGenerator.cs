using GenItEasy.Configuration;
using GenItEasy.Discovery;
using GenItEasy.Formatters;
using GenItEasy.Processing;
using GenItEasy.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TypeLitePlus;

namespace GenItEasy;

public class TypeScriptGenerator(TypeScriptGenConfig config, ILogger<TypeScriptGenerator>? logger = null)
{
    private readonly TypeScriptGenConfig _config = config ?? throw new ArgumentNullException(nameof(config));
    private readonly ILogger<TypeScriptGenerator> _logger = logger ?? NullLogger<TypeScriptGenerator>.Instance;
    private readonly TypeDiscovery _typeDiscovery = new(config, logger ?? NullLogger<TypeScriptGenerator>.Instance);
    private readonly TypeFormatter _typeFormatter = new(logger ?? NullLogger<TypeScriptGenerator>.Instance);
    private readonly PathResolver _pathResolver = new(config);

    public void Generate()
    {
        try
        {
            _logger.LogInformation("Starting TypeScript generation...");

            var assembly = _typeDiscovery.LoadAssembly(_config.AssemblyName);
            var types = _typeDiscovery.DiscoverTypes(assembly);

            if (types.Count == 0)
            {
                _logger.LogWarning("No types found matching the configured patterns. Nothing to generate.");
                return;
            }

            _logger.LogInformation("Found {TypeCount} types to generate", types.Count);

            var model = BuildTypeScriptModel(types);
            var tsCode = GenerateTypeScriptCode(model);
            var outputPath = WriteOutputFile(tsCode);

            _logger.LogInformation("TypeScript models successfully generated at: {OutputPath}", outputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate TypeScript models");
            throw;
        }
    }

    /// <summary>
    /// Builds the TypeScript model from discovered types.
    /// </summary>
    private TsModel BuildTypeScriptModel(List<Type> types)
    {
        var modelBuilder = new TsModelBuilder();

        foreach (var type in types)
        {
            _logger.LogDebug("Adding type to model: {TypeName}", type.FullName);
            modelBuilder.Add(type);
        }

        return modelBuilder.Build();
    }

    /// <summary>
    /// Generates TypeScript code from the model.
    /// </summary>
    private string GenerateTypeScriptCode(TsModel model)
    {
        var generator = ConfigureGenerator();
        var tsCode = generator.Generate(model, TsGeneratorOutput.Properties | TsGeneratorOutput.Enums);

        // Post-processing for cleanup
        return CodePostProcessor.Process(tsCode);
    }

    /// <summary>
    /// Configures the TypeScript generator with custom formatters.
    /// </summary>
    private TsGenerator ConfigureGenerator()
    {
        // Disable const enums for better TypeScript compatibility
        var generator = new TsGenerator { GenerateConstEnums = false };

        // Set up formatters
        generator.SetIdentifierFormatter(IdentifierFormatter.FormatPropertyName);
        generator.SetMemberTypeFormatter((prop, typeName) => _typeFormatter.FormatMemberType(prop, typeName));
        generator.SetModuleNameFormatter(IdentifierFormatter.FormatModuleName);
        generator.SetTypeVisibilityFormatter((_, typeName) => TypeVisibilityFormatter.IsTypeVisible(typeName));

        return generator;
    }

    /// <summary>
    /// Writes the generated TypeScript code to the output file.
    /// </summary>
    private string WriteOutputFile(string tsCode)
    {
        var outputPath = _pathResolver.ResolveOutputPath();
        var outputDirectory = Path.GetDirectoryName(outputPath);

        if (string.IsNullOrEmpty(outputDirectory))
        {
            throw new InvalidOperationException("Could not determine output directory from path.");
        }

        Directory.CreateDirectory(outputDirectory);

        _logger.LogDebug("Writing output to: {OutputPath}", outputPath);
        File.WriteAllText(outputPath, tsCode);

        return outputPath;
    }
}