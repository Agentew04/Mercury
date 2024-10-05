using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InstructionCreator.Instruction;

public enum ISA {
    [XmlEnum(Name = "UNKNOWN")]
    Unknown,
    [XmlEnum(Name = "MIPS I")]
    MIPS_I,
    [XmlEnum(Name = "MIPS II")]
    MIPS_II,
    [XmlEnum(Name = "MIPS III")]
    MIPS_III,
    [XmlEnum(Name = "MIPS IV")]
    MIPS_IV,
    [XmlEnum(Name = "MIPS V")]
    MIPS_V,
    [XmlEnum(Name = "MIPS32")]
    MIPS32,
    [XmlEnum(Name = "MIPS64")]
    MIPS64,
}
