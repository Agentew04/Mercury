using System.Diagnostics;
using ELFSharp.ELF;
using Mercury.Engine.Common;
using Mercury.Engine.Common.Builders;
using Mercury.Engine.Memory;
using Mercury.Engine.Mips.Runtime;

// create machine
await using Stream stdout = Console.OpenStandardOutput();
using StreamChannel stdoutChannel = new(stdout);

using Memory memory = new MemoryBuilder()
    .With4Gb()
    .WithLittleEndian()
    .WithVolatileStorage()
    .WithBlockCapacity(64)
    .WithBlockSize(4096)
    .Build();

using MipsMachine machine = new MachineBuilder()
    .WithMemory(memory)
    .WithMips()
    .WithMipsMonocycle()
    .WithMarsOs()
    .Build();

// load ELF
if (!File.Exists("bench.elf")) {
    Console.WriteLine("File 'bench.elf' does not exist on current directory({0}).", Directory.GetCurrentDirectory());
    return;
}
ELF<uint> elf = ELFReader.Load<uint>("bench.elf");
machine.LoadElf(elf);


// run test
Stopwatch sw = new();
ulong clocks = 0;
sw.Start();
while (!machine.IsClockingFinished()) {
    await machine.ClockAsync();
    clocks++;
}
sw.Stop();
Console.WriteLine("Instructions Executed: {0}", clocks);
Console.WriteLine("Elapsed time: {0:F3} ms", sw.ElapsedMilliseconds);
double rate = clocks / sw.Elapsed.TotalSeconds;
string[] prefixes = ["", "k", "M", "G"];
int prefix = 0;
while (rate > 1000) {
    prefix++;
    rate /= 1000;
}
Console.WriteLine($"Frequency: {rate:F3}{prefixes[prefix]}Hz");