using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Documents;
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

    private const string EntryPointPreambule = ".globl __start\n__start:\n";
    
    public async ValueTask<CompilationResult> CompileStandaloneAsync(CompilationFile input)
    {   
        var project = projectService.GetCurrentProject();
        if (project is null)
        {
            return internalErrorCompilationResult;
        }

        var compilationDirectory = Path.Combine(project.ProjectDirectory, project.OutputPath);
        Directory.CreateDirectory(compilationDirectory);
        
        input.CalculateHash(project.ProjectDirectory);
        var compilationId = CalculateIdFromHashes([input.Hash]);
        var processedPath = Path.Combine(compilationDirectory, "standalone.asm");
        Directory.CreateDirectory(Path.GetDirectoryName(processedPath)!);
        await using var fsOut = File.Open(processedPath, FileMode.Create, FileAccess.Write);
        StreamWriter swout = new(fsOut, leaveOpen: true);
        await swout.WriteAsync(EntryPointPreambule);
        swout.Close();
        await using var fsIn = File.Open(Path.Combine(project.ProjectDirectory, input.FilePath), FileMode.Open, FileAccess.Read);
        await fsIn.CopyToAsync(fsOut);
        fsIn.Close();
        fsOut.Close();
        
        var exePath = Path.Combine(compilationDirectory, "standalone.exe");
        
        var command = GenerateCommand([processedPath], exePath);
        using MemoryStream diagMs = new();
        var commandError = await RunCommand(command, TimeSpan.FromMilliseconds(1000), diagMs);
        diagMs.Seek(0, SeekOrigin.Begin);

        if (commandError != CompilationError.None && commandError != CompilationError.CompilationError)
        {
            return LastCompilationResult = new CompilationResult()
            {
                IsSuccess = false,
                Id = compilationId,
                Error = commandError,
                Diagnostics = null,
                OutputStream = null,
                OutputElf = null
            };
        }

        var diagnostics = ParseDiagnostics(diagMs);
        
        if (commandError == CompilationError.CompilationError)
        {
            return LastCompilationResult = new CompilationResult()
            {
                IsSuccess = false,
                Id = compilationId,
                Error = CompilationError.CompilationError,
                Diagnostics = diagnostics,
                OutputElf = null,
                OutputStream = null
            };
        }
        
        var exeFs = File.Open(exePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var elf = ELFReader.Load<uint>(exeFs, false);

        if (elf is null) {
            // nao conseguiu ler corretamente o arquivo ELF
            exeFs.Close();
            return LastCompilationResult = new CompilationResult() {
                IsSuccess = false,
                Error = CompilationError.ElfError,
                OutputStream = null,
                OutputElf = null,
                Diagnostics = diagnostics,
                Id = compilationId
            };
        }
        
        return LastCompilationResult = new CompilationResult {
            IsSuccess = true,
            Error = CompilationError.None,
            OutputStream = exeFs,
            OutputElf = elf,
            Diagnostics = diagnostics,
            Id = compilationId
        };
    }

    public async ValueTask<CompilationResult> CompileAsync(CompilationInput input)
    {
        var project = projectService.GetCurrentProject();
        if (project is null)
        {
            return internalErrorCompilationResult;
        }

        if (input.Files.Count(x => x.IsEntryPoint) > 1)
        {
            throw new Exception("Only one entry point is allowed.");
        }
        
        var compilationDirectory = Path.Combine(project.ProjectDirectory, project.OutputPath);
        Directory.CreateDirectory(compilationDirectory);
        
        // calcular id
        var compilationId = CalculateIdFromCompilation(input, project.ProjectDirectory);

        // copia soh o entry point para o arquivo de saida
        var paths = input.Files.Select(x =>
        {
            var basePath = x.IsEntryPoint ? project.ProjectDirectory : compilationDirectory;
            return Path.Combine(basePath, x.FilePath);
        }).ToList();
        paths.ForEach(x => Directory.CreateDirectory(Path.GetDirectoryName(x)!));

        var entry = input.Files.Find(x => x.IsEntryPoint);
        
        var fsOut = File.Open(Path.Combine(compilationDirectory, entry.FilePath), FileMode.Create, FileAccess.Write);
        StreamWriter swout = new(fsOut, leaveOpen: true);
        await swout.WriteAsync(EntryPointPreambule);
        swout.Close();
        var fsIn = File.Open(Path.Combine(project.ProjectDirectory, entry.FilePath), FileMode.Open, FileAccess.Read);
        await fsIn.CopyToAsync(fsOut);
        fsIn.Close();
        fsOut.Close();
        
        var exePath = Path.Combine(compilationDirectory, project.OutputFile);
        
        var command = GenerateCommand(paths, exePath);
        
        using MemoryStream diagMs = new();
        var commandError = await RunCommand(command, TimeSpan.FromMilliseconds(1000), diagMs);
        diagMs.Seek(0, SeekOrigin.Begin);

        if (commandError != CompilationError.None && commandError != CompilationError.CompilationError)
        {
            return LastCompilationResult = new CompilationResult
            {
                IsSuccess = false,
                Error = commandError,
                OutputElf = null,
                Diagnostics = null,
                OutputStream = null,
                Id = Guid.Empty
            }; 
        }
        
        var diagnostics = ParseDiagnostics(diagMs);

        if (commandError == CompilationError.CompilationError)
        {
            return LastCompilationResult = new CompilationResult
            {
                IsSuccess = false,
                Error = commandError,
                OutputElf = null,
                OutputStream = null,
                Diagnostics = diagnostics,
                Id = compilationId
            };
        }
        
        var exeFs = File.Open(exePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var elf = ELFReader.Load<uint>(exeFs, false);
        exeFs.Seek(0, SeekOrigin.Begin);

        if (elf is null)
        {
            exeFs.Close();
            return LastCompilationResult = new CompilationResult
            {
                IsSuccess = false,
                Error = CompilationError.ElfError,
                OutputElf = null,
                OutputStream = null,
                Diagnostics = diagnostics,
                Id = compilationId
            };
        }
        
        return LastCompilationResult = new CompilationResult
        {
            IsSuccess = true,
            Id = compilationId,
            Error = CompilationError.None,
            Diagnostics = diagnostics,
            OutputElf = elf,
            OutputStream = exeFs
        };
    }

    public CompilationResult LastCompilationResult { get; private set; }

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
    
    private readonly CompilationResult internalErrorCompilationResult = new() {
        IsSuccess = false,
        Error = CompilationError.InternalError,
        OutputElf = null,
        OutputStream = null,
        Diagnostics = null,
        Id = Guid.Empty
    };
    
    private static Guid CalculateIdFromHashes(List<byte[]> hashes)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        using var ms = new MemoryStream();
        foreach (var hash in hashes)
        {
            ms.Write(hash, 0, hash.Length);
        }
        ms.Seek(0, SeekOrigin.Begin);
        var id = sha256.ComputeHash(ms).Take(16).ToArray();
        return new Guid(id);
    }

    private static Guid CalculateIdFromCompilation(CompilationInput input, string baseDirectory)
    {
        List<byte[]> hashes = [];
        foreach (var file in input.Files)
        {
            if (file.IsEntryPoint)
            {
                using MemoryStream ms = new();
                StreamWriter writer = new(ms);
                writer.Write(EntryPointPreambule);
                writer.Close();
                Stream fs = File.OpenRead(Path.Combine(baseDirectory, file.FilePath));
                fs.CopyTo(ms);
                fs.Close();
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                ms.Seek(0, SeekOrigin.Begin);
                var hash = sha256.ComputeHash(ms);
                ms.Close();
                hashes.Add(hash);
            }
            else
            {
                file.CalculateHash(baseDirectory);
                hashes.Add(file.Hash);
            }
        }
        return CalculateIdFromHashes(hashes);
    }

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
        var error = process.ExitCode == 0 ? CompilationError.None : CompilationError.CompilationError;
        process.Close();
        return error;
    }
}