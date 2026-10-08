using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RobloxAPI.Generator;

public static class Program
{
    private const string DefaultDumpUrl = "https://raw.githubusercontent.com/MaximumADHD/Roblox-Client-Tracker/roblox/API-Dump.json";

    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=================================================");
        Console.WriteLine("   [Roblox C# Generator] RobloxAPI Type Builder  ");
        Console.WriteLine("=================================================");
        Console.ResetColor();

        bool forceFetch = false;
        string? inputDumpPath = null;
        string? outputCsPath = null;

        foreach (var arg in args)
        {
            if (arg.Equals("--fetch", StringComparison.OrdinalIgnoreCase) || arg.Equals("-f", StringComparison.OrdinalIgnoreCase))
            {
                forceFetch = true;
            }
            else if (arg.Equals("--help", StringComparison.OrdinalIgnoreCase) || arg.Equals("-h", StringComparison.OrdinalIgnoreCase) || arg.Equals("-?"))
            {
                Console.WriteLine("Usage: RobloxAPI.Generator [options] [input-dump.json] [output-RobloxAPI.cs]");
                Console.WriteLine("Options:");
                Console.WriteLine("  --fetch, -f    Fetch latest api-dump.json directly from GitHub tracker");
                Console.WriteLine("  --help, -h     Show this help message");
                return 0;
            }
            else if (inputDumpPath == null)
            {
                inputDumpPath = arg;
            }
            else if (outputCsPath == null)
            {
                outputCsPath = arg;
            }
        }

        // Determine base directories
        var currentDir = Path.GetFullPath(Directory.GetCurrentDirectory());
        var baseDir = Path.GetFullPath(AppContext.BaseDirectory);

        // Find the RobloxAPI directory
        string robloxApiDir;
        if (Directory.Exists(Path.Combine(currentDir, "RobloxAPI")))
        {
            robloxApiDir = Path.Combine(currentDir, "RobloxAPI");
        }
        else if (File.Exists(Path.Combine(currentDir, "RobloxAPI.csproj")))
        {
            robloxApiDir = currentDir;
        }
        else if (File.Exists(Path.Combine(currentDir, "..", "RobloxAPI.csproj")))
        {
            robloxApiDir = Path.GetFullPath(Path.Combine(currentDir, ".."));
        }
        else if (Directory.Exists(Path.Combine(baseDir, "..", "..", "..", "RobloxAPI")))
        {
            robloxApiDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "RobloxAPI"));
        }
        else if (File.Exists(Path.Combine(baseDir, "..", "..", "..", "RobloxAPI.csproj")))
        {
            robloxApiDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
        }
        else
        {
            robloxApiDir = currentDir;
        }

        string? resolvedDumpPath = null;
        if (!string.IsNullOrEmpty(inputDumpPath))
        {
            resolvedDumpPath = Path.GetFullPath(inputDumpPath);
        }
        else
        {
            var candidateDumpPaths = new[]
            {
                Path.Combine(robloxApiDir, "api-dump.json"),
                Path.Combine(currentDir, "api-dump.json"),
                Path.Combine(currentDir, "..", "api-dump.json")
            };

            foreach (var p in candidateDumpPaths)
            {
                if (File.Exists(p))
                {
                    resolvedDumpPath = Path.GetFullPath(p);
                    break;
                }
            }

            resolvedDumpPath ??= Path.Combine(robloxApiDir, "api-dump.json");
        }

        string jsonContent;

        if (forceFetch || !File.Exists(resolvedDumpPath))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"🌐 Fetching latest Roblox API-Dump from {DefaultDumpUrl}...");
            Console.ResetColor();

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("RobloxAPI.Generator/1.0");
                jsonContent = await client.GetStringAsync(DefaultDumpUrl);

                var savePath = resolvedDumpPath;
                var saveDir = Path.GetDirectoryName(savePath);
                if (!string.IsNullOrEmpty(saveDir) && !Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }
                await File.WriteAllTextAsync(savePath, jsonContent, Encoding.UTF8);
                Console.WriteLine($"💾 Saved fresh api-dump.json to: {savePath}");
            }
            catch (Exception ex)
            {
                if (File.Exists(resolvedDumpPath))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"⚠️ Failed to fetch remote dump ({ex.Message}), falling back to local: {resolvedDumpPath}");
                    Console.ResetColor();
                    jsonContent = await File.ReadAllTextAsync(resolvedDumpPath, Encoding.UTF8);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ Error fetching API-Dump: {ex.Message}");
                    Console.ResetColor();
                    return 1;
                }
            }
        }
        else
        {
            Console.WriteLine($"📖 Reading local API dump from: {resolvedDumpPath}");
            jsonContent = await File.ReadAllTextAsync(resolvedDumpPath, Encoding.UTF8);
        }

        // Determine output path
        if (string.IsNullOrEmpty(outputCsPath))
        {
            outputCsPath = Path.Combine(robloxApiDir, "RobloxAPI.cs");
        }

        outputCsPath = Path.GetFullPath(outputCsPath);

        Console.WriteLine($"⚙️  Parsing API dump JSON ({(jsonContent.Length / 1024.0 / 1024.0):F2} MB)...");
        var sw = Stopwatch.StartNew();

        ApiDump? dump;
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
            dump = JsonSerializer.Deserialize<ApiDump>(jsonContent, jsonOptions);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Failed to parse JSON: {ex.Message}");
            Console.ResetColor();
            return 1;
        }

        if (dump == null || dump.Classes.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ API dump is empty or invalid.");
            Console.ResetColor();
            return 1;
        }

        Console.WriteLine($"   Found {dump.Classes.Count} classes, {dump.Enums.Count} enums (API Version: {dump.Version})");
        Console.WriteLine("🔨 Generating C# bindings...");

        var generatedCode = CodeGenerator.Generate(dump);

        var outDir = Path.GetDirectoryName(outputCsPath);
        if (!string.IsNullOrEmpty(outDir) && !Directory.Exists(outDir))
        {
            Directory.CreateDirectory(outDir);
        }

        var utf8NoBom = new UTF8Encoding(false);
        await File.WriteAllTextAsync(outputCsPath, generatedCode, utf8NoBom);
        sw.Stop();

        var fi = new FileInfo(outputCsPath);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ Successfully generated {outputCsPath}");
        Console.WriteLine($"   Size: {fi.Length / 1024.0 / 1024.0:F2} MB ({generatedCode.Split('\n').Length:N0} lines) in {sw.ElapsedMilliseconds} ms");
        Console.ResetColor();
        Console.WriteLine("=================================================");

        return 0;
    }
}
