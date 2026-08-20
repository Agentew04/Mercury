using System.Globalization;
using System.Text;
using Mercury.Engine.Common;
using Mercury.Engine.Common.Events;

namespace Mercury.Engine.Mips.Runtime.OS;

/// <summary>
/// Operating system that mocks the MARS
/// environment syscalls.
/// </summary>
public sealed class Mars : MipsSyscallModule {

    public override string FriendlyName => "Mars 4.5 Runtime";

    public override string Identifier => "mars";

    protected override async ValueTask ExecuteSyscall(uint code) {
        switch (code) {
            case 1:
                await PrintInteger();
                break;
            case 2:
                await PrintFloat();
                break;
            case 3:
                await PrintDouble();
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
                await ReadDouble();
                break;
            case 8:
                await ReadString();
                break;
            case 9:
                Sbrk();
                break;
            case 10:
                Exit();
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
                await ReadFromFile();
                break;
            case 15:
                await WriteToFile();
                break;
            case 16:
                CloseFile();
                break;
            case 17:
                ExitWithValue();
                break;
            // code < 17 are compatible with SPIM simulator
            // code >= 30 are MARS specific
            case 30:
                SystemTime();
                break;
            case 31:
                MidiOut();
                break;
            case 32:
                await Sleep();
                break;
            case 33:
                MidiOutSync();
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
        string integer = Context.A0.ToString();
        return Print(integer);
    }

    /// <summary>
    /// Prints a single precision floating point number
    /// to the console.
    /// </summary>
    /// <remarks>
    /// $f12 contains the float to print
    /// </remarks>
    private ValueTask PrintFloat() {
        int value = (int)Context.F12;
        float flt = BitConverter.Int32BitsToSingle(value);
        return Print(flt.ToString(CultureInfo.CurrentCulture));
    }

    /// <summary>
    /// Prints a double precision floating point number to the console.
    /// </summary>
    /// <remarks>
    /// $f12 contains the double to print
    /// </remarks>
    private ValueTask PrintDouble() {
        int value1 = (int)Context.F12;
        int value2 = (int)Context.F13;
        long value = (long)value1 << 32;
        value |= (uint)value2;
        double dlb = BitConverter.Int64BitsToDouble(value);
        return Print(dlb.ToString(CultureInfo.CurrentCulture));
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
        uint address = Context.A0;
        byte current;
        while ((current = ReadByte(address++)) != 0) {
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
    private ValueTask PrintCharacter() {
       byte character = (byte)Context.A0;
       return stdout.WriteAsync(Convert.ToChar(character));
    }

    /// <summary>
    /// Prints an integer in hexadecimal format to the console.
    /// The displayed value has 8 digits, left padded with zeros.
    /// </summary>
    /// <remarks>
    /// $a0 contains the integer to print.
    /// </remarks>
    private ValueTask PrintIntHex() {
        string integer = Context.A0.ToString("X8");
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
        string integer = Context.A0.ToString("b32");
        return Print(integer);
    }

    /// <summary>
    /// Prints an unsigned integer to the console.
    /// </summary>
    /// <remarks>
    /// $a0 contains the integer to print.
    /// </remarks>
    private ValueTask PrintUnsignedInt() {
        string integer = (Context.A0).ToString();
        return Print(integer);
    }

    /// <summary>
    /// Prints an boolean value to the console.
    /// </summary>
    /// <remarks>
    /// $a0 contains the boolean value to print.
    /// </remarks>
    private ValueTask PrintBoolean() {
        bool value = Context.A0 != 0;
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
        string line = await GetString();
        
        if (!int.TryParse(line, out int value)) {
            Context.RespondV0(int.MinValue);
            return;
        }

        Context.RespondV0(value);
    }
    
    /// <summary>
    /// Reads a single precision floating point number from the console.
    /// </summary>
    /// <remarks>
    /// $f0 returns the float read.
    /// </remarks>
    private async ValueTask ReadFloat() {
        string line = await GetString();
        if (!float.TryParse(line, out float value)) {
            Context.RespondF0(BitConverter.SingleToInt32Bits(float.NaN));
            return;
        }

        int valueBinary = BitConverter.SingleToInt32Bits(value);
        Context.RespondF0(valueBinary);
    }
    
    /// <summary>
    /// Reads a double precision floating point number from the console.
    /// </summary>
    /// <remarks>
    /// $f0 returns the double read.
    /// </remarks>
    private async ValueTask ReadDouble() {
        string line = await GetString();
        if (!double.TryParse(line, out double value)) {
            long nanBin = BitConverter.DoubleToInt64Bits(double.NaN);
            Context.RespondF0((int)(nanBin >> 32));
            Context.RespondF1((int)(nanBin & 0xFFFF_FFFF));
            return;
        }

        long valueBinary = BitConverter.DoubleToInt64Bits(value);
        Context.RespondF0((int)(valueBinary >> 32));
        Context.RespondF1((int)(valueBinary & 0xFFFF_FFFF));
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
        int n = (int)Context.A1;
        uint address = Context.A0;

        if (n < 1) {
            return;
        }
        if (n == 1) {
            WriteByte(address,0);
            return;
        }

        Memory<char> bufferChars = new char[n];
        Memory<byte> bufferBytes = new byte[n];
        int read = 0;
        await eventBus.PublishAsync(new StdInReadEvent() {
            Buffer = bufferChars,
            Delimiter = '\n',
            OnReadComplete = r => read = r
        });
        
        Encoding.ASCII.GetBytes(bufferChars.Span, bufferBytes.Span);
        // pad with zeros
        for (int i = read; i < n; i++) {
            bufferBytes.Span[i] = 0;
        }
        eventBus.Publish(new MemoryWriteEvent() {
            Buffer = bufferBytes,
            Size = (ulong)n,
            Address = address
        });
    }
    
    /// <summary>
    /// Reads an ASCII character from the console.
    /// </summary>
    /// <remarks>
    /// $v0 returns the character read.
    /// </remarks>
    private async ValueTask ReadCharacter() {
        char c = await stdin.ReadCharAsync();
        Context.RespondV0(c);
    }

    /// <summary>
    /// Reads a boolean value from the console.
    /// </summary>
    /// <remarks>
    /// $v0 returns 1 if true, 0 if false or if error.
    /// </remarks>
    private async ValueTask ReadBoolean() {
        string line = await GetString();
        
        // int representation
        if(int.TryParse(line, out int intValue)) {
            Context.RespondV0(intValue != 0 ? 1 : 0);
            return;
        }
        // bool representation
        line = line.Trim().ToLower();
        if (line is "true" or "1" or "yes" or "y") {
            Context.RespondV0(1);
            return;
        }
        Context.RespondV0(0);
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
    private void Exit() {
        eventBus.Publish(new HaltEvent() {
            ExitCode = 0,
            Address = 0 // TODO: como pegar o pc atual?
        });
    }

    /// <summary>
    /// Terminates the program with a given value.
    /// </summary>
    /// <remarks>
    /// $a0 contains the exit value.
    /// </remarks>
    private void ExitWithValue() {
        eventBus.Publish(new HaltEvent() {
            ExitCode = (int)Context.A0,
            Address = 0 // TODO: como pegar o pc atual?
        });
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
        Context.RespondA0((int)(ticks & 0xFFFF_FFFF));
        Context.RespondA1((int)(ticks >> 32));
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
        return Task.Delay((int)Context.A0);
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
        throw new NotImplementedException();
    }

    #endregion
    
    #region File

    private readonly Dictionary<int, Stream> fileDescriptors = [];
    
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
        EnsureStdioDescriptors();

        StringBuilder sb = new();
        uint address = Context.A0;
        try {
            byte current = ReadByte(address);
            while (current != 0) {
                sb.Append((char)current);
                current = ReadByte(++address);
            }
        }catch (Exception) {
            Context.RespondV0(-1);
            return;
        }
        string path = sb.ToString();
        if (!File.Exists(path)) {
            Context.RespondV0(-1);
            return;
        }

        int flags = (int)Context.A1;
        // 0: read only
        // 1: write only with create
        // 9: write only with create and append
        if(flags != 0 && flags != 1 && flags != 9) {
            // que flag eh essa passada?
            Context.RespondV0(-1);
            return;
        }
        
        int newDescriptor = fileDescriptors.Count;
        fileDescriptors[newDescriptor] = new FileStream(path, flags switch {
            0 => FileMode.Open,
            1 => FileMode.Create,
            9 => FileMode.Append,
            _ => throw new ArgumentOutOfRangeException(nameof(flags), "Invalid file open flag")
        });
        Context.RespondV0(newDescriptor);
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
    private async Task ReadFromFile() {
        EnsureStdioDescriptors();
        
        int fileDescriptor = (int)Context.A0;
        int address = (int)Context.A1;
        int n = (int)Context.A2;
        
        if (!fileDescriptors.TryGetValue(fileDescriptor, out Stream? stream) || !stream.CanRead) {
            Context.RespondV0(-1);
            return;
        }

        byte[] buffer = new byte[n];
        int read = await stream.ReadAsync(buffer.AsMemory(0, n));
        if (read == 0) {
            Context.RespondV0(0);
            return;
        }
        
        for (int i = 0; i < read; i++) {
            WriteByte((uint)(address + i), buffer[i]);
        }
        Context.RespondV0(read);
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
    private async Task WriteToFile() {
        EnsureStdioDescriptors();

        int fileDescriptor = (int)Context.A0;
        ulong address = Context.A1;
        ulong n = Context.A2;
        
        if (!fileDescriptors.TryGetValue(fileDescriptor, out Stream? stream) || !stream.CanWrite) {
            Context.RespondV0(-1);
            return;
        }
        Memory<byte> buffer = new byte[n];
        eventBus.Publish(new MemoryReadEvent() {
            Address = address,
            Buffer = buffer,
            Size = n
        });
        await stream.WriteAsync(buffer);
    }

    /// <summary>
    /// Closes a file descriptor.
    /// </summary>
    /// <remarks>
    /// $a0 contains the file descriptor.
    /// </remarks>
    private void CloseFile() {
        int fileDescriptor = (int)Context.A0;
        if (fileDescriptor <= 2) {
            // nao pode fechar stdin, stdout ou stderr
            return; 
        }

        if (!fileDescriptors.TryGetValue(fileDescriptor, out Stream? stream)) {
            return;
        } 
        stream.Dispose();
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
        int id = (int)Context.A0;
        rngs[id] = new Random((int)Context.A1);
    }

    /// <summary>
    /// Generates a random integer.
    /// </summary>
    /// <remarks>$a0 containes the id of the generator. $a0 returns the next
    /// random value.</remarks>
    private void RandomInt() {
        int id = (int)Context.A0;
        if (!rngs.TryGetValue(id, out Random? value)) {
            value = new Random();
            rngs[id] = value;
        }
        Context.RespondA0(value.Next()); 
    }

    /// <summary>
    /// Generates a random integer in a range [0,N]
    /// </summary>
    /// <remarks>The $a0 contains the id of the generator. $a1 contains the upper
    /// bound of the range. Value returned in $a0</remarks>
    private void RandomIntRange() {
        int id = (int)Context.A0;
        if (!rngs.TryGetValue(id, out Random? value)) {
            value = new Random();
            rngs[id] = value;
        }
        Context.RespondA0(value.Next((int)Context.A1)); 
    }
    
    /// <summary>
    /// Returns a random 32 floating point number.
    /// </summary>
    /// <remarks>
    /// $a0 contains the id of the generator. $f0 contains the generated number in the range [0,1].
    /// </remarks>
    private void RandomFloat() {
        int id = (int)Context.A0;
        if (!rngs.TryGetValue(id, out Random? value)) {
            value = new Random();
            rngs[id] = value;
        }
        float flt = (float)value.NextDouble();
        int fltBinary = BitConverter.SingleToInt32Bits(flt);
        Context.RespondF0(fltBinary);
    }
    
    /// <summary>
    /// Returns a random 64 floating point number.
    /// </summary>
    /// <remarks>
    /// $a0 contains the id of the generator. $f0 contains the generated number in the range [0,1].
    /// </remarks>
    private void RandomDouble() {
        int id = (int)Context.A0;
        if (!rngs.TryGetValue(id, out Random? value)) {
            value = new Random();
            rngs[id] = value;
        }
        double dlb = value.NextDouble();
        long dlbBinary = BitConverter.DoubleToInt64Bits(dlb);
        Context.RespondF0((int)(dlbBinary >> 32));
        Context.RespondF1((int)(dlbBinary & 0xFFFF_FFFF));
    }

    #endregion
    
    public override void Dispose() {
        foreach ((int descriptor, Stream stream) in fileDescriptors) {
            if (descriptor <= 2) {
                continue;
            }
            stream.Dispose();
        }
    }

    private ValueTask Print(string s) {
        return eventBus.PublishAsync(new StdOutWriteEvent() {
            Data = s.AsMemory(),
        });
    }

    private async ValueTask<string> GetString() {
        int read = 0;
        await eventBus.PublishAsync(new StdInReadEvent() {
            Buffer = inputBuffer,
            Delimiter = '\n',
            OnReadComplete = r => read = r
        });
        string s = new(inputBuffer[..read].Span);
        return s;
    }
    
    private void EnsureStdioDescriptors() {
        if (fileDescriptors.Count != 0) {
            return;
        }
        fileDescriptors[0] = new ChannelStream(stdin);
        fileDescriptors[1] = new ChannelStream(stdout);
        fileDescriptors[2] = new ChannelStream(stderr);
    }

    private readonly Memory<byte> wordBuffer = new byte[4]; 
    private readonly Memory<char> inputBuffer = new char[64];
    
    private byte ReadByte(ulong address) {
        eventBus.Publish(new MemoryReadEvent() {
            Address = address,
            Buffer = wordBuffer,
            Size = 1
        });
        return wordBuffer.Span[0];
    }

    private void WriteByte(ulong address, byte value) {
        wordBuffer.Span[0] = value;
        eventBus.Publish(new MemoryWriteEvent() {
            Address = address,
            Buffer = wordBuffer,
            Size = 1
        });
    }
}