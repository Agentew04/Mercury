using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ELFSharp.ELF;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;
using SAAE.Editor.Models.Compilation;

namespace SAAE.Editor.Services;

public partial class MipsCompiler : ICompilerService {

    private readonly SettingsService settingsService = App.Services.GetRequiredService<SettingsService>();
    private readonly ProjectService projectService = App.Services.GetRequiredService<ProjectService>();
    private string CompilerPath => Path.Combine(settingsService.Preferences.CompilerPath, "clang.exe");
    private string LinkerPath => Path.Combine(settingsService.Preferences.CompilerPath, "linker.ld");

    
    public async ValueTask<CompilationResult> CompileStandaloneAsync(Stream input) {
        ProjectFile? project = projectService.GetCurrentProject();
        if (project is null) {
            return new CompilationResult() {
                IsSuccess = false,
                Error = CompilationError.InternalError,
                OutputElf = null,
                OutputStream = null,
                Diagnostics = null,
                ErrorStream = null
            };
        }

        string compilationDirectory = Path.Combine(project.ProjectDirectory, project.OutputPath);
        Directory.CreateDirectory(compilationDirectory);
        
        string processedPath = Path.Combine(compilationDirectory, project.EntryFile);
        Directory.CreateDirectory(Path.GetDirectoryName(processedPath)!);
        await using FileStream fs = File.Open(processedPath, FileMode.Create, FileAccess.Write);
        StreamWriter swout = new(fs, leaveOpen: true);
        await swout.WriteAsync("\t.global __start\n__start:\n");
        swout.Close();
        await input.CopyToAsync(fs);
        fs.Close();
        
        string exePath = Path.Combine(compilationDirectory, project.OutputFile);
        
        ProcessStartInfo startInfo = new() {
            FileName = CompilerPath,
            Arguments = GenerateCommand([processedPath], exePath),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        CancellationTokenSource processCts = new(new TimeSpan(0, 0, 0, 10));
        
        Process? process;
        try {
            process = Process.Start(startInfo);
        }
        catch (Exception) {
            return new CompilationResult() {
                IsSuccess = false,
                Error = CompilationError.FileNotFound, // causa mais provavel
                OutputElf = null,
                OutputStream = null,
                Diagnostics = null,
                ErrorStream = null
            };
        }
        if (process is null) {
            // clang nao foi nem executado
            return new CompilationResult() {
                IsSuccess = false,
                Error = CompilationError.InternalError,
                OutputElf = null,
                OutputStream = null,
                ErrorStream = null,
                Diagnostics = null
            };
        }
        try {
            await process.WaitForExitAsync(processCts.Token);
        }
        catch (TaskCanceledException) {
            process.Kill();
            process.Close();
            return new CompilationResult() {
                IsSuccess = false,
                Error = CompilationError.TimeoutError,
                OutputElf = null,
                OutputStream = null,
                Diagnostics = null,
                ErrorStream = null
            };
        }

        MemoryStream diagMs = new();
        await process.StandardError.BaseStream.CopyToAsync(diagMs);
        diagMs.Seek(0, SeekOrigin.Begin);
        List<Diagnostic> diagnostics = ParseDiagnostics(diagMs);
        diagMs.Seek(0, SeekOrigin.Begin);
        
        if (process.ExitCode != 0) {
            // compiler error
            process.Close();
            return new CompilationResult {
                IsSuccess = false,
                Error = CompilationError.CompilationError,
                OutputElf = null,
                Diagnostics = diagnostics,
                ErrorStream = diagMs,
                OutputStream = null
            };
        }
        
        // deu tudo ok a princípio
        process.Close();
        
        FileStream exeFs = File.Open(exePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        // usar tryLoad?
        ELF<uint>? elf = ELFReader.Load<uint>(exeFs, false);

        if (elf is null) {
            // nao conseguiu ler corretamente o arquivo ELF
            exeFs.Close();
            return new CompilationResult() {
                IsSuccess = false,
                Error = CompilationError.ElfError,
                ErrorStream = diagMs,
                OutputStream = null,
                OutputElf = null,
                Diagnostics = diagnostics
            };
        }
        
        return new CompilationResult {
            IsSuccess = true,
            Error = CompilationError.None,
            ErrorStream = diagMs,
            OutputStream = exeFs,
            OutputElf = elf,
            Diagnostics = diagnostics,
        };
    }

    public ValueTask<CompilationResult> CompileAsync(CompilationInput input) {
        throw new NotImplementedException();
    }
    
    private string GenerateCommand(List<string> inputFiles, string outputName) {
        string files = string.Join(" ", inputFiles);
        return $"--target=mips-linux-gnu -O0 -fno-pic -mno-abicalls -nostartfiles -Wl -T \"{LinkerPath}\" -nostdlib" +
               $" -static -fuse-ld=lld -o \"{outputName}\" \"{files}\"";
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
    private List<Diagnostic> ParseDiagnostics(Stream stream) {
        List<Diagnostic> diagnostics = [];

        using StreamReader reader = new(stream, leaveOpen: true);

        // esperando um formato especifico:
        // <path>:<line>:<column> <error>: <message>
        // <line_content>
        // [unknown amount of spaces] ^

        Regex regex = ClangDiagnosticRegex();
        while (!reader.EndOfStream) {
            string? line = reader.ReadLine();
            if (line is null) {
                break;
            }

            Match match = regex.Match(line);
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
}