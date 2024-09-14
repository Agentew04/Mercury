using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstructionCreator {
    record struct Serialize(string Type, bool Fixed, int Size, string Value);
}
