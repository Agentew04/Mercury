using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;
using SAAE.Editor.Models.Compilation;

namespace SAAE.Editor.Services;

public partial class MipsCompiler : ICompilerService {

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
         * 4. Compilar 
         */
        ProjectFile project = projectService.GetCurrentProject()!;
        
        if (input.Files.Count(x => x.IsEntryPoint) > 1)
        {
            throw new Exception("Only one entry point is allowed.");
        }
        
        // 1. Calcular onde eh o diretorio de compilacao
        string compilationDirectory = Path.Combine(project.ProjectDirectory, project.OutputPath);
        Directory.CreateDirectory(compilationDirectory);
        
        // 2. Calcular id da compilacao
        Guid compilationId = input.CalculateId(EntryPointPreambule);

        // 3. Mover entry point modificado para o diretorio de compilacao
        CompilationFile entryPoint = input.Files.First(x => x.IsEntryPoint);
        string entryPointPath = Path.Combine(compilationDirectory, Path.GetFileName(entryPoint.FullPath));
        FileStream fsOut = File.Open(entryPointPath, FileMode.Create, FileAccess.Write);
        // adicionar preambulo do entry point
        StreamWriter swout = new(fsOut, leaveOpen: true);
        await swout.WriteAsync(EntryPointPreambule);
        swout.Close();
        // completa com arquivo original
        FileStream fsIn = File.Open(entryPoint.FullPath, FileMode.Open, FileAccess.Read);
        await fsIn.CopyToAsync(fsOut);
        fsIn.Close();
        fsOut.Close();
        
        
        // 4. Compilar
        string exePath = Path.Combine(compilationDirectory, project.OutputFile);
        List<string> paths = input.Files.Where(x => !x.IsEntryPoint)
            .Select(x => x.FullPath).ToList();
        paths.Add(entryPointPath);
        string command = GenerateCommand(paths, exePath);
        
        using MemoryStream diagMs = new();
        CompilationError commandError = await RunCommand(command, TimeSpan.FromMilliseconds(1000), diagMs);
        diagMs.Seek(0, SeekOrigin.Begin);

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
        
        List<Diagnostic> diagnostics = ParseDiagnostics(diagMs);

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
            OutputPath = exePath
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
    private static List<Diagnostic> ParseDiagnostics(Stream stream) {
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
            DiagnosticType type = match.Groups["type"].Value switch {
                "error" => DiagnosticType.Error,
                "warning" => DiagnosticType.Warning,
                _ => DiagnosticType.Unknown
            };
            string message = match.Groups["message"].Value;
            int lineNumber = int.Parse(match.Groups["line"].Value);
            int columnNumber = int.Parse(match.Groups["column"].Value);

            Diagnostic d = new() {
                FilePath = path,
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
            process.Kill();
            process.Close();
            return CompilationError.TimeoutError;
        }

        using CancellationTokenSource copyOutputCts = new(TimeSpan.FromMilliseconds(200));
        // aqui o compilador rodou
        // agora le o output
        await process.StandardError.BaseStream.CopyToAsync(output, copyOutputCts.Token);
        CompilationError error = process.ExitCode == 0 ? CompilationError.None : CompilationError.CompilationError;
        process.Close();
        return error;
    }
}