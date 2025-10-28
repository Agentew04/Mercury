using System.Threading.Channels;
using Mercury.Engine.Memory;
using Mercury.Engine.Mips.Runtime;

namespace Mercury.Engine.Common.Builders;

/// <summary>
/// The standard builder for creating a machine instance. Must be extended for specific machine types because
/// the <see cref="Build"/> method will throw an exception if not overridden.
/// </summary>
public class MachineBuilder : IBuilder<Machine>
{
    protected IMemory? Memory { get; private set; }
    protected Channel<char>? StdIn { get; private set; }
    protected Channel<char>? StdOut { get; private set; }
    protected Channel<char>? StdErr { get; private set; }

    public MachineBuilder()
    {
        // Default constructor for user
    }
    
    protected MachineBuilder(MachineBuilder m)
    {
        Memory = m.Memory;
        StdIn = m.StdIn;
        StdOut = m.StdOut;
        StdErr = m.StdErr;
    }

    public MachineBuilder WithMemory(IMemory memory)
    {
        Memory = memory;
        return this;
    }

    public MachineBuilder WithInMemoryStdio() {
        StdIn = Channel.CreateUnbounded<char>();
        StdOut = Channel.CreateUnbounded<char>();
        StdErr = Channel.CreateUnbounded<char>();
        return this;
    }
    
    public MachineBuilder WithStdio(Channel<char> stdin, Channel<char> stdout, Channel<char> stderr)
    {
        StdIn = stdin;
        StdOut = stdout;
        StdErr = stderr;
        return this;
    }

    public MipsMachineBuilder WithMips()
    {
        return new MipsMachineBuilder(this);
    }

    public virtual Machine Build()
    {
        throw new InvalidOperationException("Build method must be overridden in derived class.");
    }
}
