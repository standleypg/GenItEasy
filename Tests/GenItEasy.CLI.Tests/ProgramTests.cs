using System.Diagnostics;

namespace GenItEasy.CLI.Tests;

/// <summary>
/// Integration tests for the GenItEasy CLI entry point.
/// These tests verify command-line argument parsing and exit code behavior.
/// </summary>
public class ProgramTests : IDisposable
{
    private readonly string _tempDir;
    private readonly List<string> _tempFiles = [];

    public ProgramTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"GenItEasy_CLI_Tests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            if (File.Exists(file))
            {
                try { File.Delete(file); } catch { }
            }
        }

        if (Directory.Exists(_tempDir))
        {
            try { Directory.Delete(_tempDir, true); } catch { }
        }
        GC.SuppressFinalize(this);
    }

    private string CreateTempConfigFile(string content)
    {
        var tempFile = Path.Combine(_tempDir, $"config_{Guid.NewGuid()}.json");
        File.WriteAllText(tempFile, content);
        _tempFiles.Add(tempFile);
        return tempFile;
    }

    [Fact]
    public void Program_WithNonExistentConfigFile_ReturnsExitCode1()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_tempDir, "does_not_exist.json");

        // Act
        var exitCode = RunCli(nonExistentPath);

        // Assert - FileNotFoundException returns exit code 1
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public void Program_WithInvalidConfig_MissingAssemblyName_ReturnsExitCode2()
    {
        // Arrange
        var json = """
        {
            "outputPath": "./output",
            "outputFileName": "types.ts",
            "namespaces": [
                { "namespace": "Test.Models" }
            ]
        }
        """;
        var configPath = CreateTempConfigFile(json);

        // Act
        var exitCode = RunCli(configPath);

        // Assert - InvalidOperationException (config error) returns exit code 2
        Assert.Equal(2, exitCode);
    }

    [Fact]
    public void Program_WithInvalidConfig_MissingOutputPath_ReturnsExitCode2()
    {
        // Arrange
        var json = """
        {
            "assemblyName": "Test.dll",
            "outputFileName": "types.ts",
            "namespaces": [
                { "namespace": "Test.Models" }
            ]
        }
        """;
        var configPath = CreateTempConfigFile(json);

        // Act
        var exitCode = RunCli(configPath);

        // Assert
        Assert.Equal(2, exitCode);
    }

    [Fact]
    public void Program_WithInvalidConfig_EmptyNamespaces_ReturnsExitCode2()
    {
        // Arrange
        var json = """
        {
            "assemblyName": "Test.dll",
            "outputPath": "./output",
            "outputFileName": "types.ts",
            "namespaces": []
        }
        """;
        var configPath = CreateTempConfigFile(json);

        // Act
        var exitCode = RunCli(configPath);

        // Assert
        Assert.Equal(2, exitCode);
    }

    [Fact]
    public void Program_WithValidConfig_NonExistentAssembly_ReturnsExitCode2()
    {
        // Arrange - Valid config but assembly doesn't exist
        var json = """
        {
            "assemblyName": "NonExistentAssembly.dll",
            "outputPath": "./output",
            "outputFileName": "types.ts",
            "namespaces": [
                { "namespace": "Test.Models" }
            ]
        }
        """;
        var configPath = CreateTempConfigFile(json);

        // Act
        var exitCode = RunCli(configPath);

        // Assert - Assembly not found throws InvalidOperationException, exit code 2
        Assert.Equal(2, exitCode);
    }

    [Fact]
    public void Program_WithBaseDirectoryArgument_ParsesArgument()
    {
        // Arrange - Valid config but assembly doesn't exist in specified base directory
        var json = """
        {
            "assemblyName": "Test.dll",
            "outputPath": "./output",
            "outputFileName": "types.ts",
            "namespaces": [
                { "namespace": "Test.Models" }
            ]
        }
        """;
        var configPath = CreateTempConfigFile(json);

        // Act - Pass --base-directory argument
        var exitCode = RunCli("--base-directory", _tempDir, configPath);

        // Assert - Should parse args and fail on assembly load (exit code 2)
        Assert.Equal(2, exitCode);
    }

    [Fact]
    public void Program_WithMalformedJson_ReturnsExitCode3()
    {
        // Arrange - Invalid JSON
        var json = "{ this is not valid json }";
        var configPath = CreateTempConfigFile(json);

        // Act
        var exitCode = RunCli(configPath);

        // Assert - JSON parse error is unexpected, exit code 3
        Assert.Equal(3, exitCode);
    }

    private static int RunCli(params string[] args)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{GetCliProjectPath()}\" --framework net9.0 -- {string.Join(" ", args.Select(a => $"\"{a}\""))}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start CLI process");
        }

        process.WaitForExit(30000); // 30 second timeout
        return process.ExitCode;
    }

    private static string GetCliProjectPath()
    {
        // Navigate from test output to CLI project
        var currentDir = AppContext.BaseDirectory;
        var solutionDir = FindSolutionDirectory(currentDir);
        return Path.Combine(solutionDir, "GenItEasy.CLI", "GenItEasy.CLI.csproj");
    }

    private static string FindSolutionDirectory(string startPath)
    {
        var dir = new DirectoryInfo(startPath);
        while (dir != null)
        {
            // Check for .sln or .slnx files
            if (dir.GetFiles("*.sln").Length > 0 || dir.GetFiles("*.slnx").Length > 0)
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        throw new InvalidOperationException("Could not find solution directory");
    }
}
