using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Mercury.Editor.Extensions;
using Mercury.Editor.Models;
using Mercury.Editor.Models.Compilation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mercury.Editor.Services;

public partial class MipsCompiler : BaseService<MipsCompiler>, ICompilerService {

    private readonly SettingsService settingsService = App.Services.GetRequiredService<SettingsService>();
    private readonly ProjectService projectService = App.Services.GetRequiredService<ProjectService>();
    private string AssemblerPath => Path.Combine(settingsService.Preferences.CompilerDirectory, UserPreferences.AssemblerFileName);
    
    private string LinkerPath => Path.Combine(settingsService.Preferences.CompilerDirectory, UserPreferences.LinkerFileName);
    private string LinkerScriptPath => Path.Combine(settingsService.Preferences.CompilerDirectory, "linker.ld");

    private const string EntryPointPreambule = ".globl __start\n__start:\n";
    
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
        foreach ((PathObject old, PathObject @new, int injectedLines) in moveTasks.Select(x => x.Result)) {
            string key = @new.ToString();
            pathMapping[key] = old;
            injectedLinesMapping[key] = injectedLines;
        }
        
        // 5. Compilar cada arquivo
        PathObject exePath = compilationDirectory + project.OutputFile;
        List<string> paths = pathMapping.Keys.ToList();
        using MemoryStream diagMs = new();
        List<ProcessResult> compilations = await CompileFiles(paths);
        List<Diagnostic> diagnostics = compilations.Select(x => x.Diagnostics)
            .Where(x => x is not null)
            .SelectMany(x => ParseDiagnostics(x!, pathMapping, injectedLinesMapping))
            .ToList();
        // se algum nao compilou corretamente, falha
        if (compilations.Any(x => !x.Success)) {
            Logger.LogDebug("One of the compilations was not sucessfull: file {File}", compilations.First(x => !x.Success).OutputFilePath);
            return LastCompilationResult = new CompilationResult() {
                Error = CompilationError.CompilationError,
                OutputPath = null,
                Id = compilationId,
                IsSuccess = false,
                Diagnostics = diagnostics
            };
        }
        
        // 6. Linkar todos arquivos
        ProcessResult link = await LinkFiles(compilations
            .Where(x => x.OutputFilePath is not null)
            .Select(x => x.OutputFilePath!)
            .ToList(),
            exePath.ToString());
        diagnostics.AddRange(link.Diagnostics is not null
            ? ParseDiagnostics(link.Diagnostics, pathMapping, injectedLinesMapping) : []);
        link.Diagnostics?.Dispose();
        // se nao conseguiu linkar, da erro
        if (!link.Success) {
            Logger.LogDebug("Link operation was not successful");
            return LastCompilationResult = new CompilationResult() {
                Error = CompilationError.LinkError,
                OutputPath = null,
                Id = compilationId,
                IsSuccess = false,
                Diagnostics = diagnostics
            };
        }
        
        // Libera recursos da compilacao
        compilations.ForEach(x => x.Diagnostics?.Dispose());
        
        // 7. Retorna a compilacao
        return LastCompilationResult = new CompilationResult() {
            IsSuccess = true,
            Diagnostics = diagnostics,
            Id = compilationId,
            Error = CompilationError.None,
            OutputPath = exePath.ToString()
        };
    }

    public CompilationResult LastCompilationResult { get; private set; }

    private async Task<List<ProcessResult>> CompileFiles(List<string> sourceFiles) {
        List<ProcessResult> results = [];
        string assemblerPath = AssemblerPath;
        await Parallel.ForEachAsync(sourceFiles, async (file, t) => {
            string outputName = Path.ChangeExtension(file, ".o");
            string commandArgs = $"--arch=mips --assemble --filetype=obj --no-exec-stack -o \"{outputName}\" \"{file}\"";

            MemoryStream ms = new();
            CompilationError result = await RunCommand(assemblerPath, commandArgs,
                TimeSpan.FromMilliseconds(300),
                ms, null);
            ms.Seek(0, SeekOrigin.Begin);
            if (result != CompilationError.None) {
                StreamReader sr = new(ms, leaveOpen: true);
                Logger.LogDebug("llvm-mc exit code non zero. Code: {Code} output for {file}: {output}", result, file, await sr.ReadToEndAsync(t));
                sr.Dispose();
                ms.Seek(0, SeekOrigin.Begin);
            }
            lock (results) {
                Logger.LogDebug("llvm-mc compilation sucess for file {File}", file);
                results.Add(new ProcessResult() {
                    Success = result == CompilationError.None,
                    OutputFilePath = result == CompilationError.None ? outputName : null,
                    Diagnostics = ms
                });
            }
        });
        return results;
    }

    private async Task<ProcessResult> LinkFiles(List<string> objectFiles, string elfFile) {
        string linker = LinkerPath;
        string args = $"-T \"{LinkerScriptPath}\" -static -O0 -oformat=elf --nostdlib --no-pie {string.Join(' ', objectFiles.Select(x => '"'+x+'"'))} -o \"{elfFile}\"";
        MemoryStream ms = new();
        CompilationError result = await RunCommand(linker, args, TimeSpan.FromMilliseconds(500), ms, settingsService.Preferences.CompilerDirectory);

        if (result != CompilationError.None)
        {
            ms.Seek(0, SeekOrigin.Begin);
            using StreamReader sr = new(ms, leaveOpen: true);
            Logger.LogDebug("Link operation failed. Message: {Message}", sr.ReadToEnd());
        }
        ms.Seek(0, SeekOrigin.Begin);
        return new ProcessResult() {
            Success = result == CompilationError.None,
            Diagnostics = ms,
            OutputFilePath = result == CompilationError.None ? elfFile : null
        };
    }

    private struct ProcessResult {
        public bool Success { get; init; }
        public Stream? Diagnostics { get; init; }
        public string? OutputFilePath { get; init; }
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

    private async Task<CompilationError> RunCommand(string path, string arguments, TimeSpan timeout, Stream output, string? workingDir)
    {
        ProcessStartInfo startInfo = new() {
            FileName = path,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (workingDir is not null) {
            startInfo.WorkingDirectory = workingDir;
        }
        
        using CancellationTokenSource processCts = new(timeout);
        
        Process? process;
        try {
            process = Process.Start(startInfo);
        }
        catch (Exception ex) {
            Logger.LogError(ex, "Process.Start raised exception. Command: {Cmd}. Error Message: {Msg}", path+" "+arguments, ex.Message);
            return CompilationError.ProgramError;
        }
        if (process is null) {
            // OS reutilizou outro processo. nao deveria acontecer nunca
            Logger.LogError("Started process is null. This should never happen when UseShellExecute = false. Its joever");
            return CompilationError.InternalError;
        }
        try {
            await process.WaitForExitAsync(processCts.Token);
        }
        catch (TaskCanceledException) {
            Logger.LogError("Timeout exceeded for started process. Aborting. Standard Error: {stderr}",
                await process.StandardError.ReadToEndAsync());
            process.Kill();
            process.Close();
            return CompilationError.TimeoutError;
        }

        //using CancellationTokenSource copyOutputCts = new(TimeSpan.FromMilliseconds(500));
        await process.StandardError.BaseStream.CopyToAsync(output);
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
        await sw.WriteLineAsync(".section metadata, \"\", @progbits # define secao de metadados que guarda onde no elf esse arquivo comeca");
        await sw.WriteLineAsync($".asciiz \"{input.Path.ToString().Replace("\\","/")}\"");
        await sw.WriteLineAsync(".quad __filestart");
        await sw.WriteLineAsync($".word {index}");
        await sw.WriteLineAsync(".text");
        injected+=8;
        if (input.IsEntryPoint) {
            await sw.WriteAsync(EntryPointPreambule);
            injected+=2;
        }
        await sw.FlushAsync();
        
        // preambulo injetado
        // agora prefixa todas linhas de codigo com a linha original
        string? line = await sr.ReadLineAsync();
        bool inTextSection = true;
        bool inMacro = false;
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

                if (processed.StartsWith(".macro")) {
                    inMacro = true;
                }else if (processed.StartsWith(".endm")) {
                    inMacro = false;
                }

                if (processed.StartsWith(".text")) {
                    inTextSection = true;
                }
            }

            if (inTextSection && !processed.StartsWith(':') && !string.IsNullOrWhiteSpace(processed) && processed.Trim() != ".text"
                && !inMacro && !processed.StartsWith('.')) {
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