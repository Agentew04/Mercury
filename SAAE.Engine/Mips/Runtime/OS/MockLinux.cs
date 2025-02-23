using SAAE.Engine.Mips.Runtime.Simple;

namespace SAAE.Engine.Mips.Runtime;

/// <summary>
/// Class that implements basic callbacks to a mock
/// operating system. Basically responsible for executing
/// correctly syscalls.
/// </summary>
/// <remarks>Uses the o32 linux syscall codes. Available
/// at: https://github.com/torvalds/linux/blob/master/arch/mips/kernel/syscalls/syscall_o32.tbl.
/// Convencao do mips32: https://gist.github.com/yamnikov-oleg/454f48c3c45b735631f2
/// 
///  </remarks>
public class MockLinux : OperatingSystem {

    public override string OperatingSystemName => "linux";

    protected override void OnSyscall(uint code) {
        switch (code) {
            case 0:
                Exit();
                break;
            case 2:
                Fork();
                break;
            case 3:
                Read();
                break;
            case 4:
                Write();
                break;
            case 5:
                Open();
                break;
            case 6:
                Close();
                break;
        }
    }

    public override void Dispose() {
        throw new NotImplementedException();
    }

    private void Exit() {
        throw new InvalidOperationException();
    }

    private void Fork() {
        throw new InvalidOperationException();
    }

    private void Read() {
        throw new InvalidOperationException();
    }

    private void Write() {
        throw new InvalidOperationException();
    }

    private void Open() {
        throw new InvalidOperationException();
    }

    private void Close() {
        throw new InvalidOperationException();
    }
}