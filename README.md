# 🗂️ ch — Code Bundler CLI (.NET)

A command-line tool built with C# and .NET 8 that bundles multiple source code files into a single output file.  
Developed using the `System.CommandLine` library.

## 💡 What It Does

Instead of manually copying code from multiple files, `ch` lets you run a single command that collects all relevant source files from a directory, filters them by language, and merges them into one clean output file — with optional sorting, author info, source notes, and empty-line removal.

## 🛠️ Tech Stack

- **Language:** C#, .NET 8
- **Library:** [`System.CommandLine`](https://learn.microsoft.com/en-us/dotnet/standard/commandline/) (beta)

## 🚀 Getting Started

### Run with dotnet
```bash
dotnet run -- bundle --language cs --output result.txt
```

### Or publish and add to PATH
```bash
dotnet publish -c Release -o ./publish
# Then add ./publish to your system PATH environment variable
```

After adding to PATH, run from anywhere:
```bash
ch bundle --language cs --output result.txt
```

---

## 📋 Commands

### `bundle` — Bundle source files into one

Scans the current directory recursively, filters files by language, and merges them into a single output file.  
Automatically skips `bin/`, `obj/`, `Debug/`, and `Release/` folders.

| Option | Alias | Description | Required |
|--------|-------|-------------|----------|
| `--language` | `-l` | Languages to include (e.g. `cs py js`), or `all` | ✅ Yes |
| `--output` | `-o` | Output file path (default: `./bundle.txt`) | No |
| `--note` | `-n` | Add source file path as a comment above each file's content | No |
| `--sort` | `-s` | Sort by `name` (default) or `type` | No |
| `--remove-empty-lines` | `-r` | Strip empty lines from each file before bundling | No |
| `--author` | `-a` | Add author name as a comment at the top of the output file | No |

#### Examples

```bash
# Bundle all C# files
ch bundle -l cs

# Bundle C# and JavaScript files, sorted by type, with source notes
ch bundle -l cs js -s type -n -o output.txt

# Bundle everything, add author, remove empty lines
ch bundle -l all -a "Chaya Babayof" -r -o bundle.txt
```

---

### `create-rsp` — Interactive response file generator

Walks you through a guided prompt to build a full `bundle` command, then saves it as a `.rsp` response file.

```bash
ch create-rsp
```

You will be prompted for each option interactively. The result is saved as a `.rsp` file that you can reuse:

```bash
dotnet @bundle.rsp
```

This is especially useful for long commands you want to run repeatedly without retyping.

---

## 📁 Project Structure

```
CLI.NET/
├── ch/
│   ├── Program.cs      # CLI logic — bundle & create-rsp commands
│   └── ch.csproj       # .NET 8 project file
├── ch.sln
└── .gitignore
```
