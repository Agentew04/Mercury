using SAAE.Engine.Common;
using SAAE.Engine.Mips.Instructions;
using SAAE.Engine.Mips.Runtime.Simple;
using SAAE.Engine.Common.Builders;
using SAAE.Engine.Mips.Runtime;
using SAAE.Engine.Mips.Runtime.OS;

namespace SAAE.Engine.Test.Mips.Runtime;

[TestClass]
public class MonocycleTest {
    [TestMethod]
    public void TestBeq()
    {
        using MipsMachine mipsMachine = new MachineBuilder()
            .WithMemory(new MemoryBuilder()
                .With4Gb()
                .WithVolatileStorage()
                .Build())
            .WithInMemoryStdio()
            .WithMips()
            .WithMipsMonocycle()
            .WithMarsOs()
            .Build();
        
        Monocycle cpu = (Monocycle)mipsMachine.Cpu; 
        int[] code = [
            0x2008_000f,
            0x2009_0014,
            0x1109_0002,
            0x0000_000d,
            0x0810_0006,
            0x0000_004d
            ];
        mipsMachine.LoadProgram(code, Span<int>.Empty);
        bool hasBreaked = false;
        
        cpu.SignalException += (e) => {
            if(e.Signal != SignalExceptionEventArgs.SignalType.Breakpoint) {
                return Task.CompletedTask;
            }

            Instruction? inst = Disassembler.Disassemble((uint)e.Instruction);
            if(inst is not Break brk) {
                return Task.CompletedTask;
            }
            Console.WriteLine($"Reached BREAK {brk.Code}");
            Assert.AreEqual(0, brk.Code);
            hasBreaked = true;
            return Task.CompletedTask;
        };
        cpu.UseBranchDelaySlot = false;
        while (!cpu.IsClockingFinished() && !hasBreaked) {
            cpu.ClockAsync().AsTask().GetAwaiter().GetResult();
        }
    }

    [TestMethod]
    public void TestBuilder() {
        using MipsMachine mipsMachine = new MachineBuilder()
            .WithMemory(new MemoryBuilder()
                .With4Gb()
                .WithVolatileStorage()
                .Build())
            .WithInMemoryStdio()
            .WithMips()
            .WithMipsMonocycle()
            .WithMarsOs()
            .Build();
        
        Assert.IsNotNull(mipsMachine.DataMemory);
        Assert.IsNotNull(mipsMachine.InstructionMemory);
        Assert.IsNotNull(mipsMachine.Cpu);
        Assert.IsNotNull(mipsMachine.Os);
        Assert.IsNotNull(mipsMachine.Cpu.Machine);
        Assert.AreSame(mipsMachine.Cpu.RegisterBank, mipsMachine.Registers);
        Assert.IsNotNull(mipsMachine.Os.Machine);
        if(!mipsMachine.Os.Machine.TryGetTarget(out Machine? target)) {
            Assert.Fail("OS Machine weak reference is not set.");
        }
        Assert.AreSame(target, mipsMachine);
        
        const ulong gb = 1024 * 1024 * 1024;
        Assert.AreEqual(4 * gb, (mipsMachine.DataMemory as Engine.Memory.Memory)!.Size);
        Assert.IsInstanceOfType<Monocycle>(mipsMachine.Cpu);
        Assert.IsInstanceOfType<Mars>(mipsMachine.Os);
    }

    [TestMethod]
    public void TestFibonacci() {
        using MipsMachine mipsMachine = new MachineBuilder()
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
        mipsMachine.LoadProgram(code, Span<int>.Empty);

        while (!mipsMachine.IsClockingFinished()) {
            mipsMachine.ClockAsync().AsTask().GetAwaiter().GetResult();
        }
        
        Assert.AreEqual(0, mipsMachine.Cpu.ExitCode);    
    }
}
