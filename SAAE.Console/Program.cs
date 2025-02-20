using ELFSharp.ELF;
using SAAE.Engine.Mips.Runtime;
using static System.Console;
using Machine = SAAE.Engine.Mips.Runtime.Machine;

namespace SAAE.Console; 
internal class Program {
    static void Main(string[] args) {

        ELF<uint>? elf =
            ELFReader.Load<uint>("C:\\Users\\digoa\\OneDrive\\Documentos\\projetos\\SAAE\\test\\test.exe");
        
        if (elf is null) {
            WriteLine("ELF file not found");
            return;
        }
        
        WriteLine("ELF file loaded");
        
        Machine machine = new MachineBuilder()
            .With4GbRam()
            .WithMarsOs()
            .WithMipsMonocycle()
            .Build();

        machine.LoadElf(elf);
        
        machine.OnRegisterChanged += regs => {
            WriteLine("---register changed start---");
            foreach (RegisterFile.Register reg in regs) {
                WriteLine($"Register {reg} changed to {machine.Registers[reg]:X8}");
            }
            WriteLine("---register changed end---");
        };
        
        while (!machine.IsClockingFinished()) {
            machine.Clock();
        }

        WriteLine($"T1: 0x{machine.Registers[RegisterFile.Register.T1]:X8}");
        
    }
}
