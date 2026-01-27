using System.Text.Json;
using GenItEasy;
using GenItEasy.Utilities;
using Microsoft.Extensions.Logging;

try
{
    using var loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.SetMinimumLevel(LogLevel.Debug);
    });

    var logger = loggerFactory.CreateLogger<TypeScriptGenerator>();

    string? configPath = null;
    string? baseDirectory = null;

    for (var i = 0; i < args.Length; i++)
    {
        if (args[i] == "--base-directory" && i + 1 < args.Length)
        {
            baseDirectory = args[++i];
        }
        else if (configPath == null)
        {
            configPath = args[i];
        }
    }

    var config = ConfigLoader.LoadConfig(configPath);

    if (baseDirectory != null)
    {
        config.BaseDirectory = Path.GetFullPath(baseDirectory);
    }

    Console.WriteLine("Configuration loaded successfully.");
    Console.WriteLine($"Assembly: {config.AssemblyName}");
    Console.WriteLine($"Output: {config.OutputPath}/{config.OutputFileName}");
    Console.WriteLine();

    var generator = new TypeScriptGenerator(config, logger);
    generator.Generate();

    Console.WriteLine();
    Console.WriteLine("TypeScript generation completed successfully!");
    Environment.ExitCode = 0;
}
catch (FileNotFoundException ex)
{
    Console.Error.WriteLine($"ERROR: {ex.Message}");
    Console.Error.WriteLine();
    Console.Error.WriteLine("Usage: GenItEasy.Core.CLI [configPath]");
    Console.Error.WriteLine("  configPath: Optional path to typescriptgenconfig.json");
    Environment.ExitCode = 1;
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"CONFIGURATION ERROR: {ex.Message}");
    Environment.ExitCode = 2;
}
catch (JsonException ex)
{
    Console.Error.WriteLine($"JSON PARSE ERROR: {ex.Message}");
    Environment.ExitCode = 3;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"UNEXPECTED ERROR: {ex.Message}");
    Console.Error.WriteLine();
    Console.Error.WriteLine(ex.StackTrace);
    Environment.ExitCode = 3;
}
