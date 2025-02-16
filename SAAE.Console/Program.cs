using ELFSharp.ELF;
using static System.Console;

namespace SAAE.Console; 
internal class Program {
    static void Main(string[] args) {

        ELF<uint>? elfreader =
            ELFReader.Load<uint>("C:\\Users\\digoa\\OneDrive\\Documentos\\projetos\\SAAE\\test\\test.exe");
        
        if (elfreader is null) {
            WriteLine("ELF file not found");
            return;
        }
        
        WriteLine("ELF file loaded");
        
    }
}
