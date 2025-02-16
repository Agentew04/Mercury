using SAAE.Engine.Mips.Instructions;
using SAAE.Engine.Mips.Runtime.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAAE.Engine.Mips.Runtime;

namespace SAAE.Engine.Test.Mips.Runtime;

[TestClass]
public class MonocycleTest {
    [TestMethod]
    public void TestBeq() {
        using Machine machine = new MachineBuilder()
            .WithMarsOs()
            .With4GbRam()
            .WithMipsMonocycle()
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

            Instruction inst = factory.Disassemble(e.Instruction);
            if(inst is not Break brk) {
                return;
            }
            Console.WriteLine($"Reached BREAK {brk.Code}");
            Assert.AreEqual(0, brk.Code);
            hasBreaked = true;
        };
        cpu.UseBranchDelaySlot = false;
        while (!cpu.IsExecutionFinished() && !hasBreaked) {
            cpu.Clock();
        }
    }

    [TestMethod]
    public void TestBuilder() {
        Machine machine = new MachineBuilder()
            .With4GbRam()
            .WithMarsOs()
            .WithMipsMonocycle()
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
}
