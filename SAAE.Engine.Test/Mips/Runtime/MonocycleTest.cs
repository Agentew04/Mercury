using SAAE.Engine.Mips.Instructions;
using SAAE.Engine.Mips.Runtime.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Test.Mips.Runtime;

[TestClass]
public class MonocycleTest {
    [TestMethod]
    public void TestBeq() {
        Monocycle cpu = new();
        int[] code = [
            0x2008_000f,
            0x2009_0014,
            0x1109_0002,
            0x0000_000d,
            0x0810_0006,
            0x0000_004d
            ];
        cpu.LoadTextSection(code);
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
        cpu.RegisterFile[Engine.Mips.Runtime.RegisterFile.Register.Pc] = 0x0040_0000;
        while (cpu.IsExecutionFinished() && !hasBreaked) {
            cpu.Clock();
        }
        cpu.Dispose();
    }
}
