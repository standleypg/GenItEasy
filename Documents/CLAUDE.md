# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Run

```bash
# Build the solution
dotnet build GenItEasy.slnx

# Run all tests
dotnet test GenItEasy.slnx

# Pack the library
dotnet pack GenItEasy.Core/GenItEasy.Core.csproj

# Pack the CLI tool
dotnet pack GenItEasy.CLI/GenItEasy.CLI.csproj

# Run the CLI tool locally
dotnet run --project GenItEasy.CLI -- typescriptgenconfig.json --base-directory path/to/dlls
```

## Architecture

This is a multi-target .NET class library (NuGet package) and CLI tool (dotnet tool) that generates TypeScript type definitions from C# assemblies. The library uses a custom internal TypeScript generation engine (no external dependencies for generation).

### Pipeline

```
GenItEasy.CLI/Program.cs (CLI entry, exit codes 0-3)
  → ConfigLoader (load + validate typescriptgenconfig.json)
  → Applies --base-directory override
  → TypeScriptGenerator.Generate() orchestrates:
      1. TypeDiscovery: Load assembly from file path, find types in configured namespaces
         └── TypeFilter: Apply include/exclude patterns, generic type filters
      2. TsModelBuilder (internal): Build TypeScript model from discovered types
         └── Creates TsModel with TsModule, TsClass, TsEnum, TsProperty
      3. Configure TsGenerator with formatters:
         - IdentifierFormatter: PascalCase→camelCase properties, I-prefix interfaces
         - TypeFormatter: Guid→string, DateTime→Date, nullable→union with null
         - TypeVisibilityFormatter: Hide System.Guid declarations
         - ModuleNameFormatter: Apply outputNamespace consolidation
      4. TsGenerator.Generate(): Generate TypeScript code
         └── Supports enumStyle: Numeric, String, StringLiteral
         └── Handles tuples, generics, cross-namespace references
      5. CodePostProcessor: Remove leftover Guid refs, clean whitespace
      6. PathResolver: Resolve output path (relative to current working directory)
      7. Write .ts file
```

### Key Design Decisions

- **Include-by-default**: All public types in configured namespaces are exported unless filtered out
- **Pattern matching filters**: Support prefix, suffix, and exact match for type inclusion/exclusion
- **Nullable handling**: Detects both `Nullable<T>` value types and nullable reference types, maps to `| null` unions
- **Two entry points**: CLI via `genit` tool or programmatic via `new TypeScriptGenerator(config, logger?).Generate()`
- **Separated concerns**: `baseDirectory` controls assembly loading path; output path resolves relative to working directory
- **Optional logging**: `ILogger` parameter defaults to `NullLogger` when not provided
- **Namespace consolidation**: `outputNamespace` merges all types into a single namespace with simple references
- **Enum flexibility**: Three enum styles - numeric, string, or string literal union types

### Configuration Shape (typescriptgenconfig.json)

- `assemblyName` (required): Assembly name or DLL filename to load
- `outputPath` / `outputFileName` (required): Where to write generated .ts (relative to working directory)
- `outputNamespace` (optional): Consolidate all types into this single namespace
- `enumStyle` (optional, default "Numeric"): "Numeric", "String", or "StringLiteral"
- `baseDirectory` (optional): Directory to find the assembly DLL (overridden by `--base-directory` CLI flag)
- `includeStaticClasses` (optional, default false)
- `namespaces[]`: Each has `namespace`, optional `includeNested`, `includeTypes`, `excludeTypes`, `includeGenericTypes`, `excludeGenericTypes`

### TypeScript Generation Engine (Internal)

Located in `GenItEasy.Core/TypeScript/`:

```
TypeScript/
├── Models/
│   ├── ITsType.cs          # Common interface for TsClass/TsEnum
│   ├── TsProperty.cs       # Property wrapper
│   ├── TsClass.cs          # Class/interface representation
│   ├── TsEnum.cs           # Enum representation
│   ├── TsEnumValue.cs      # Enum member
│   ├── TsModule.cs         # Namespace/module container
│   ├── TsModel.cs          # Root model
│   └── TsGeneratorOutput.cs # Output flags (Properties, Enums)
├── TsModelBuilder.cs       # Builds TsModel from C# types
└── TsGenerator.cs          # Generates TypeScript code
```
