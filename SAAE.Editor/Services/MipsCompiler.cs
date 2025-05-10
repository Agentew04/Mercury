using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ELFSharp.ELF;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;

namespace SAAE.Editor.Services;

public class MipsCompiler : ICompilerService {

    private readonly SettingsService settingsService = App.Services.GetRequiredService<SettingsService>();
    private readonly ProjectService projectService = App.Services.GetRequiredService<ProjectService>();
    private string CompilerPath => Path.Combine(settingsService.Preferences.CompilerPath, "clang.exe");
    private string LinkerPath => Path.Combine(settingsService.Preferences.CompilerPath, "linker.ld");

    
    
    public async Task<(bool success, IELF? elf)> TryCompileAssemblyAsync(string assemblyCode) {
        // create temporary file
        var files = CreateTemporaryFiles(".s", ".exe");
        await File.WriteAllTextAsync(assemblyPath, assemblyCode);
        
        // compile
        string compilerPath = Path.Combine(settingsService.Preferences.CompilerPath, "clang.exe");
        string scriptPath = Path.Combine(settingsService.Preferences.CompilerPath, "linker.ld");
        ProcessStartInfo startInfo = new() {
            FileName = compilerPath,
            Arguments =
                $"--target=mips-linux-gnu -O0 -fno-pic -mno-abicalls -nostartfiles -Wl -T \"{scriptPath}\" -nostdlib" +
                $" -static -fuse-ld=lld -o \"{exePath}\" \"{assemblyPath}\"",
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
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        CancellationTokenSource cts = new();
        using Process? process = Process.Start(startInfo);
        if (process is null) {
            return (false, null);
        }

        TimeSpan timeout = new(0, 0, 0, 10);
        cts.CancelAfter(timeout);
        try {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (TaskCanceledException) {
            process.Kill();
            return (false, null);
        }

        if (process.ExitCode != 0) {
            return (false, null);
        }

        // uint sinaliza que eh um binario 32 bits
        ELF<uint>? elf = ELFReader.Load<uint>(exePath);
        return elf is null ? (false, null) : (true, elf);
    }

    public async Task<bool> TryCompileCodeAsync(string highlevelCode) {
        string assemblyPath = Path.GetTempFileName();
        File.Move(assemblyPath, Path.ChangeExtension(assemblyPath, ".s"));
        assemblyPath = Path.ChangeExtension(assemblyPath, ".c");
        string exePath = Path.ChangeExtension(assemblyPath, ".exe");
        
        await File.WriteAllTextAsync(assemblyPath, highlevelCode);
        
        // compile
        string compilerPath = Path.Combine(settingsService.Preferences.CompilerPath, "clang.exe");
        ProcessStartInfo startInfo = new() {
            FileName = compilerPath,
            Arguments =
                $"--target=mips-linux-gnu -O0 -nostdlib -static -fuse-ld=lld -o \"{exePath}\" \"{assemblyPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        CancellationTokenSource cts = new();
        using Process? process = Process.Start(startInfo);
        if (process is null) {
            return false;
        }

        TimeSpan timeout = new(0, 0, 0, 10);
        cts.CancelAfter(timeout);
        try {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (TaskCanceledException) {
            process.Kill();
            return false;
        }

        if (process.ExitCode != 0) {
            return false;
        }

        // uint sinaliza que eh um binario 32 bits
        ELF<uint>? elf = ELFReader.Load<uint>(exePath);
        if (elf is null) {
            return false;
        }

        return true;
    }

    public async ValueTask<CompilationResult> CompileStandaloneAsync(Stream input) {
        ProjectFile? project = projectService.GetCurrentProject();
        if (project is null) {
            return new CompilationResult() {
                IsSuccess = false,
                Error = CompilationError.InternalError,
                OutputElf = null,
                OutputStream = null,
            };
        }

        string processedPath = Path.Combine(project.OutputPath, project.EntryFile);
        await using FileStream fs = File.Open(processedPath, FileMode.Create, FileAccess.Write);
        StreamWriter swout = new(fs, leaveOpen: true);
        await swout.WriteAsync("\t.global __start\n__start:\n");
        swout.Close();
        await input.CopyToAsync(fs);
        fs.Close();
        
        ProcessStartInfo startInfo = new() {
            FileName = CompilerPath,
            Arguments = GenerateCommand([processedPath], project.OutputPath),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        CancellationTokenSource cts = new(new TimeSpan(0, 0, 0, 10));
        
        Process? process;
        try {
            process = Process.Start(startInfo);
        }
        catch (Exception) {
            return new CompilationResult() {
                IsSuccess = false,
                Error = CompilationError.FileNotFound, // causa mais provavel
                OutputElf = null,
                OutputStream = null
            };
        }
        if (process is null) {
            // clang nao foi nem executado
            return new CompilationResult() {
                IsSuccess = false,
                Error = CompilationError.InternalError,
                OutputElf = null,
                OutputStream = null
            };
        }
        try {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (TaskCanceledException) {
            process.Kill();
            return (false, null);
        }

        if (process.ExitCode != 0) {
            return (false, null);
        }
        process.Close();
    }

    public ValueTask<CompilationResult> CompileAsync(CompilationInput input) {
        throw new NotImplementedException();
    }

    private static Dictionary<string,string> CreateTemporaryFiles(params string[] extensions) {
        // extensions includes the '.'
        string basePath = Path.GetTempFileName();
        Dictionary<string, string> files = new();
        foreach (string extension in extensions) {
            File.Create(Path.ChangeExtension(basePath, extension)).Close();
            files.Add(extension, Path.ChangeExtension(basePath, extension));
        }
        return files;
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
}