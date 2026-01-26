# Complete File Listing

## All Files in the Solution

### Root Files
- `TypeScriptGenerator.cs` - Main orchestrator class
- `GenItEasy.csproj` - Library project (NuGet package)
- `GenItEasy.slnx` - Solution file

### Configuration/
- `TypeScriptGenConfig.cs` - Main configuration model and NamespaceConfig

### Discovery/
- `TypeDiscovery.cs` - Assembly loading and type discovery

### Filters/
- `TypeFilter.cs` - Type filtering logic (include/exclude patterns)

### Formatters/
- `TypeFormatter.cs` - Type conversion and nullable handling
- `IdentifierFormatter.cs` - Naming conventions (camelCase, I prefix)
- `TypeVisibilityFormatter.cs` - Type visibility control

### Processing/
- `CodePostProcessor.cs` - Post-processing cleanup

### Utilities/
- `ConfigLoader.cs` - Configuration loading and validation
- `PathResolver.cs` - Output path resolution

### GenItEasy.CLI/
- `GenItEasy.CLI.csproj` - CLI tool project (dotnet tool)
- `Program.cs` - CLI entry point

## File Purposes Quick Reference

| File | Purpose |
|------|---------|
| TypeScriptGenerator.cs | Orchestrates all components |
| TypeDiscovery.cs | Loads assemblies from file path, finds types |
| TypeFilter.cs | Applies filtering rules |
| TypeFormatter.cs | Type conversions & nullables |
| IdentifierFormatter.cs | Naming conventions |
| TypeVisibilityFormatter.cs | Show/hide types |
| CodePostProcessor.cs | Cleanup generated code |
| PathResolver.cs | Resolve output paths (relative to working directory) |
| ConfigLoader.cs | Load & validate config |
| TypeScriptGenConfig.cs | Configuration models |
| Program.cs (CLI) | CLI entry point with argument parsing |

## Namespace Structure

```
GenItEasy
├── GenItEasy.Configuration
├── GenItEasy.Discovery
├── GenItEasy.Filters
├── GenItEasy.Formatters
├── GenItEasy.Processing
└── GenItEasy.Utilities
```

## Dependencies Between Components

```
TypeScriptGenerator
├─→ TypeDiscovery
│   └─→ TypeFilter
├─→ TypeFormatter
├─→ IdentifierFormatter
├─→ TypeVisibilityFormatter
├─→ PathResolver
└─→ CodePostProcessor

Program.cs (GenItEasy.CLI)
└─→ ConfigLoader
    └─→ TypeScriptGenConfig
└─→ TypeScriptGenerator
```
