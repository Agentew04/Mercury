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

        if (instruction is Nop) {
            return;
        }

        // CPU does not know this instruction: let other modules (e.g. FPU) try
        UnhandledInstructionEvent unhandled = new() {
            Instruction = instruction,
            Word = (uint)BytesToInt32(instructionBuffer.Span),
            Address = (ulong)Registers.Get(MipsGprRegisters.Pc)
        };
        eventBus.Publish(unhandled);
        if (unhandled.Handled) {
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