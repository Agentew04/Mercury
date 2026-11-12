using Mercury.Engine.Common;
using Mercury.Engine.Common.Events;
using Mercury.Engine.Mips.Runtime.Events;

namespace Mercury.Engine.Mips.Runtime.OS;

/// <summary>
/// Common interface all operating systems must implement.
/// Specific version for MIPS archtecture.
/// </summary>
public abstract class MipsSyscallModule : ISyscallModule {
    
    private List<IDisposable> subscriptions = [];
    protected EventBus eventBus = null!;
    protected EventStream stdin = null!;
    protected EventStream stdout = null!;
    protected EventStream stderr = null!;
    
    public void SubscribeToEvents(EventBus bus) {
        this.eventBus = bus;
        subscriptions.Add(bus.Subscribe<OnSyscallEvent>(async e => await OnSyscallReceive(e)));

        stdin = new EventStream(async (_) => {
            Memory<char> buffer = new char[1].AsMemory();
            int read = 0;
            await bus.PublishAsync(new StdInReadEvent() {
                Buffer = buffer,
                Delimiter = '\n',
                OnReadComplete = r => read = r
            });
            return buffer.Span[0];
        }, null);
        stdout = new EventStream(null, (buffer,_) => {
            Memory<char> buffer2 = new char[buffer.Length];
            buffer.CopyTo(buffer2.Span);
            return bus.PublishAsync(new StdOutWriteEvent {
                Data = buffer2
            });
        });
        stderr = new EventStream(null, (buffer,_) => {
            Memory<char> buffer2 = new char[buffer.Length];
            buffer.CopyTo(buffer2.Span);
            return bus.PublishAsync(new StdErrWriteEvent {
                Data = buffer2
            });
        });
    }

    public void UnsubscribeFromEvents() {
        foreach(IDisposable disposable in subscriptions) {
            disposable.Dispose();
        }
        subscriptions.Clear();
    }

    public abstract void Serialize(Stream s);
    public abstract void Deserialize(Stream s);

    public virtual void Dispose() {
        UnsubscribeFromEvents();
    }

    public Architecture CompatibleArchitecture => Architecture.Mips;
    
    public abstract string FriendlyName { get; }
    
    public abstract string Identifier { get; }

    protected OnSyscallEvent Context;

    public async Task OnSyscallReceive(OnSyscallEvent e) {
        Context = e;
        uint mask = 0xF_FFFF << 6;
        // this signal is embedded in syscall (normally not used)
        // used when: 'syscall 5'
        uint instructionSignal = e.Instruction & mask;
        if (instructionSignal != 0) {
            await ExecuteSyscall(instructionSignal);
        }
        else {
            // this is normally used on mips
            uint registerSignal = e.V0;
            await ExecuteSyscall(registerSignal);
        }
    }

    /// <summary>
    /// Function that will be called when a syscall is executed.
    /// </summary>
    /// <param name="code"></param>
    protected abstract ValueTask ExecuteSyscall(uint code);
}