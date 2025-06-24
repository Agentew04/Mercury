using SAAE.Engine.Memory;
using SAAE.Engine.Mips.Runtime;

namespace SAAE.Engine.Common.Builders;

/// <summary>
/// The standard builder for creating a machine instance. Must be extended for specific machine types because
/// the <see cref="Build"/> method will throw an exception if not overridden.
/// </summary>
public class MachineBuilder : IBuilder<Machine>
{
    protected IMemory? Memory { get; private set; }
    protected Stream? StdIn { get; private set; }
    protected Stream? StdOut { get; private set; }
    protected Stream? StdErr { get; private set; }

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

    public MachineBuilder WithInMemoryStdio()
    {
        StdIn = new MemoryStream();
        StdOut = new MemoryStream();
        StdErr = new MemoryStream();
        return this;
    }
    
    public MachineBuilder WithStdio(Stream stdin, Stream stdout, Stream stderr)
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
