using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ELFSharp.ELF;
using Microsoft.Extensions.DependencyInjection;

namespace SAAE.Editor.Services;

public class MipsCompiler : CompilerService {

    private readonly SettingsService settings = App.Services.GetService<SettingsService>()!;
    
    public async Task<(bool success, IELF? elf)> TryCompileAssemblyAsync(string assemblyCode) {
        // create temporary file
        string assemblyPath = Path.GetTempFileName();
        File.Move(assemblyPath, Path.ChangeExtension(assemblyPath, ".s"));
        assemblyPath = Path.ChangeExtension(assemblyPath, ".s");
        string exePath = Path.ChangeExtension(assemblyPath, ".exe");
        
        await File.WriteAllTextAsync(assemblyPath, assemblyCode);
        
        // compile
        string compilerPath = Path.Combine(settings.Preferences.CompilerPath, "clang.exe");
        string scriptPath = Path.Combine(settings.Preferences.CompilerPath, "linker.ld");
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
        string compilerPath = Path.Combine(settings.Preferences.CompilerPath, "clang.exe");
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

    protected override Task Compile()
    {
        throw new NotImplementedException();
    }
}