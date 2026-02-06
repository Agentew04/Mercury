using Mercury.Engine.Common;
using Mercury.Engine.Mips.Instructions;

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

        await MipsMachine.StdErr.Writer.WriteAsync(
            $"Instruction {instruction.ToString()} recognized but not executed @ PC=0x{Registers.Get(MipsGprRegisters.Pc):X8}");
    }
    
    private Task InvokeSignal(SignalExceptionEventArgs e) {
        return SignalException is null ? Task.CompletedTask : SignalException.Invoke(e);
    }
}