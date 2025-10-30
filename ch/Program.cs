using System.CommandLine;
using System.Diagnostics;

var bundleOption = new Option<FileInfo?>(
    "--output",
    description: "Output file for the bundled (optional, default: ./bundle.txt)"
);
bundleOption.AddAlias("-o");
bundleOption.AddValidator(result =>
{
    var file = result.GetValueOrDefault<FileInfo?>();
    if (file != null && string.IsNullOrWhiteSpace(file.FullName))
        result.ErrorMessage = "Invalid output file path.";
});

var bundleOptionLan = new Option<string[]>("--language", "Languages to include")
{
    IsRequired = true,
    AllowMultipleArgumentsPerToken = true
};
bundleOptionLan.AddAlias("-l");
bundleOptionLan.AddValidator(result =>
{
    var langs = result.GetValueOrDefault<string[]>();
    if (langs == null || langs.Length == 0)
        result.ErrorMessage = "You must specify at least one language, or 'all'.";
});

var bundleOptionNote = new Option<bool>("--note", "Add a note to the bundle");
bundleOptionNote.AddAlias("-n");

var bundleOptionSort = new Option<string>(
    new[] { "--sort", "-s" },
    () => "name",
    "Sort files by 'name' or 'type'"
);
bundleOptionSort.AddValidator(result =>
{
    var value = result.GetValueOrDefault<string>();
    if (result.Tokens.Count > 0 && value != "name" && value != "type")
    {
        result.ErrorMessage = "Invalid sort option. Use 'name' or 'type'.";
    }
});

var bundleOptionRemoveEmptyLines = new Option<bool>("--remove-empty-lines", "Remove empty lines from the files before bundling");
bundleOptionRemoveEmptyLines.AddAlias("-r");

var bundleOptionAuthor = new Option<string>("--author", "Author of the bundle");
bundleOptionAuthor.AddAlias("-a");
bundleOptionAuthor.AddValidator(result =>
{
    var author = result.GetValueOrDefault<string>();
    if (!string.IsNullOrWhiteSpace(author) && author.Length > 100)
        result.ErrorMessage = "Author name is too long (max 100 characters).";
});

var bundleCommand = new Command("bundle", "Bundle many files to one");
bundleCommand.AddOption(bundleOption);
bundleCommand.AddOption(bundleOptionLan);
bundleCommand.AddOption(bundleOptionNote);
bundleCommand.AddOption(bundleOptionSort);
bundleCommand.AddOption(bundleOptionRemoveEmptyLines);
bundleCommand.AddOption(bundleOptionAuthor);

bundleCommand.SetHandler((output, language, note, sort, removeEmptyLines, author) =>
{
    try
    {
        string sourceFolder = Directory.GetCurrentDirectory();

        if (language.Contains("all", StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine("Including all languages...");
        }
        else
        {
            Console.WriteLine("Including only the following languages:");
            foreach (var lang in language)
                Console.WriteLine($" - {lang}");
        }

        if (output == null)
        {
            output = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "bundle.txt"));
            Console.WriteLine($"No output path specified. Using default: {output.FullName}");
        }

        File.Create(output.FullName).Dispose();

        if (!string.IsNullOrWhiteSpace(author))
            File.AppendAllText(output.FullName, $"// Author: {author}\n\n");

        var files = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories)
            .Where(f => !f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)
                     && !f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar)
                     && !f.Contains(Path.DirectorySeparatorChar + "Debug" + Path.DirectorySeparatorChar)
                     && !f.Contains(Path.DirectorySeparatorChar + "Release" + Path.DirectorySeparatorChar))
            .ToArray();

        if (!language.Contains("all", StringComparer.OrdinalIgnoreCase))
        {
            files = files.Where(f => language.Any(lang => f.EndsWith($".{lang}", StringComparison.OrdinalIgnoreCase)))
                         .ToArray();
        }

        files = sort.ToLower() switch
        {
            "type" => files.OrderBy(f => Path.GetExtension(f)).ThenBy(f => Path.GetFileName(f)).ToArray(),
            _ => files.OrderBy(f => Path.GetFileName(f)).ToArray()
        };

        foreach (var file in files)
        {
            string relativePath = Path.GetRelativePath(sourceFolder, file);

            if (note)
                File.AppendAllText(output.FullName, $"// Source: {relativePath}\n");

            string content = File.ReadAllText(file);

            if (removeEmptyLines)
            {
                content = string.Join(
                    "\n",
                    content.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line))
                );
            }

            File.AppendAllText(output.FullName, content + "\n\n");
        }

        Console.WriteLine($"Bundled {files.Length} files successfully into {output.FullName}");

    }
    catch (DirectoryNotFoundException)
    {
        Console.WriteLine("error: mistake in path...");
    }
}, bundleOption, bundleOptionLan, bundleOptionNote, bundleOptionSort, bundleOptionRemoveEmptyLines, bundleOptionAuthor);

var rootCommand = new RootCommand("A simple command-line application");
rootCommand.AddCommand(bundleCommand);

var createRspCommand = new Command("create-rsp", "Create a response file with a prebuilt bundle command");

createRspCommand.SetHandler(() =>
{
    Console.WriteLine("Creating a response file for the bundle command.");

    Console.Write("Enter output file path (e.g., bundle.txt): ");
    string output = Console.ReadLine()?.Trim();

    Console.Write("Enter languages (comma separated) or 'all': ");
    string languages = Console.ReadLine()?.Trim();

    Console.Write("Include source notes? (yes/no): ");
    string noteInput = Console.ReadLine()?.Trim();
    string note = (noteInput?.ToLower() == "yes") ? "--note" : "";

    Console.Write("Sort by 'name' or 'type' [default: name]: ");
    string sortInput = Console.ReadLine()?.Trim();
    string sort = string.IsNullOrWhiteSpace(sortInput) ? "" : $"--sort {sortInput}";

    Console.Write("Remove empty lines? (yes/no): ");
    string removeEmptyInput = Console.ReadLine()?.Trim();
    string removeEmpty = (removeEmptyInput?.ToLower() == "yes") ? "--remove-empty-lines" : "";

    Console.Write("Author name (leave empty if none): ");
    string authorName = Console.ReadLine()?.Trim();
    string author = string.IsNullOrWhiteSpace(authorName) ? "" : $"--author \"{authorName}\"";

    string command = $"dotnet run -- bundle --output \"{output}\" --language {languages} {note} {sort} {removeEmpty} {author}";

    Console.Write("Enter response file name (e.g., bundle.rsp): ");
    string rspFileName = Console.ReadLine()?.Trim();

    File.WriteAllText(rspFileName, command);
    Console.WriteLine($"Response file '{rspFileName}' created successfully!");
});
rootCommand.AddCommand(createRspCommand);

rootCommand.InvokeAsync(args);



