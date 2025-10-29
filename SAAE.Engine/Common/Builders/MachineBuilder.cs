using System.Threading.Channels;
using SAAE.Engine.Memory;
using SAAE.Engine.Mips.Runtime;

namespace SAAE.Engine.Common.Builders;

/// <summary>
/// The standard builder for creating a machine instance. Must be extended for specific machine types because
/// the <see cref="Build"/> method will throw an exception if not overridden.
/// </summary>
public class MachineBuilder : IBuilder<Machine>
{
    protected IMemory? DataMemory { get; private set; }
    protected IMemory? InstructionMemory { get; private set; }
    protected Channel<char>? StdIn { get; private set; }
    protected Channel<char>? StdOut { get; private set; }
    protected Channel<char>? StdErr { get; private set; }

    public MachineBuilder()
    {
        // Default constructor for user
    }
    
    protected MachineBuilder(MachineBuilder m)
    {
        DataMemory = m.DataMemory;
        InstructionMemory = m.InstructionMemory;
        StdIn = m.StdIn;
        StdOut = m.StdOut;
        StdErr = m.StdErr;
    }

    public MachineBuilder WithMemory(IMemory memory)
    {
        DataMemory = memory;
        InstructionMemory = memory;
        return this;
    }

    public MachineBuilder WithInstructionMemory(IMemory memory) {
        InstructionMemory = memory;
        return this;
    }
    
    public MachineBuilder WithDataMemory(IMemory memory) {
        DataMemory = memory;
        return this;
    }

    public MachineBuilder WithInMemoryStdio() {
        StdIn = Channel.CreateUnbounded<char>();
        StdOut = Channel.CreateUnbounded<char>();
        StdErr = Channel.CreateUnbounded<char>();
        return this;
    }
    
    public MachineBuilder WithStdio(Channel<char>? stdin, Channel<char>? stdout, Channel<char>? stderr)
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
