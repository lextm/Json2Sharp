using Json2SharpApp.Handlers;
using System.CommandLine;

namespace Json2SharpApp;

/// <summary>
/// Entry point class.
/// </summary>
internal sealed class Program
{
    /// <summary>
    /// Entry point.
    /// </summary>
    /// <param name="args">CLI arguments.</param>
    /// <returns>Exit code.</returns>
    private async static Task<int> Main(string[] args)
    {
        var inputOption = new Option<FileInfo?>(new[] { "--input", "-i" }, "The relative path to the JSON file in the file system.");
        var outputOption = new Option<string?>(new[] { "--output", "-o" }, "The relative path to the resulting file in the file system.");
        var configOption = new Option<string?>(new[] { "--config", "-c" }, "The conversion options.");
        var rootCommand = new RootCommand("Convert a JSON object to a language type declaration.")
        {
            inputOption,
            outputOption,
            configOption
        };

        rootCommand.SetHandler(
            async (inputFile, outputPath, configOptions) => await RootHandlerAsync(rootCommand, inputFile, outputPath, configOptions),
            inputOption, outputOption, configOption
        );

        return await rootCommand.InvokeAsync(args);
    }

    /// <summary>
    /// Handles invocation of the root command.
    /// </summary>
    /// <param name="rootCommand">The root command.</param>
    /// <param name="inputFile">The file that contains the JSON data or <see langword="null"/> if data is being piped.</param>
    /// <param name="outputPath">
    /// The path where the file should be created or <see langword="null"/>
    /// if the output should be printed to stdout.
    /// </param>
    /// <param name="configOptions">The command-line configuration options.</param>
    private async static Task RootHandlerAsync(RootCommand rootCommand, FileInfo? inputFile, string? outputPath, string? configOptions)
    {
        var options = ConfigHandler.Handle(configOptions?.Split(' ', StringSplitOptions.TrimEntries) ?? Array.Empty<string>());
        var inputSuccessful = InputHandler.Handle(inputFile, options, out var typeDefinition);

        if (inputSuccessful is not null)
        {
            if (!await OutputHandler.HandleAsync(outputPath, typeDefinition, !inputSuccessful.Value, options.TargetLanguage))
                await OutputHandler.StderrWriteAsync("No permission to write on output folder.", ConsoleColor.Red);
        }
        else
        {
            await OutputHandler.StderrWriteAsync("Error: no input was provided." + Environment.NewLine, ConsoleColor.Red);
            await rootCommand.InvokeAsync("--help");
        }
    }
}