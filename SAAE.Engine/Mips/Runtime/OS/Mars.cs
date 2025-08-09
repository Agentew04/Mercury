using System.Text;
using SAAE.Engine.Common;

namespace SAAE.Engine.Mips.Runtime.OS;

/// <summary>
/// Operating system that mocks the MARS
/// environment syscalls.
/// </summary>
public sealed class Mars : MipsOperatingSystem {

    public override string FriendlyName => "Mars 4.5 Runtime";

    public override string Identifier => "mars";

    protected override async ValueTask OnSyscall(uint code) {
        switch (code) {
            case 1:
                await PrintInteger();
                break;
            case 2:
                PrintFloat();
                break;
            case 3:
                PrintDouble();
                break;
            case 4:
                await PrintString();
                break;
            case 5:
                await ReadInteger();
                break;
            case 6:
                await ReadFloat();
                break;
            case 7:
                ReadDouble();
                break;
            case 8:
                await ReadString();
                break;
            case 9:
                //Sbrk();
                break;
            case 10:
                await Exit();
                break;
            case 11:
                await PrintCharacter();
                break;
            case 12:
                await ReadCharacter();
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
                await ExitWithValue();
                break;
            // code < 17 are compatible with SPIM simulator
            // code >= 30 are MARS specific
            case 30:
                SystemTime();
                break;
            case 31:
                //MidiOut();
                break;
            case 32:
                await Sleep();
                break;
            case 33:
                //MidiOutSync();
                break;
            case 34:
                await PrintIntHex();
                break;
            case 35:
                await PrintIntBinary();
                break;
            case 36:
                await PrintUnsignedInt();
                break;
            case 40:
                SetRandomSeed();
                break;
            case 41:
                RandomInt();
                break;
            case 42:
                RandomIntRange();
                break;
            case 43:
                RandomFloat();
                break;
            case 44:
                RandomDouble();
                break;
            case 45:
                await PrintBoolean();
                break;
            case 46:
                await ReadBoolean();
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
    private ValueTask PrintInteger() {
        string integer = Machine.Registers[RegisterFile.Register.A0].ToString();
        return Print(integer);
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
    private ValueTask PrintString() {
        StringBuilder sb = new();
        uint address = (uint)Machine.Registers[RegisterFile.Register.A0];
        byte current;
        while ((current = Machine.Memory.ReadByte(address++)) != 0) {
            sb.Append((char)current);
        }
        return Print(sb.ToString());
    }

    /// <summary>
    /// Prints the low order byte contents as a ASCII character to the console.
    /// </summary>
    /// <remarks>
    /// $a0 contains the character to print.
    /// </remarks>
    private async ValueTask PrintCharacter() {
        byte character = (byte)Machine.Registers[RegisterFile.Register.A0];
        if (Machine.StdOut is not null) {
            await Machine.StdOut.Writer.WriteAsync(Convert.ToChar(character));
        }
    }

    /// <summary>
    /// Prints an integer in hexadecimal format to the console.
    /// The displayed value has 8 digits, left padded with zeros.
    /// </summary>
    /// <remarks>
    /// $a0 contains the integer to print.
    /// </remarks>
    private ValueTask PrintIntHex() {
        string integer = Machine.Registers[RegisterFile.Register.A0].ToString("X8");
        return Print(integer);
    }

    /// <summary>
    /// Prints the binary representation of an integer. The output
    /// is 32 bits long and left padded with zeros.
    /// </summary>
    /// <remarks>
    /// $a0 contains the integer to print.
    /// </remarks>
    private ValueTask PrintIntBinary() {
        string integer = Machine.Registers[RegisterFile.Register.A0].ToString("b32");
        return Print(integer);
    }

    /// <summary>
    /// Prints an unsigned integer to the console.
    /// </summary>
    /// <remarks>
    /// $a0 contains the integer to print.
    /// </remarks>
    private ValueTask PrintUnsignedInt() {
        string integer = ((uint)Machine.Registers[RegisterFile.Register.A0]).ToString();
        return Print(integer);
    }

    /// <summary>
    /// Prints an boolean value to the console.
    /// </summary>
    /// <remarks>
    /// $a0 contains the boolean value to print.
    /// </remarks>
    private ValueTask PrintBoolean() {
        bool value = Machine.Registers[RegisterFile.Register.A0] != 0;
        return Print(value ? "true" : "false");
    }
    
    #endregion

    #region Read

    /// <summary>
    /// Reads an integer from the console.
    /// </summary>
    /// <remarks>
    /// $v0 returns the integer read.
    /// </remarks>
    private async ValueTask ReadInteger() {
        if (Machine.StdIn is null) {
            return;
        }
        string line = await Machine.StdIn.Reader.ReadLine(); // blocking ateh achar \n
        
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
    private async ValueTask ReadFloat() {
        if (Machine.StdIn is null) {
            return;
        }

        throw new NotImplementedException();
        string line = await Machine.StdIn.Reader.ReadLine();
        if (!float.TryParse(line, out float value)) {
            Machine.Registers[RegisterFile.Register.V0] = BitConverter.SingleToInt32Bits(float.NaN);
            return;
        }
        
        //Machine.Registers[RegisterFile.Register.V0] = BitConverter.SingleToInt32Bits(value);
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
    private async ValueTask ReadString() {
        if (Machine.StdIn is null) {
            return;
        }
        
        int n = Machine.Registers[RegisterFile.Register.A1];
        uint address = (uint)Machine.Registers[RegisterFile.Register.A0];

        if (n < 1) {
            return;
        }else if (n == 1) {
            Machine.Memory.WriteByte(address, 0);
            return;
        }

        string line = await Machine.StdIn.Reader.ReadLine();
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
    private async ValueTask ReadCharacter() {
        if (Machine.StdIn is null) {
            return;
        }

        char c = await Machine.StdIn.Reader.ReadAsync();
        Machine.Registers[RegisterFile.Register.V0] = c;
    }

    /// <summary>
    /// Reads a boolean value from the console.
    /// </summary>
    /// <remarks>
    /// $v0 returns 1 if true, 0 if false or if error.
    /// </remarks>
    private async ValueTask ReadBoolean()
    {
        if (Machine.StdIn is null) {
            return;
        }

        string line = await Machine.StdIn.Reader.ReadLine();
        
        // int representation
        if(int.TryParse(line, out int intValue)) {
            Machine.Registers[RegisterFile.Register.V0] = intValue != 0 ? 1 : 0;
            return;
        }
        // bool representation
        line = line.Trim().ToLower();
        if (line is "true" or "1" or "yes" or "y") {
            Machine.Registers[RegisterFile.Register.V0] = 1;
            return;
        }
        Machine.Registers[RegisterFile.Register.V0] = 0;
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
        throw new NotImplementedException();
    }

    /// <summary>
    /// Terminates the execution of the program
    /// </summary>
    private ValueTask Exit() {
        return Machine.Cpu.Halt();
    }

    /// <summary>
    /// Terminates the program with a given value.
    /// </summary>
    /// <remarks>
    /// $a0 contains the exit value.
    /// </remarks>
    private ValueTask ExitWithValue() {
        int code = Machine.Registers[RegisterFile.Register.A0];
        return Machine.Cpu.Halt(code);
    }

    /// <summary>
    /// Returns the current system time
    /// </summary>
    /// <remarks>
    /// $a0 returns the low order 32 bits of the system time<br/>
    /// $a1 returns the high order 32 bits of the system time
    /// </remarks>
    private void SystemTime() {
        long ticks = DateTime.Now.Ticks;
        Machine.Registers[RegisterFile.Register.A0] = (int)(ticks & 0xFFFF_FFFF);
        Machine.Registers[RegisterFile.Register.A1] = (int)(ticks >> 32);
    }

    /// <summary>
    /// Generates a tone and returns immediately.
    /// </summary>
    /// <remarks>
    /// $a0 contains the pitch (0-127)<br/>
    /// $a1 contains the duration in milliseconds<br/>
    /// $a2 contains the instrument (0-127)<br/>
    /// $a3 contains the volume (0-127)
    /// </remarks>
    private void MidiOut() {
        // not implemented yet
    }

    /// <summary>
    /// Sleeps the program for a given amount of time.
    /// In the implementation, only acts when the user tries to execute
    /// the next clock cycle to not block the calling thread.
    /// </summary>
    /// <remarks>
    /// $a0 contains the time to sleep in milliseconds.
    /// </remarks>
    private Task Sleep() {
        return Task.Delay(Machine.Registers[RegisterFile.Register.A0]);
    }

    /// <summary>
    /// Plays a tone and only returns when it is over
    /// </summary>
    /// <remarks>
    /// $a0 contains the pitch (0-127)<br/>
    /// $a1 contains the duration in milliseconds<br/>
    /// $a2 contains the instrument (0-127)<br/>
    /// $a3 contains the volume (0-127)
    /// </remarks>
    private void MidiOutSync() {
        
    }

    #endregion
    
    #region File

    private readonly Dictionary<int, Stream?> fileDescriptors = [];
    
    /// <summary>
    /// Opens a handle to a file in the host's physical filesystem.
    /// </summary>
    /// <remarks>
    /// $a0 contains the address of the filename. Is a null terminated string.<br/>
    /// $a1 contains the flags to open the file.<br/>
    /// $a2 contains the mode to open the file. Mars ignores it.<br/>
    /// $v0 returns the file descriptor or negative value if error ocurred.
    /// </remarks>
    private void OpenFile() {
        if (fileDescriptors.Count == 0) {
            fileDescriptors[0] = Machine.StdIn != null ? new ChannelStream(Machine.StdIn) : null;
            fileDescriptors[1] = Machine.StdOut != null ? new ChannelStream(Machine.StdOut) : null;
            fileDescriptors[2] = Machine.StdErr != null ? new ChannelStream(Machine.StdErr) : null;
        }

        StringBuilder sb = new();
        uint address = (uint)Machine.Registers[RegisterFile.Register.A0];
        try {
            byte current = Machine.Memory.ReadByte(address);
            while (current != 0) {
                sb.Append((char)current);
                current = Machine.Memory.ReadByte(++address);
            }
        }catch (Exception) {
            Machine.Registers[RegisterFile.Register.V0] = -1;
            return;
        }
        string path = sb.ToString();
        if (!File.Exists(path)) {
            Machine.Registers[RegisterFile.Register.V0] = -1;
            return;
        }
        int flags = Machine.Registers[RegisterFile.Register.A1];
        // 0: read only
        // 1: write only with create
        // 9: write only with create and append
        if(flags != 0 && flags != 1 && flags != 9) {
            // que flag eh essa passada?
            Machine.Registers[RegisterFile.Register.V0] = -1;
            return;
        }
        
        int newDescriptor = fileDescriptors.Count;
        fileDescriptors[newDescriptor] = new FileStream(path, flags switch {
            0 => FileMode.Open,
            1 => FileMode.Create,
            9 => FileMode.Append,
            _ => throw new ArgumentOutOfRangeException(nameof(flags), "Invalid file open flag")
        });
        Machine.Registers[RegisterFile.Register.V0] = newDescriptor;
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
        if (fileDescriptors.Count == 0) {
            fileDescriptors[0] = Machine.StdIn != null ? new ChannelStream(Machine.StdIn) : null;
            fileDescriptors[1] = Machine.StdOut != null ? new ChannelStream(Machine.StdOut) : null;
            fileDescriptors[2] = Machine.StdErr != null ? new ChannelStream(Machine.StdErr) : null;
        }
        
        int fileDescriptor = Machine.Registers[RegisterFile.Register.A0];
        int address = Machine.Registers[RegisterFile.Register.A1];
        int n = Machine.Registers[RegisterFile.Register.A2];
        
        if (!fileDescriptors.TryGetValue(fileDescriptor, out Stream? stream)) {
            Machine.Registers[RegisterFile.Register.V0] = -1;
            return;
        }

        if (stream is null) {
            // fail silently. stdin/out/err is not defined. let that slide B)
            return;
        }

        if (!stream.CanRead) {
            Machine.Registers[RegisterFile.Register.V0] = -1;
            return;
        }
        
        byte[] buffer = new byte[n];
        int read = stream.Read(buffer, 0, n);
        if (read == 0) {
            Machine.Registers[RegisterFile.Register.V0] = 0;
            return;
        }
        
        for (int i = 0; i < read; i++) {
            Machine.Memory.WriteByte((uint)(address + i), buffer[i]);
        }
        Machine.Registers[RegisterFile.Register.V0] = read;
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
        if (fileDescriptors.Count == 0) {
            fileDescriptors[0] = Machine.StdIn != null ? new ChannelStream(Machine.StdIn) : null;
            fileDescriptors[1] = Machine.StdOut != null ? new ChannelStream(Machine.StdOut) : null;
            fileDescriptors[2] = Machine.StdErr != null ? new ChannelStream(Machine.StdErr) : null;
        }

        int fileDescriptor = Machine.Registers[RegisterFile.Register.A0];
        int address = Machine.Registers[RegisterFile.Register.A1];
        int n = Machine.Registers[RegisterFile.Register.A2];
        
        if (!fileDescriptors.TryGetValue(fileDescriptor, out Stream? stream)) {
            Machine.Registers[RegisterFile.Register.V0] = -1;
            return;
        }

        if (stream is null) {
            // fail silently. stdin/out/err is not defined.
            return;
        }
        
        if (!stream.CanWrite) {
            Machine.Registers[RegisterFile.Register.V0] = -1;
            return;
        }
        
        byte[] buffer = Machine.Memory.Read((uint)address, n);
        stream.Write(buffer, 0, n);
    }

    /// <summary>
    /// Closes a file descriptor.
    /// </summary>
    /// <remarks>
    /// $a0 contains the file descriptor.
    /// </remarks>
    private void CloseFile() {
        int fileDescriptor = Machine.Registers[RegisterFile.Register.A0];
        if (fileDescriptor <= 2) {
            // nao pode fechar stdin, stdout ou stderr
            return; 
        }

        if (!fileDescriptors.TryGetValue(fileDescriptor, out Stream? stream)) {
            return;
        } 
        stream!.Dispose();
        fileDescriptors.Remove(fileDescriptor);
    }

    #endregion

    #region Random

    private readonly Dictionary<int, Random> rngs = [];
    
    /// <summary>
    /// Sets the seed of a random number generator.
    /// </summary>
    /// <remarks>$a0 contains the id of the number generator and $a1 contains
    /// the seed.</remarks>
    private void SetRandomSeed() {
        int id = Machine.Registers[RegisterFile.Register.A0];
        rngs[id] = new Random(Machine.Registers[RegisterFile.Register.A1]);
    }

    /// <summary>
    /// Generates a random integer.
    /// </summary>
    /// <remarks>$a0 containes the id of the generator. $a0 returns the next
    /// random value.</remarks>
    private void RandomInt() {
        int id = Machine.Registers[RegisterFile.Register.A0];
        if (!rngs.TryGetValue(id, out Random? value)) {
            value = new Random();
            rngs[id] = value;
        }
        Machine.Registers[RegisterFile.Register.A0] = value.Next(); 
    }

    /// <summary>
    /// Generates a random integer in a range [0,N]
    /// </summary>
    /// <remarks>The $a0 contains the id of the generator. $a1 contains the upper
    /// bound of the range. Value returned in $a0</remarks>
    private void RandomIntRange() {
        int id = Machine.Registers[RegisterFile.Register.A0];
        if (!rngs.TryGetValue(id, out Random? value)) {
            value = new Random();
            rngs[id] = value;
        }
        Machine.Registers[RegisterFile.Register.A0] = value.Next(Machine.Registers[RegisterFile.Register.A1]); 
    }
    
    /// <summary>
    /// Returns a random 32 floating point number.
    /// </summary>
    /// <remarks>
    /// $a0 contains the id of the generator. $f0 contains the generated number in the range [0,1].
    /// </remarks>
    private void RandomFloat() {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Returns a random 64 floating point number.
    /// </summary>
    /// <remarks>
    /// $a0 contains the id of the generator. $f0 contains the generated number in the range [0,1].
    /// </remarks>
    private void RandomDouble() {
        throw new NotImplementedException();
    }

    #endregion
    
    public override void Dispose() {
        foreach ((int descriptor, Stream? stream) in fileDescriptors) {
            if (descriptor <= 2) {
                continue;
            }
            stream?.Dispose();
        }
    }

    private ValueTask Print(string s) {
        return Machine.StdOut is not null ? Machine.StdOut.Writer.WriteAsync(s) : ValueTask.CompletedTask;
    }
}