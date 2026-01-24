# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Run

```bash
# Build the solution
dotnet build GenItEasy.slnx

# Pack the library
dotnet pack GenItEasy.csproj

# Pack the CLI tool
dotnet pack GenItEasy.CLI/GenItEasy.CLI.csproj

# Run the CLI tool locally
dotnet run --project GenItEasy.CLI -- typescriptgenconfig.json --base-directory path/to/dlls
```

No test suite or linter is configured.

## Architecture

This is a .NET 9.0 class library (NuGet package) and CLI tool (dotnet tool) that generates TypeScript type definitions from C# assemblies using TypeLitePlus.

### Pipeline

```
GenItEasy.CLI/Program.cs (CLI entry, exit codes 0-3)
  → ConfigLoader (load + validate typescriptgenconfig.json)
  → Applies --base-directory override
  → TypeScriptGenerator.Generate() orchestrates:
      1. TypeDiscovery: Load assembly from file path, find types in configured namespaces
         └── TypeFilter: Apply include/exclude patterns, generic type filters
      2. TsModelBuilder (TypeLitePlus): Build TypeScript model from discovered types
      3. Configure TsGenerator with formatters:
         - IdentifierFormatter: PascalCase→camelCase properties, I-prefix interfaces
         - TypeFormatter: Guid→string, DateTime→Date, nullable→union with null
         - TypeVisibilityFormatter: Hide System.Guid declarations
      4. CodePostProcessor: Remove leftover Guid refs, clean whitespace
      5. PathResolver: Resolve output path (relative to current working directory)
      6. Write .ts file
```

### Key Design Decisions

- **Include-by-default**: All public types in configured namespaces are exported unless filtered out
- **Pattern matching filters**: Support prefix, suffix, and exact match for type inclusion/exclusion
- **Nullable handling**: Detects both `Nullable<T>` value types and nullable reference types, maps to `| null` unions
- **Two entry points**: CLI via `dotnet GenItEasy` tool or programmatic via `new TypeScriptGenerator(config, logger?).Generate()`
- **Separated concerns**: `baseDirectory` controls assembly loading path; output path resolves relative to working directory
- **Optional logging**: `ILogger` parameter defaults to `NullLogger` when not provided

### Configuration Shape (typescriptgenconfig.json)

- `assemblyName` (required): Assembly name or DLL filename to load
- `outputPath` / `outputFileName` (required): Where to write generated .ts (relative to working directory)
- `baseDirectory` (optional): Directory to find the assembly DLL (overridden by `--base-directory` CLI flag)
- `includeStaticClasses` (optional, default false)
- `namespaces[]`: Each has `namespace`, optional `includeNested`, `includeTypes`, `excludeTypes`, `includeGenericTypes`, `excludeGenericTypes`
