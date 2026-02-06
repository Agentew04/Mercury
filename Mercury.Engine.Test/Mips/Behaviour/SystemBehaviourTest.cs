using Mercury.Engine.Common.Builders;
using Mercury.Engine.Mips.Instructions;
using Mercury.Engine.Mips.Runtime;

namespace Mercury.Engine.Test.Mips.Behaviour;

[TestClass]
public class SystemBehaviourTest {

    [TestMethod]
    public void TestBreak() {
        int[] code = [
            0x0000000d, // break
            0x0000004d, // break 1
            0x003fffcd, // break 65535
            0x00000a8d  // break 42
        ];

        List<int> breakValues = [];
        
        using MipsMachine machine = new MachineBuilder()
            .WithMemory(new MemoryBuilder()
                .With4Gb()
                .WithBigEndian()
                .WithBlockSize(128)
                .WithVolatileStorage()
                .WithBlockCapacity(4)
                .Build())
            .WithInMemoryStdio()
            .WithMips()
            .WithMipsMonocycle()
            .WithAnonymousOs(e => {
                if (e.Signal == SignalExceptionEventArgs.SignalType.Breakpoint) {
                    Break @break = new();
                    @break.FromInt((uint)e.Instruction);
                    breakValues.Add(@break.Code);
                }
                return Task.CompletedTask;
            })
            .Build();
        
        machine.LoadProgram(code.AsSpan(), Span<int>.Empty);
        while (!machine.IsClockingFinished()) {
            machine.ClockAsync().GetAwaiter().GetResult();
        }

        int[] expected = [0,1,65535,42];
        CollectionAssert.AreEqual(expected, breakValues);
    }
}