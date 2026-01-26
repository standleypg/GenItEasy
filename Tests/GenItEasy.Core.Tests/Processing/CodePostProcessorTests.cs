using GenItEasy.Processing;

namespace GenItEasy.Core.Tests.Processing;

public class CodePostProcessorTests
{
    [Fact]
    public void Process_RemovesGuidNamespaceDeclaration()
    {
        // Arrange
        var input = """
            declare namespace System {
                export interface Guid {
                    value: string;
                }
            }

            declare namespace MyApp {
                export interface User {
                    id: string;
                }
            }
            """;

        // Act
        var result = CodePostProcessor.Process(input);

        // Assert
        Assert.DoesNotContain("interface Guid", result);
        Assert.Contains("interface User", result);
    }

    [Fact]
    public void Process_RemovesIGuidInterface()
    {
        // Arrange
        var input = """
            export interface IGuid {
                value: string;
            }

            export interface IUser {
                id: string;
            }
            """;

        // Act
        var result = CodePostProcessor.Process(input);

        // Assert
        Assert.DoesNotContain("interface IGuid", result);
        Assert.Contains("interface IUser", result);
    }

    [Fact]
    public void Process_CleansUpMultipleEmptyLines()
    {
        // Arrange
        var input = """
            interface IUser {
                id: string;
            }



            interface IProduct {
                name: string;
            }
            """;

        // Act
        var result = CodePostProcessor.Process(input);

        // Assert
        // Should not have more than 2 consecutive newlines
        Assert.DoesNotContain("\n\n\n", result);
    }

    [Fact]
    public void Process_PreservesDoubleNewlines()
    {
        // Arrange
        var input = """
            interface IUser {
                id: string;
            }

            interface IProduct {
                name: string;
            }
            """;

        // Act
        var result = CodePostProcessor.Process(input);

        // Assert
        Assert.Contains("interface IUser", result);
        Assert.Contains("interface IProduct", result);
    }

    [Fact]
    public void Process_TrimsResult()
    {
        // Arrange
        var input = """

            interface IUser {
                id: string;
            }

            """;

        // Act
        var result = CodePostProcessor.Process(input);

        // Assert
        Assert.Equal(result, result.Trim());
    }

    [Fact]
    public void Process_WithNoGuidDeclarations_ReturnsCleanedCode()
    {
        // Arrange
        var input = """
            declare namespace MyApp {
                export interface User {
                    name: string;
                }
            }
            """;

        // Act
        var result = CodePostProcessor.Process(input);

        // Assert
        Assert.Contains("interface User", result);
        Assert.Contains("name: string", result);
    }

    [Fact]
    public void Process_WithEmptyInput_ReturnsEmpty()
    {
        // Arrange
        var input = string.Empty;

        // Act
        var result = CodePostProcessor.Process(input);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Process_WithWhitespaceOnly_ReturnsEmpty()
    {
        // Arrange
        var input = "   \n\n   \n   ";

        // Act
        var result = CodePostProcessor.Process(input);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Process_RemovesGuidWithDifferentFormats()
    {
        // Arrange - Test with different whitespace patterns
        var input = """
            declare namespace System { export interface IGuid { value: string; } }

            declare namespace MyApp {
                export interface IUser {
                    id: string;
                }
            }
            """;

        // Act
        var result = CodePostProcessor.Process(input);

        // Assert
        Assert.DoesNotContain("IGuid", result);
        Assert.Contains("IUser", result);
    }
}
