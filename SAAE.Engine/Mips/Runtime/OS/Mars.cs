using System.Text;
using SAAE.Engine.Mips.Instructions;
using SAAE.Engine.Mips.Runtime.Simple;

namespace SAAE.Engine.Mips.Runtime;

/// <summary>
/// Operating system that mocks the MARS
/// environment syscalls.
/// </summary>
public class Mars : OperatingSystem {
    
    protected override void OnSyscall(uint code) {

        switch (code) {
            case 1:
                PrintInteger();
                break;
            case 2:
                PrintFloat();
                break;
            case 3:
                PrintDouble();
                break;
            case 4:
                PrintString();
                break;
            case 5:
                ReadInteger();
                break;
            case 6:
                ReadFloat();
                break;
            case 7:
                ReadDouble();
                break;
            case 8:
                ReadString();
                break;
            case 9:
                Sbrk();
                break;
            case 10:
                Exit();
                break;
            case 11:
                PrintCharacter();
                break;
            case 12:
                ReadCharacter();
                break;
            case 13:
                OpenFile();
                break;
            case 14:
                ReadFromFile();
                break;
            case 15:
                WriteToFile();
                break;
            case 16:
                CloseFile();
                break;
            case 17:
                ExitWithValue();
                break;
        }
    }

    #region Print

    /// <summary>
    /// Prints an integer to the console.
    /// </summary>
    /// <remarks>
    /// $a0 contains the integer to be printed.
    /// </remarks>
    private void PrintInteger() {
        string integer = Machine.Registers[RegisterFile.Register.A0].ToString();
        Machine.StdOut.Write(Encoding.ASCII.GetBytes(integer));
    }

    /// <summary>
    /// Prints a single precision floating point number
    /// to the console.
    /// </summary>
    /// <remarks>
    /// $f12 contains the float to print
    /// </remarks>
    private void PrintFloat() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Prints a double precision floating point number to the console.
    /// </summary>
    /// <remarks>
    /// $f12 contains the double to print
    /// </remarks>
    private void PrintDouble() {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Prints a null terminated string to the console.
    /// </summary>
    /// <remarks>
    /// $a0 contains the base address of the string to print.
    /// Must end with a null character. 
    /// </remarks>
    private void PrintString() {
        StringBuilder sb = new();
        uint address = (uint)Machine.Registers[RegisterFile.Register.A0];
        byte current;
        while ((current = Machine.Memory.ReadByte(address++)) != 0) {
            sb.Append((char)current);
        }
        Machine.StdOut.Write(Encoding.ASCII.GetBytes(sb.ToString()));
    }

    /// <summary>
    /// Prints the low order byte contents as a ASCII character to the console.
    /// </summary>
    /// <remarks>
    /// $a0 contains the character to print.
    /// </remarks>
    private void PrintCharacter() {
        byte character = (byte)Machine.Registers[RegisterFile.Register.A0];
        Machine.StdOut.Write(new byte[] { character });
    }
    
    #endregion

    #region Read

    /// <summary>
    /// Reads an integer from the console.
    /// </summary>
    /// <remarks>
    /// $v0 returns the integer read.
    /// </remarks>
    private void ReadInteger() {
        using var sr = new StreamReader(Machine.StdIn, Encoding.ASCII, leaveOpen: true);
        string? line = sr.ReadLine();
        if (line is null) {
            Machine.Registers[RegisterFile.Register.V0] = int.MinValue;
            return;
        }
        
        if (!int.TryParse(line, out int value)) {
            Machine.Registers[RegisterFile.Register.V0] = int.MinValue;
            return;
        }
        
        Machine.Registers[RegisterFile.Register.V0] = value;
    }
    
    /// <summary>
    /// Reads a single precision floating point number from the console.
    /// </summary>
    /// <remarks>
    /// $f0 returns the float read.
    /// </remarks>
    private void ReadFloat() {
        using var sr = new StreamReader(Machine.StdIn, Encoding.ASCII, leaveOpen: true);
        string? line = sr.ReadLine();
        if (line is null) {
            Machine.Registers[RegisterFile.Register.V0] = BitConverter.SingleToInt32Bits(float.NaN);
            return;
        }
        
        if (!float.TryParse(line, out float value)) {
            Machine.Registers[RegisterFile.Register.V0] = BitConverter.SingleToInt32Bits(float.NaN);
            return;
        }
        
        //Machine.Registers[RegisterFile.Register.V0] = BitConverter.SingleToInt32Bits(value);
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Reads a double precision floating point number from the console.
    /// </summary>
    /// <remarks>
    /// $f0 returns the double read.
    /// </remarks>
    private void ReadDouble() {
        
    }
    
    /// <summary>
    /// Reads a string from the console. Given a maximum buffer length, string can be at
    /// a maximum n-1 characters long. If the string is smaller than buffer, end with newline.
    /// Always pads buffer with null characters. If n==1, input is ignored and null is written
    /// to address. If n&lt;1, input is ignored and nothing is written.
    /// </summary>
    /// <remarks>
    /// $a0 contains the address of the input buffer.<br/>
    /// $a1 contains the maximum number of characters to read
    /// </remarks>
    private void ReadString() {
        int n = Machine.Registers[RegisterFile.Register.A1];
        uint address = (uint)Machine.Registers[RegisterFile.Register.A0];

        if (n < 1) {
            return;
        }else if (n == 1) {
            Machine.Memory.WriteByte(address, 0);
            return;
        }
        
        using var sr = new StreamReader(Machine.StdIn, Encoding.ASCII, leaveOpen: true);
        string? line = sr.ReadLine();
        if (line is null) {
            Machine.Memory.WriteByte(address, 0);
            return;
        }
        
        if(line.Length > n - 1) {
            line = line[..(n - 1)];
        }
        
        if (line.Length < n - 1 && !line.EndsWith('\n')) {
            line += '\n';
        }
        
        byte[] buffer = Encoding.ASCII.GetBytes(line);
        // write buffer and fill with null characters
        for (uint i = 0; i < n; i++) {
            Machine.Memory.WriteByte(address + i, i < buffer.Length ? buffer[i] : (byte)0);
        }
    }
    
    /// <summary>
    /// Reads an ASCII character from the console.
    /// </summary>
    /// <remarks>
    /// $v0 returns the character read.
    /// </remarks>
    private void ReadCharacter() {
        using var sr = new StreamReader(Machine.StdIn, Encoding.ASCII, leaveOpen: true);

        int c = sr.Read();
        Machine.Registers[RegisterFile.Register.V0] = c == -1 ? int.MinValue : c;
    }

    #endregion

    #region System

    /// <summary>
    /// Allocates more heap memory(increasing BRK).
    /// </summary>
    /// <remarks>
    /// $a0 contains the number of bytes to allocate<br/>
    /// $v0 returns the address of the allocated memory
    /// </remarks>
    private void Sbrk() {
        
    }

    /// <summary>
    /// Terminates the execution of the program
    /// </summary>
    private void Exit() {
        
    }

    /// <summary>
    /// Terminates the program with a given value.
    /// </summary>
    /// <remarks>
    /// $a0 contains the exit value.
    /// </remarks>
    private void ExitWithValue() {
        
    }

    #endregion
    
    #region File

    /// <summary>
    /// Opens a handle to a file in the host's physical filesystem.
    /// </summary>
    /// <remarks>
    /// $a0 contains the address of the filename. Is a null terminated string.<br/>
    /// $a1 contains the flags to open the file.<br/>
    /// $a2 contains the mode to open the file.<br/>
    /// $v0 returns the file descriptor or negative value if error ocurred.
    /// </remarks>
    private void OpenFile() {
        
    }

    /// <summary>
    /// Reads a certain amount of characters from an already opened file.
    /// </summary>
    /// <remarks>
    /// $a0 contains the file descriptor.<br/>
    /// $a1 contains the address of the buffer to write the data.<br/>
    /// $a2 contains the maximum number of bytes to read.<br/>
    /// $v0 returns the number of bytes read, negative value if error ocurred or 0 if EOF.
    /// </remarks>
    private void ReadFromFile() {
        
    }
    
    /// <summary>
    /// Writes a certain amount of characters to an already opened file.
    /// </summary>
    /// <remarks>
    /// $a0 contains the file descriptor.<br/>
    /// $a1 contains the address of the buffer to read the data from.<br/>
    /// $a2 contains the number of characters to write.<br/>
    /// $v0 returns the amount of characters written or negative value if error ocurred.
    /// </remarks>
    private void WriteToFile() {
        
    }

    /// <summary>
    /// Closes a file descriptor.
    /// </summary>
    /// <remarks>
    /// $a0 contains the file descriptor.
    /// </remarks>
    private void CloseFile() {
        
    }

    #endregion
}