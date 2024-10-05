using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstructionCreator.Instruction; 

/// <summary>
/// The base type of a instruction. Defines which
/// runtime type it will be.
/// </summary>
public enum InstructionType {
    RType,
    IType,
    JType,
}
