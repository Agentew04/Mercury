using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstructionCreator {
    internal class Instruction {
        public string Id { get; set; } = string.Empty;
        public string Mnemonic { get; set; } = string.Empty;
        public string Arch { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public List<Serialize> Serializes { get; set; } = [];
        public List<Parse> Parses { get; set; } = [];
        public string Fullname { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Usage { get; set; } = string.Empty;
    }
}
