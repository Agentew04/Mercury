using ELFSharp.ELF;
using Mercury.Engine.Common;
using Mercury.Engine.Common.Builders;
using Mercury.Engine.Mips.Runtime.Simple;
using Machine = Mercury.Engine.Common.Machine;

namespace Bench;

public class Demo {
    public static void Main2() {
        int opt = 1;
        // demo1 => 1
        // demo2 => 2

        switch (opt) {
            case 1:
                Demo1().GetAwaiter().GetResult();
                break;
            case 2:
                Demo2();
                break;
        }
    }

    private static async Task Demo1() {
        Machine machine = new MachineBuilder()
            .WithMemory(new MemoryBuilder()
                .With4Gb()
                .WithVolatileStorage()
                .Build())
            .WithMips()
            .WithMipsMonocycle()
            .WithMarsOs()
            .With<ConsoleStdOutModule>() // print std out to console
            .Build();

        (machine.CpuModule as Monocycle).UseBranchDelaySlot = false;

        Console.WriteLine($"ISA: {machine.Architecture}");
        Console.WriteLine($"CPU module name: {machine.CpuModule.GetType().FullName}");
        Console.WriteLine($"Syscall module name: {machine.CpuModule.GetType().FullName}");

        ELF<uint>? elf = ELFReader.Load<uint>(@"C:\Users\digoa\Desktop\teste\mips\demo.elf");
        machine.LoadElf(elf);
        elf.Dispose();

        Console.WriteLine("Comecando clocks");
        
        while (!machine.IsClockingFinished()) {
            await machine.ClockAsync();
        }

        Console.WriteLine("Fim dos clocks");
        
        
    }

    private static void Demo2() {
        
    }
}