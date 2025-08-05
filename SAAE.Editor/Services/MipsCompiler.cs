using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ELFSharp.ELF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Extensions;
using SAAE.Editor.Models;
using SAAE.Editor.Models.Compilation;

namespace SAAE.Editor.Services;

public partial class MipsCompiler : BaseService<MipsCompiler>, ICompilerService {

    private readonly SettingsService settingsService = App.Services.GetRequiredService<SettingsService>();
    private readonly ProjectService projectService = App.Services.GetRequiredService<ProjectService>();
    private string CompilerPath => Path.Combine(settingsService.Preferences.CompilerPath, "clang.exe");
    private string LinkerPath => Path.Combine(settingsService.Preferences.CompilerPath, "linker.ld");

    public const string EntryPointPreambule = ".globl __start\n__start:\n";
    
    public async ValueTask<CompilationResult> CompileAsync(CompilationInput input)
    {
        /*
         * 1. Calcular onde eh o diretorio de compilacao
         * 2. Calcular id da compilacao
         * 3. Mover entry point modificado para o diretorio de compilacao
         * 4. Salvar relacao paths antigos e novos
         * 5. Compilar 
         */
        ProjectFile project = projectService.GetCurrentProject()!;
        if (input.Files.Count(x => x.IsEntryPoint) > 1)
        {
            throw new Exception("Only one entry point is allowed.");
        }

        // 1. Calcular onde eh o diretorio de compilacao
        PathObject compilationDirectory = project.ProjectDirectory + project.OutputPath;
        Directory.CreateDirectory(compilationDirectory.ToString());
        
        // 2. Calcular id da compilacao
        Guid compilationId = input.CalculateId();

        StandardLibrary stdlib = settingsService.StdLibSettings.GetCompatibleLibrary(project)!;

        // 3. Mover todos os arquivos modificados para o diretorio de compilacao
        List<Task<(PathObject Old, PathObject New, int injectedLines)>> moveTasks = input.Files.Select((x,i) =>
            MoveToBinAsync(
                index: i,
                input: x,
                srcDirectory: project.ProjectDirectory + project.SourceDirectory,
                binDirectory: compilationDirectory,
                stdlibDirectory: stdlib.Path))
            .ToList();
        await Task.WhenAll(moveTasks);
        
        // 4. construir dicionarios
        Dictionary<string, PathObject> pathMapping = [];
        Dictionary<string, int> injectedLinesMapping = [];
        foreach ((PathObject Old, PathObject New, int injectedLines) in moveTasks.Select(x => x.Result)) {
            string key = New.ToString();
            pathMapping[key] = Old;
            injectedLinesMapping[key] = injectedLines;
        }
        
        // 5. Compilar
        PathObject exePath = compilationDirectory + project.OutputFile;
        List<string> paths = pathMapping.Keys.ToList();
        string command = GenerateCommand(paths, exePath.ToString());
        using MemoryStream diagMs = new();
        CompilationError commandError = await RunCommand(command, TimeSpan.FromMilliseconds(2000), diagMs);
        diagMs.Seek(0, SeekOrigin.Begin);

        if (commandError != CompilationError.None) {
            using StreamReader sr = new(diagMs, leaveOpen: true);
            Logger.LogWarning("Error compiling. Type: {type}. Out: {out}", commandError, await sr.ReadToEndAsync());
        }

        if (commandError != CompilationError.None && commandError != CompilationError.CompilationError)
        {
            return LastCompilationResult = new CompilationResult
            {
                IsSuccess = false,
                Error = commandError,
                Diagnostics = null,
                Id = Guid.Empty,
                OutputPath = null
            }; 
        }
        
        List<Diagnostic> diagnostics = ParseDiagnostics(diagMs, pathMapping, injectedLinesMapping);

        if (commandError == CompilationError.CompilationError)
        {
            return LastCompilationResult = new CompilationResult
            {
                IsSuccess = false,
                Error = commandError,
                Diagnostics = diagnostics,
                Id = compilationId,
                OutputPath = null
            };
        }
        
        return LastCompilationResult = new CompilationResult
        {
            IsSuccess = true,
            Id = compilationId,
            Error = CompilationError.None,
            Diagnostics = diagnostics,
            OutputPath = exePath.ToString()
        };
    }

    public CompilationResult LastCompilationResult { get; private set; }

    private string GenerateCommand(List<string> inputFiles, string outputName) {
        StringBuilder sb = new();
        foreach (string file in inputFiles) {
            sb.Append('"');
            sb.Append(file);
            sb.Append('"');
            sb.Append(' ');
        }
        string? files = string.Join(" ", inputFiles);
        return $"--target=mips-linux-gnu -O0 -fno-pic -mno-abicalls -nostartfiles -Wl -T \"{LinkerPath}\" -nostdlib" +
               $" -static -fuse-ld=lld -o \"{outputName}\" {files}";
        /*
         * -O0: no optimization
         * -fno-pic: desativa position independent code. Coloquei pra garantir que 'la' traduza para 'lui'+'ori'
         *  e nao usar o $gp
         * -mno-abicalls: codigo asm puro, sem nada do linker
         * -nostartfiles: nao incluir arquivos de inicializacao
         * -Wl -T: passa o script do linker (obriga o .text e .data ser o endereco q a gente quer)
         * -nostdlib: nao linka a standard library. sem printf, scanf, etc
         * -static: linka estaticamente(um exe só, sem dlls)
         * -fuse-ld=lld: usa o lld como linker(qq muda? n sei. mas ld normal n funciona pra mips, sla)
         */
    }

    // talvez esse codigo repita para todos os compilers que usem clang!
    private static List<Diagnostic> ParseDiagnostics(Stream stream, Dictionary<string, PathObject> pathMapping, Dictionary<string, int> injectedLinesMapping) {
        List<Diagnostic> diagnostics = [];

        using StreamReader reader = new(stream, leaveOpen: true);

        // esperando um formato especifico:
        // <path>:<line>:<column> <error>: <message>
        // <line_content>
        // [unknown amount of spaces] ^

        Regex regex = ClangDiagnosticRegex();
        while (!reader.EndOfStream) {
            string? line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) {
                break;
            }

            Match match = regex.Match(line);
            if (!match.Success) {
                continue;
            }
            string path = match.Groups["path"].Value;
            PathObject originalPath = pathMapping[path];
            DiagnosticType type = match.Groups["type"].Value switch {
                "error" => DiagnosticType.Error,
                "warning" => DiagnosticType.Warning,
                _ => DiagnosticType.Unknown
            };
            string message = match.Groups["message"].Value;
            int lineNumber = int.Parse(match.Groups["line"].Value) - injectedLinesMapping[path];
            int columnNumber = int.Parse(match.Groups["column"].Value);

            Diagnostic d = new() {
                FilePath = originalPath,
                Line = lineNumber,
                Column = columnNumber,
                Type = type,
                Message = message,
            };
            diagnostics.Add(d);

            // le linha com conteudo original
            _ = reader.ReadLine();
            // le linha com ^ apontando caractere
            _ = reader.ReadLine();
        }
        
        return diagnostics;
    }

    [GeneratedRegex(@"^(?<path>.+):(?<line>\d+):(?<column>\d+): (?<type>error|warning): (?<message>.+)$")]
    private static partial Regex ClangDiagnosticRegex();
    
    private readonly CompilationResult internalErrorCompilationResult = new() {
        IsSuccess = false,
        Error = CompilationError.InternalError,
        Diagnostics = null,
        Id = Guid.Empty,
        OutputPath = null
    };

    private async ValueTask<CompilationError> RunCommand(string arguments, TimeSpan timeout, Stream output)
    {
        ProcessStartInfo startInfo = new() {
            FileName = CompilerPath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        using CancellationTokenSource processCts = new(timeout);
        
        Process? process;
        try {
            process = Process.Start(startInfo);
        }
        catch (Exception) {
            return CompilationError.FileNotFound; // causa mais provavel, nao achou o compilador
        }
        if (process is null) {
            // clang nao foi nem executado
            return CompilationError.InternalError; // nem ideia oq causaria isso
        }
        try {
            await process.WaitForExitAsync(processCts.Token);
        }
        catch (TaskCanceledException) {
            // possivelmente permission denied. arquivo .elf esta
            // sendo usado por outro processo!
            Logger.LogError("StdOut: {out}; StdErr: {err}",
                await process.StandardOutput.ReadToEndAsync(),
                await process.StandardError.ReadToEndAsync());
            process.Kill();
            process.Close();
            return CompilationError.TimeoutError;
        }

        using CancellationTokenSource copyOutputCts = new(TimeSpan.FromMilliseconds(500));
        // aqui o compilador rodou
        // agora le o output
        await process.StandardError.BaseStream.CopyToAsync(output, copyOutputCts.Token);
        CompilationError error = process.ExitCode == 0 ? CompilationError.None : CompilationError.CompilationError;
        process.Close();
        return error;
    }

    private static async Task<(PathObject Old, PathObject New, int injectedLines)> MoveToBinAsync(int index, CompilationFile input, PathObject srcDirectory, PathObject binDirectory, PathObject stdlibDirectory) {
        PathObject outputFilepath;
        try {
            outputFilepath = binDirectory.Folder("src") + (input.Path - srcDirectory);
        }
        catch (NotSupportedException) {
            outputFilepath = binDirectory.Folder("stdlib") + (input.Path - stdlibDirectory);
        }
        Directory.CreateDirectory(outputFilepath.Path().ToString());

        await using FileStream fsIn = File.OpenRead(input.Path.ToString());
        using StreamReader sr = new(fsIn);
        await using FileStream fsOut = File.Open(outputFilepath.ToString(), FileMode.Create, FileAccess.Write);
        int injected = 0;
        await using StreamWriter sw = new(fsOut);
        await sw.WriteLineAsync(".text");
        await sw.WriteLineAsync(".hidden __filestart");
        await sw.WriteLineAsync("__filestart: # comeca com __, vai ser ignorado pelo aplicativo.");
        await sw.WriteLineAsync("# se vc eh um usuario lendo isso, nao use __ nas suas labels :)");
        await sw.WriteLineAsync(".section metadata, \"\", @progbits # define secao de metadados que guarda onde no elf esse arquivo comeca");
        await sw.WriteLineAsync($".asciiz \"{input.Path.ToString().Replace("\\","/")}\"");
        await sw.WriteLineAsync(".quad __filestart");
        await sw.WriteLineAsync($".word {index}");
        await sw.WriteLineAsync(".text");
        injected+=9;
        if (input.IsEntryPoint) {
            await sw.WriteAsync(EntryPointPreambule);
            injected+=2;
        }
        await sw.FlushAsync();
        
        // preambulo injetado
        // agora prefixa todas linhas de codigo com a linha original
        string? line = await sr.ReadLineAsync();
        bool inTextSection = true;
        for (int lineIndex = 1; line is not null; lineIndex++) {
            // remove possivel comentario
            string processed = line;
            int commentIndex = line.IndexOf('#');
            if(commentIndex != -1) {
                // remove comentario
                processed = line[..commentIndex];
            }
            int lastColonIndex = line.LastIndexOf(':');
            if (lastColonIndex != -1) {
                // remove labels
                processed = line[lastColonIndex..];
            }

            if (processed.StartsWith('.')) {
                // eh uma diretiva
                if (processed.StartsWith(".rodata")
                    || processed.StartsWith(".data")
                    || processed.StartsWith(".bss")
                    || processed.StartsWith(".org")
                    || processed.StartsWith(".section")
                    || processed.StartsWith(".pushsection")
                    || processed.StartsWith(".popsection")) {
                    inTextSection = false;
                }

                if (processed.StartsWith(".text")) {
                    inTextSection = true;
                }
            }

            if (inTextSection && !processed.StartsWith(':') && !string.IsNullOrWhiteSpace(processed) && processed.Trim() != ".text") {
                await sw.WriteAsync($"L.{index}.{lineIndex}: ");
            }
            await sw.WriteLineAsync(line);
            line = await sr.ReadLineAsync();
        }
        
        if (input.IsEntryPoint) {
            await sw.WriteLineAsync("j __end # prevenir execucao de padding. simulador le __end do elf e seta como endereco de dropoff");
            // nao incrementa injected pois esta no final
        }
        return (input.Path, outputFilepath, injected);
    }
}