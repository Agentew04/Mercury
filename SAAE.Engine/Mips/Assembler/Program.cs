using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Mips.Assembler; 

/// <summary>
/// Represents a MIPS program
/// </summary>
public class Program {

    public Dictionary<string, ulong> Labels { get; set; } = [];

    public Dictionary<string, string> Eqvs { get; set; } = [];

    public List<DataValue> DataValues { get; set; } = [];

    public List<InstructionValue> Instructions { get; set; } = [];
}

/// <summary>
/// Represents a single entry in the .data section.
/// </summary>
public class DataValue {

    public DataType Type { get; set; }

    public ulong Address { get; set; }

    public uint Size { get; set; }

    public byte[] Value { get; set; }

    public DataValue(DataType type, ulong address, uint size, byte[] value) {
        Type = type;
        Address = address;
        Size = size;
        Value = value;
    }

    public enum DataType {
        Byte,
        Half,
        Word,
        Float,
        Double,
        Ascii,
        Asciiz,
        Space,
        Invalid
    }

    public static DataType DirectiveToDataType(string directive) {
        return directive switch {
            ".byte" => DataType.Byte,
            ".half" => DataType.Half,
            ".word" => DataType.Word,
            ".float" => DataType.Float,
            ".double" => DataType.Double,
            ".ascii" => DataType.Ascii,
            ".asciiz" => DataType.Asciiz,
            ".space" => DataType.Space,
            _ => DataType.Invalid
        };
    }

    public static byte[] Serialize(DataType type, string value) {
        return type switch {
            DataType.Byte => [byte.Parse(value)],
            DataType.Half => BitConverter.GetBytes(short.Parse(value)),
            DataType.Word => BitConverter.GetBytes(int.Parse(value)),
            DataType.Float => BitConverter.GetBytes(float.Parse(value)),
            DataType.Double => BitConverter.GetBytes(double.Parse(value)),
            DataType.Ascii => Encoding.ASCII.GetBytes(value),
            DataType.Asciiz => Encoding.ASCII.GetBytes(value + '\0'),
            DataType.Space => new byte[int.Parse(value)],
            _ => []
        };
    }
}

/// <summary>
/// Represents an abstract instruction in a program.
/// </summary>
public class InstructionValue {
    
    public ulong Address { get; set; }

    public string Mnemonic { get; set; }

    public List<Operand> Operands { get; set; } = [];
}

public abstract class Operand {
    public string Value { get; set; } = "";
}

public class RegisterOperand : Operand {}
public class VariableOperand : Operand { }
public class ImmediateOperand : Operand { }
public class LabelOperand : Operand { }
public class MacroParam : Operand { }