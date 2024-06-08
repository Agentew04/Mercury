using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SAAE.Engine.Mips.Assembler {
    internal partial class IntructionsRegex {

        #region Simple

        #region Arithmetic
#pragma warning disable IDE0090
#pragma warning disable SYSLIB1045
        public Regex Add = new Regex(@"add\s+\$(?<rd>\d+),\s+\$(?<rs>\d+),\s+\$(?<rt>\d+)");
        public Regex Subtract = new Regex(@"sub\s+\$(?<rd>\d+),\s+\$(?<rs>\d+),\s+\$(?<rt>\d+)");
        public Regex AddImmediate = new Regex(@"addi\s+\$(?<rt>\d+),\s+\$(?<rs>\d+),\s+(?<imm>\d+)");
        public Regex AddUnsigned = new Regex(@"addu\s+\$(?<rd>\d+),\s+\$(?<rs>\d+),\s+\$(?<rt>\d+)");
        public Regex SubtractUnsigned = new Regex(@"subu\s+\$(?<rd>\d+),\s+\$(?<rs>\d+),\s+\$(?<rt>\d+)");
        public Regex AddImmediateUnsigned = new Regex(@"addiu\s+\$(?<rt>\d+),\s+\$(?<rs>\d+),\s+(?<imm>\d+)");
        public Regex MultiplyWithoutOverflow = new Regex(@"mult\s+\$(?<rs>\d+),\s+\$(?<rt>\d+)");
        public Regex Multiply = new Regex(@"multu\s+\$(?<rs>\d+),\s+\$(?<rt>\d+)");
        public Regex Divide = new Regex(@"div\s+\$(?<rs>\d+),\s+\$(?<rt>\d+)");

        #endregion

        #region Logical

        public Regex And = new Regex(@"and\s+\$(?<rd>\d+),\s+\$(?<rs>\d+),\s+\$(?<rt>\d+)");
        public Regex Or = new Regex(@"or\s+\$(?<rd>\d+),\s+\$(?<rs>\d+),\s+\$(?<rt>\d+)");
        public Regex AndImmediate = new Regex(@"andi\s+\$(?<rt>\d+),\s+\$(?<rs>\d+),\s+(?<imm>\d+)");
        public Regex OrImmediate = new Regex(@"ori\s+\$(?<rt>\d+),\s+\$(?<rs>\d+),\s+(?<imm>\d+)");
        public Regex ShiftLeftLogical = new Regex(@"sll\s+\$(?<rd>\d+),\s+\$(?<rt>\d+),\s+(?<shamt>\d+)");
        public Regex ShiftRightLogical = new Regex(@"srl\s+\$(?<rd>\d+),\s+\$(?<rt>\d+),\s+(?<shamt>\d+)");

        #endregion

        #region Data Transfer

        public Regex LoadWord = new Regex(@"lw\s+\$(?<rt>\d+),\s+(?<imm>\d+)\(\$(?<rs>\d+)\)");
        public Regex StoreWord = new Regex(@"sw\s+\$(?<rt>\d+),\s+(?<imm>\d+)\(\$(?<rs>\d+)\)");
        public Regex LoadUpperImmediate = new Regex(@"lui\s+\$(?<rt>\d+),\s+(?<imm>\d+)");
        public Regex MoveFromHi = new Regex(@"mfhi\s+\$(?<rd>\d+)");
        public Regex MoveFromLo = new Regex(@"mflo\s+\$(?<rd>\d+)");

        #endregion

        #region Branching

        public Regex BranchEqual = new Regex(@"beq\s+\$(?<rs>\d+),\s+\$(?<rt>\d+),\s+(?<label>\w+)");
        public Regex BranchNotEqual = new Regex(@"bne\s+\$(?<rs>\d+),\s+\$(?<rt>\d+),\s+(?<label>\w+)");

        #endregion

        #region Comparison

        public Regex SetOnLessThan = new Regex(@"slt\s+\$(?<rd>\d+),\s+\$(?<rs>\d+),\s+\$(?<rt>\d+)");
        public Regex SetOnLessThanImmediate = new Regex(@"slti\s+\$(?<rt>\d+),\s+\$(?<rs>\d+),\s+(?<imm>\d+)");

        #endregion

        #region Jump

        public Regex Jump = new Regex(@"j\s+(?<label>\w+)");
        public Regex JumpRegister = new Regex(@"jr\s+\$(?<rs>\d+)");
        public Regex JumpAndLink = new Regex(@"jal\s+(?<label>\w+)");

        #endregion

        #region Floating Point

        #endregion


        #endregion

        #region Pseudo

        #endregion
    }
}
