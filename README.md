# GenItEasy

A .NET library and CLI tool for generating TypeScript type definitions from C# assemblies.

## Installation

### As a Library

```bash
dotnet add package GenItEasy
```

### As a CLI Tool

```bash
dotnet tool install GenItEasy.CLI
```

## Quick Start

### 1. Create a configuration file

Create `typescriptgenconfig.json`:

```json
{
  "assemblyName": "MyApp.Models",
  "outputPath": "../ClientApp/src/types",
  "outputFileName": "models.gen.ts",
  "namespaces": [
    {
      "namespace": "MyApp.Models.Dtos"
    }
  ]
}
```

### 2. Run the generator

**CLI:**
```bash
genit typescriptgenconfig.json --base-directory bin/Debug/net9.0
```

**Library:**
```csharp
using GenItEasy;
using GenItEasy.Utilities;

var config = ConfigLoader.LoadConfig("typescriptgenconfig.json");
var generator = new TypeScriptGenerator(config);
generator.Generate();
```

**MSBuild (auto-generate on build):**
```xml
<Target Name="GenerateTypeScript" AfterTargets="Build">
  <Exec Command="genit typescriptgenconfig.json --base-directory $(TargetDir)" />
</Target>
```

## Documentation

For comprehensive documentation, see:

- **[User Guide](Documents/UserGuide.md)** - Complete configuration options, type mappings, filtering patterns, examples, and troubleshooting
- **[Examples](Documents/Examples.md)** - C# to TypeScript transformation examples
- **[Architecture](Documents/Architecture.md)** - Project structure and component design

## Key Features

- Configurable namespace filtering with include/exclude patterns
- Support for generic types, tuples, collections, and dictionaries
- Multiple enum styles: Numeric, String, or StringLiteral
- Optional namespace consolidation via `outputNamespace`
- Automatic `I` prefix for interfaces, camelCase property conversion
- Nullable type support (`T?` → `T | null`)

## Dependencies

- **Microsoft.Extensions.Logging.Abstractions** - Optional logging interface
- **.NET 8.0, 9.0, 10.0** - Multi-targeting support
