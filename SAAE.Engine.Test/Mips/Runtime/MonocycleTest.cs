using SAAE.Engine.Mips.Instructions;
using SAAE.Engine.Mips.Runtime.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAAE.Engine.Common.Builders;
using SAAE.Engine.Mips.Runtime;
using SAAE.Engine.Mips.Runtime.OS;

namespace SAAE.Engine.Test.Mips.Runtime;

[TestClass]
public class MonocycleTest {
    [TestMethod]
    public void TestBeq()
    {
        using Machine machine = new MachineBuilder()
            .WithMemory(new MemoryBuilder()
                .With4Gb()
                .WithVolatileStorage()
                .Build())
            .WithInMemoryStdio()
            .WithMips()
            .WithMipsMonocycle()
            .WithMarsOs()
            .Build();
        
        Monocycle cpu = machine.Cpu; 
        int[] code = [
            0x2008_000f,
            0x2009_0014,
            0x1109_0002,
            0x0000_000d,
            0x0810_0006,
            0x0000_004d
            ];
        machine.LoadProgram(code, Span<int>.Empty);
        InstructionFactory factory = new();
        bool hasBreaked = false;
        
        cpu.OnSignalException += (_, e) => {
            if(e.Signal != Monocycle.SignalExceptionEventArgs.SignalType.Breakpoint) {
                return;
            }

            Instruction inst = factory.Disassemble((uint)e.Instruction);
            if(inst is not Break brk) {
                return;
            }
            Console.WriteLine($"Reached BREAK {brk.Code}");
            Assert.AreEqual(0, brk.Code);
            hasBreaked = true;
        };
        cpu.UseBranchDelaySlot = false;
        while (!cpu.IsClockingFinished() && !hasBreaked) {
            cpu.Clock();
        }
    }

    [TestMethod]
    public void TestBuilder() {
        using Machine machine = new MachineBuilder()
            .WithMemory(new MemoryBuilder()
                .With4Gb()
                .WithVolatileStorage()
                .Build())
            .WithInMemoryStdio()
            .WithMips()
            .WithMipsMonocycle()
            .WithMarsOs()
            .Build();
        
        Assert.IsNotNull(machine.Memory);
        Assert.IsNotNull(machine.Cpu);
        Assert.IsNotNull(machine.Os);
        Assert.AreSame(machine.Cpu.RegisterFile, machine.Registers);
        Assert.AreSame(machine.Cpu.Memory, machine.Memory);
        Assert.AreSame(machine.Os.Machine, machine);
        
        const ulong gb = 1024 * 1024 * 1024;
        Assert.AreEqual(4 * gb, machine.Memory.Size);
        Assert.IsInstanceOfType<Monocycle>(machine.Cpu);
        Assert.IsInstanceOfType<Mars>(machine.Os);
    }

    [TestMethod]
    public void TestFibonacci() {
        using Machine machine = new MachineBuilder()
            .WithMemory(new MemoryBuilder()
                .With4Gb()
                .WithVolatileStorage()
                .Build())
            .WithInMemoryStdio()
            .WithMips()
            .WithMipsMonocycle()
            .WithMarsOs()
            .Build();
        
        int[] code = [
            0x2004_0000,
            0x0c10_0023,
            0x1440_001d,
            0x0000_0000,
            0x2004_0001,
            0x0c10_0023,
            0x2001_0001,
            0x1422_0018,
            0x0000_0000,
            0x2004_0002,
            0x0c10_0023,
            0x2001_0001,
            0x1422_0013,
            0x0000_0000,
            0x2004_0005,
            0x0c10_0023,
            0x2001_0005,
            0x1422_000e,
            0x0000_0000,
            0x2004_0007,
            0x0c10_0023,
            0x2001_000d,
            0x1422_0009,
            0x0000_0000,
            0x2004_000d,
            0x0c10_0023,
            0x2001_00e9,
            0x1422_0004,
            0x0000_0000,
            0x2002_0011,
            0x2004_0000,
            0x0000_000c,
            0x2002_0011,
            0x2004_0001,
            0x0000_000c,
            0x1c80_0003,
            0x0000_0000,
            0x2002_0000,
            0x0e30_0008,
            0x2001_0001,    
            0x1424_0003,
            0x0000_0000,
            0x2002_0001,
            0x03e0_0008,
            0x2008_0000,
            0x2009_0001,
            0x200b_0002,
            0x0000_000c,
            0x1420_0006,
            0x0000_0000,
            0x0109_5020,
            0x0009_4020,
            0x000a_4820,
            0x216b_0001,
            0x0810_002f,
            0x000a_1020,
            0x03e0_0008
        ];
        machine.LoadProgram(code, Span<int>.Empty);

        while (!machine.IsClockingFinished()) {
            machine.Clock();
        }
        
        Assert.AreEqual(0, machine.Cpu.ExitCode);    
    }
}
