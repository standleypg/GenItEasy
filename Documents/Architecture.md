# TypeScript Generator - Architecture

## Project Structure

```
GenItEasy/
├── Configuration/           # Configuration models
│   └── TypeScriptGenConfig.cs  # Config with EnumStyle enum
│
├── Discovery/              # Type discovery from assemblies
│   └── TypeDiscovery.cs   # Loads assemblies and finds types
│
├── Filters/               # Type filtering logic
│   └── TypeFilter.cs      # Include/exclude pattern matching
│
├── Formatters/            # TypeScript formatting
│   ├── TypeFormatter.cs          # Type conversions (Guid→string, etc.)
│   ├── IdentifierFormatter.cs    # Naming (camelCase, I prefix)
│   └── TypeVisibilityFormatter.cs # Show/hide types
│
├── Processing/            # Post-processing
│   └── CodePostProcessor.cs # Cleanup generated code
│
├── TypeScript/            # Internal TypeScript generation engine
│   ├── Models/
│   │   ├── ITsType.cs           # Common interface for types
│   │   ├── TsProperty.cs        # Property wrapper
│   │   ├── TsClass.cs           # Class/interface model
│   │   ├── TsEnum.cs            # Enum model
│   │   ├── TsEnumValue.cs       # Enum member
│   │   ├── TsModule.cs          # Namespace container
│   │   ├── TsModel.cs           # Root model
│   │   └── TsGeneratorOutput.cs # Output flags
│   ├── TsModelBuilder.cs        # Builds model from C# types
│   └── TsGenerator.cs           # Generates TypeScript code
│
├── Utilities/            # Helper utilities
│   ├── ConfigLoader.cs  # Configuration loading and validation
│   └── PathResolver.cs  # Output path resolution
│
├── TypeScriptGenerator.cs # Main orchestrator
│
└── GenItEasy.CLI/           # CLI tool (dotnet tool)
    └── Program.cs         # CLI entry point
```

## Component Responsibilities

### TypeScriptGenerator (Main Orchestrator)
- **Purpose**: Coordinates all components to generate TypeScript
- **Responsibilities**:
    - Orchestrates the generation workflow
    - Configures TsGenerator with formatters
    - Writes output files
- **Dependencies**: All other components

### Configuration
- **Purpose**: Configuration models
- **Components**:
    - `TypeScriptGenConfig`: Main configuration (includes `EnumStyle` enum, `OutputNamespace`)
    - `NamespaceConfig`: Per-namespace settings
    - `EnumStyle`: Enum for generation style (Numeric, String, StringLiteral)

### Discovery/TypeDiscovery
- **Purpose**: Finds types in assemblies
- **Responsibilities**:
    - Loads assemblies from file path (resolved via `baseDirectory` or current directory)
    - Discovers types matching namespace patterns
    - Delegates filtering to TypeFilter

### Filters/TypeFilter
- **Purpose**: Applies include/exclude rules
- **Responsibilities**:
    - Namespace matching (exact and nested)
    - Static class detection
    - Include/exclude pattern matching
    - Generic type filtering

### Formatters
#### TypeFormatter
- **Purpose**: Type conversion and nullable handling
- **Responsibilities**:
    - Maps C# types to TypeScript (Guid→string, DateTime→Date)
    - Detects nullable types
    - Adds `| null` for nullables

#### IdentifierFormatter
- **Purpose**: Naming conventions
- **Responsibilities**:
    - Converts properties to camelCase
    - Adds 'I' prefix to interfaces (not enums)
    - Detects generic type parameters

#### TypeVisibilityFormatter
- **Purpose**: Controls which types appear in output
- **Responsibilities**:
    - Hides Guid type definitions
    - Can be extended for other hiding rules

### Processing/CodePostProcessor
- **Purpose**: Cleans up generated code
- **Responsibilities**:
    - Removes leftover Guid declarations
    - Cleans up extra whitespace
    - Regex-based text cleanup

### TypeScript/Models
- **Purpose**: Internal model types for TypeScript generation
- **Components**:
    - `ITsType`: Common interface for TsClass and TsEnum (provides `Name` property)
    - `TsProperty`: Wraps property info with Name and PropertyType
    - `TsClass`: Represents a class/interface with Properties and TypeParameters
    - `TsEnum`: Represents an enum with Values
    - `TsEnumValue`: Enum member with Name and numeric Value
    - `TsModule`: Namespace container with Classes and Enums
    - `TsModel`: Root model containing Modules
    - `TsGeneratorOutput`: Flags enum (Properties, Enums)

### TypeScript/TsModelBuilder
- **Purpose**: Builds TypeScript model from C# types
- **Responsibilities**:
    - Accepts types via `Add(Type)` and `Add<T>()`
    - Scans properties and fields
    - Extracts generic type parameters
    - Groups types by namespace into modules
    - Returns `TsModel` via `Build()`

### TypeScript/TsGenerator
- **Purpose**: Generates TypeScript code from model
- **Responsibilities**:
    - Accepts formatter callbacks (identifier, member type, module name, visibility)
    - Handles enum styles (Numeric, String, StringLiteral)
    - Maps C# types to TypeScript (tuples, collections, dictionaries, primitives)
    - Resolves cross-namespace type references
    - Consolidates modules when `outputNamespace` is used

### Utilities/PathResolver
- **Purpose**: Resolves output file paths
- **Responsibilities**:
    - Resolves relative output paths against the current working directory
    - Creates output directories

### Utilities/ConfigLoader
- **Purpose**: Loads and validates configuration
- **Responsibilities**:
    - Reads and deserializes JSON config files
    - Validates required configuration properties

### GenItEasy.CLI/Program
- **Purpose**: CLI entry point (distributed as a dotnet tool)
- **Responsibilities**:
    - Parses CLI arguments (`configPath`, `--base-directory`)
    - Sets up logging and invokes the generator

## Data Flow

```
1. Load Config (ConfigLoader)
   ↓
2. TypeDiscovery.LoadAssembly()
   ↓
3. TypeDiscovery.DiscoverTypes()
   ├→ TypeFilter.IsInTargetNamespace()
   ├→ TypeFilter.IsNotStaticClass()
   └→ TypeFilter.IsTypeIncluded()
   ↓
4. TsModelBuilder.Add(types) + Build()
   └→ Creates TsModel with TsModules, TsClasses, TsEnums
   ↓
5. ConfigureTsGenerator()
   ├→ SetIdentifierFormatter (IdentifierFormatter)
   ├→ SetMemberTypeFormatter (TypeFormatter)
   ├→ SetModuleNameFormatter (IdentifierFormatter)
   └→ SetTypeVisibilityFormatter (TypeVisibilityFormatter)
   ↓
6. TsGenerator.Generate(model, output)
   ├→ Apply EnumStyle (Numeric/String/StringLiteral)
   └→ Handle namespace consolidation (outputNamespace)
   ↓
7. CodePostProcessor.Process()
   ↓
8. PathResolver.ResolveOutputPath()
   ↓
9. Write File
```

## Adding New Features

### Add a New Type Mapping
**File**: `Formatters/TypeFormatter.cs`
```csharp
return typeName switch
{
    "System.DateTime" => "Date",
    "YourType" => "YourTsType",  // Add here
    _ => typeName
};
```

### Add a New Exclusion Rule
**File**: `Filters/TypeFilter.cs`
```csharp
public bool IsTypeIncluded(Type type, NamespaceConfig nsConfig)
{
    // Add your custom filter logic here
}
```

### Add Post-Processing Cleanup
**File**: `Processing/CodePostProcessor.cs`
```csharp
public static string Process(string tsCode)
{
    // Add regex cleanup here
}
```

### Change Naming Convention
**File**: `Formatters/IdentifierFormatter.cs`
```csharp
public string FormatPropertyName(TsProperty formatter)
{
    // Modify naming logic here
}
```

## Benefits of This Structure

**Single Responsibility**: Each class has one clear purpose
**Easy to Test**: Components can be unit tested independently
**Easy to Extend**: New features go in obvious places
**Easy to Understand**: Clear separation of concerns
**Easy to Maintain**: Changes are localized to specific files
**Reusable**: Components can be used independently

## Migration from Old Code

The old monolithic `TypeScriptGenerator.cs` has been split into:
- **Type discovery** → `Discovery/TypeDiscovery.cs`
- **Filtering logic** → `Filters/TypeFilter.cs`
- **Type formatting** → `Formatters/TypeFormatter.cs`
- **Naming** → `Formatters/IdentifierFormatter.cs`
- **Visibility** → `Formatters/TypeVisibilityFormatter.cs`
- **Post-processing** → `Processing/CodePostProcessor.cs`
- **Path handling** → `Utilities/PathResolver.cs`
- **Orchestration** → `TypeScriptGenerator.cs` (now ~130 lines instead of 463!)

The public API: instantiate `TypeScriptGenerator` with a config and optionally a logger, then call `Generate()`.
A CLI tool (`GenItEasy.CLI`) is also available as a dotnet tool for build-time integration.
