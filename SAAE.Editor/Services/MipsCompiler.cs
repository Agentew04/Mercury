using System.IO;
using System.Threading.Tasks;

namespace SAAE.Editor.Services;

public class MipsCompiler : ICompilerService {
    public async Task<bool> CompileAssembly(string assemblyCode) {
        // create temporary file
        string assemblyPath = Path.GetTempFileName();
        File.Move(assemblyPath, Path.ChangeExtension(assemblyPath, ".s"));
        assemblyPath = Path.ChangeExtension(assemblyPath, ".s");
        
        await File.WriteAllTextAsync(assemblyPath, assemblyCode);
        
        // compile
        
    }
}