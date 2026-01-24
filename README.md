# GenItEasy

A .NET library and CLI tool for generating TypeScript type definitions from C# assemblies using TypeLitePlus.

## Installation

### As a Library

```bash
dotnet add package GenItEasy
```

### As a CLI Tool

```bash
dotnet tool install GenItEasy.CLI
```

## Usage

### Library Usage

```csharp
using GenItEasy;
using GenItEasy.Utilities;

// Load config from JSON
var config = ConfigLoader.LoadConfig("typescriptgenconfig.json");
var generator = new TypeScriptGenerator(config);
generator.Generate();
```

With logging:

```csharp
using Microsoft.Extensions.Logging;

var logger = loggerFactory.CreateLogger<TypeScriptGenerator>();
var generator = new TypeScriptGenerator(config, logger);
generator.Generate();
```

### CLI Usage

```bash
# Run with config file
dotnet GenItEasy typescriptgenconfig.json

# Specify assembly base directory (where the DLLs are)
dotnet GenItEasy typescriptgenconfig.json --base-directory bin/Debug/net9.0
```

### MSBuild Integration

Add to your consumer project's `.csproj`:

```xml
<Target Name="GenerateTypeScript" AfterTargets="Build">
  <Exec Command="dotnet GenItEasy typescriptgenconfig.json --base-directory $(TargetDir)" />
</Target>
```

This runs TypeScript generation after every build, resolving assemblies from the build output directory.

## Configuration

Create a `typescriptgenconfig.json` file:

```json
{
  "assemblyName": "MyApp.Models",
  "outputPath": "../ClientApp/src/types",
  "outputFileName": "models.gen.ts",
  "namespaces": [
    {
      "namespace": "MyApp.Models.Dtos",
      "includeNested": false,
      "excludeTypes": ["Internal", "Base"]
    }
  ]
}
```

### Configuration Options

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `assemblyName` | string | Yes | - | Assembly name or DLL filename to load |
| `outputPath` | string | Yes | - | Output directory (relative to working directory, or absolute) |
| `outputFileName` | string | Yes | `"models.gen.ts"` | Generated TypeScript filename |
| `baseDirectory` | string | No | `null` | Directory to find the assembly DLL (overridden by `--base-directory` CLI flag) |
| `includeStaticClasses` | bool | No | `false` | Include static classes in output |
| `namespaces` | array | Yes | - | List of namespace configurations |

### Namespace Configuration

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `namespace` | string | Yes | - | Fully qualified namespace to scan |
| `includeTypes` | array | No | `null` | Whitelist patterns (prefix/suffix/exact match) |
| `excludeTypes` | array | No | `null` | Blacklist patterns |
| `includeGenericTypes` | array | No | `null` | Whitelist for generic type base names |
| `excludeGenericTypes` | array | No | `null` | Blacklist for generic type base names |
| `includeNested` | bool | No | `false` | Include types from child namespaces |

## Type Mappings

| C# Type | TypeScript Type |
|---------|----------------|
| `Guid` | `string` |
| `DateTime` | `Date` |
| `int`, `long`, `decimal`, `double`, `float` | `number` |
| `bool` | `boolean` |
| `string` | `string` |
| `List<T>`, `T[]` | `T[]` |
| `Dictionary<string, T>` | `{ [key: string]: T }` |
| Nullable types (`T?`) | `T \| null` |
| Classes | `interface` (with `I` prefix) |
| Enums | `enum` (no prefix) |

## Project Structure

```
GenItEasy/
├── GenItEasy.csproj              # Library (NuGet package)
├── TypeScriptGenerator.cs      # Main orchestrator
├── Configuration/              # Config models
├── Discovery/                  # Assembly loading & type discovery
├── Filters/                    # Type include/exclude filtering
├── Formatters/                 # TypeScript formatting (types, names, visibility)
├── Processing/                 # Post-processing cleanup
├── Utilities/                  # ConfigLoader, PathResolver
└── GenItEasy.CLI/
    ├── GenItEasy.CLI.csproj      # CLI tool (dotnet tool)
    └── Program.cs              # CLI entry point
```

## Dependencies

- **TypeLitePlus** 2.1.0 — TypeScript generation engine
- **Microsoft.Extensions.Logging.Abstractions** 9.0.0 — Optional logging interface
- **.NET 9.0**
