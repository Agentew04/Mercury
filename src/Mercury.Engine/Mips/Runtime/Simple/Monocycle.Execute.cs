using Mercury.Engine.Common;
using Mercury.Engine.Mips.Instructions;
using Mercury.Engine.Mips.Runtime.Events;

namespace Mercury.Engine.Mips.Runtime.Simple;

public partial class Monocycle {
    private async ValueTask Execute(IInstruction instruction) {
        if (await ExecuteTypeR(instruction)) {
            return;
        }

        if (await ExecuteTypeI(instruction)) {
            return;
        }

        if (ExecuteTypeJ(instruction)) {
            return;
        }

        if (await ExecuteTypeF(instruction)) {
            return;
        }

        if (instruction is Nop) {
            return;
        }
        
        eventBus.Publish(new UntreatedInstructionEvent {
            Address = (ulong)Registers.Get(MipsGprRegisters.Pc),
            Word = BitConverter.ToUInt32(instructionBuffer.Span),
            Description = instruction.ToString()
        });
    }
    
    private Task InvokeSignal(SignalExceptionEventArgs e) {
        return SignalException is null ? Task.CompletedTask : SignalException.Invoke(e);
    }
}