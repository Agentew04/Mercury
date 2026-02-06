using Mercury.Engine.Common.Builders;
using Mercury.Engine.Mips.Instructions;
using Mercury.Engine.Mips.Runtime;

namespace Mercury.Engine.Test.Mips.Behaviour;

[TestClass]
public class ArithmeticBehaviourTest {

    [TestMethod(DisplayName = "Test Addition Instructions")]
    public void TestAdd() {
        /*
        addi $t0, $zero, 5
addi $t1, $zero, 6
add $t2, $t0, $t1
addi $t3, $zero, 11
beq $t2, $t3, test_add_ok
nop
break 1
test_add_ok:
add $t0, $zero, $zero
add $t1, $zero, $zero
add $t2, $zero, $zero
add $t3, $zero, $zero
addi $t2, $zero, 27
addi $t3, $zero, 27
beq $t3, $t2, test_addi_ok
nop
break 2
test_addi_ok:
add $t0, $zero, $zero
add $t1, $zero, $zero
add $t2, $zero, $zero
add $t3, $zero, $zero
lui $t0, 0xffff
ori $t0, $t0, 0xffff
addiu $t2, $t0, 1
beq $t2, $zero, test_addiu_ok
nop
break 3
test_addiu_ok:
add $t0, $zero, $zero
add $t1, $zero, $zero
add $t2, $zero, $zero
add $t3, $zero, $zero
lui $t0, 0xffff
ori $t0, $t0, 0xffff
addi $t1, $zero, 1
addu $t2, $t0, $t1
beq $t2, $zero, test_addu_ok
nop
break 4
test_addu_ok:
break 0 
         */
        int[] code = [
            0x20080005,
            0x20090006,
            0x01095020,
            0x200b000b,
            0x114b0002,
            0x00000000,
            0x00004020,
            0x00004820,
            0x00005020,
            0x00005820,
            0x200a001b,
            0x200b001b,
            0x116a0002,
            0x00000000,
            0x0000008d,
            0x00004020,
            0x00004820,
            0x00005020,
            0x00005820,
            0x3c08ffff,
            0x3508ffff,
            0x250a0001,
            0x11400002,
            0x00000000,
            0x000000cd,
            0x00004020,
            0x00004820,
            0x00005020,
            0x00005820,
            0x3c08ffff,
            0x3508ffff,
            0x20090001,
            0x01095021,
            0x11400002,
            0x00000000,
            0x0000010d,
            0x0000000d
        ];

        List<int> breakValues = [];
        using MipsMachine machine = new MachineBuilder()
            .WithMemory(new MemoryBuilder()
                .With4Gb()
                .WithBigEndian()
                .WithBlockSize(128)
                .WithVolatileStorage()
                .WithBlockCapacity(4)
                .Build())
            .WithInMemoryStdio()
            .WithMips()
            .WithMipsMonocycle()
            .WithAnonymousOs(e => {
                if (e.Signal == SignalExceptionEventArgs.SignalType.Breakpoint) {
                    Break @break = new();
                    @break.FromInt((uint)e.Instruction);
                    breakValues.Add(@break.Code);
                }
                return Task.CompletedTask;
            })
            .Build();
        
        machine.LoadProgram(code.AsSpan(), Span<int>.Empty);
        while (!machine.IsClockingFinished()) {
            machine.ClockAsync().GetAwaiter().GetResult();
        }
        
        Assert.HasCount(1, breakValues);
        Assert.AreEqual(0, breakValues[0]);
    }

    [TestMethod(DisplayName = "Test Overflow Signal on Addition Instructions")]
    public void TestAddOverflow() {
        /*
         lui $t0, 0x7fff
or $t0, $t0, 0xffff
addi $t1, $zero, 1
addi $t2, $zero, 47
addi $t3, $zero, 47
add $t2, $t0, $t1
beq $t2, $t3, test_add_ok
nop
break 1
test_add_ok:
add $t0, $zero, $zero
add $t1, $zero, $zero
add $t2, $zero, $zero
add $t3, $zero, $zero
addi $t2, $zero, 47
addi $t3, $zero, 47
lui $t0, 0x7fff
or $t0, $t0, 0xffff
addi $t2, $t0, 1
beq $t2,$t3, test_addi_ok
nop
break 2
test_addi_ok:
break 0
         */
        int[] code = [
            0x3c087fff,
            0x3508ffff,
            0x20090001,
            0x200a002f,
            0x200b002f,
            0x01095020,
            0x114b0002,
            0x00000000,
            0x0000004d,
            0x00004020,
            0x00004820,
            0x00005020,
            0x00005820,
            0x200a002f,
            0x200b002f,
            0x3c087fff,
            0x3508ffff,
            0x210a0001,
            0x114b0002,
            0x00000000,
            0x0000008d,
            0x0000000d
        ];
        
        List<int> breakValues = [];
        int overflowCount = 0;
        using MipsMachine machine = new MachineBuilder()
            .WithMemory(new MemoryBuilder()
                .With4Gb()
                .WithBigEndian()
                .WithBlockSize(128)
                .WithVolatileStorage()
                .WithBlockCapacity(4)
                .Build())
            .WithInMemoryStdio()
            .WithMips()
            .WithMipsMonocycle()
            .WithAnonymousOs(e => {
                if (e.Signal == SignalExceptionEventArgs.SignalType.Breakpoint) {
                    Break @break = new();
                    @break.FromInt((uint)e.Instruction);
                    breakValues.Add(@break.Code);
                }

                if (e.Signal == SignalExceptionEventArgs.SignalType.IntegerOverflow) {
                    overflowCount++;
                }
                return Task.CompletedTask;
            })
            .Build();
        
        machine.LoadProgram(code.AsSpan(), Span<int>.Empty);
        while (!machine.IsClockingFinished()) {
            machine.ClockAsync().GetAwaiter().GetResult();
        }
        
        Assert.HasCount(1,breakValues);
        Assert.AreEqual(0, breakValues[0]);
        Assert.AreEqual(2,overflowCount);
    }

    [TestMethod(DisplayName = "Test HI and LO")]
    public void TestHiLo() {
        /*
         addi $t0, $zero, 52
addi $t1, $zero, 19
mthi $t0
mtlo $t1
mfhi $t2
mflo $t3
beq $t0,$t2, test_hi_ok
nop
break 1
test_hi_ok:
beq $t1, $t3, test_lo_ok
nop
break 2
test_lo_ok:
break 0
         */
        int[] code = [
            0x20080034,
            0x20090013,
            0x01000011,
            0x01200013,
            0x00005010,
            0x00005812,
            0x110a0002,
            0x00000000,
            0x0000004d,
            0x112b0002,
            0x00000000,
            0x0000008d,
            0x0000000d
        ];
        
        List<int> breakValues = [];
        using MipsMachine machine = new MachineBuilder()
            .WithMemory(new MemoryBuilder()
                .With4Gb()
                .WithBigEndian()
                .WithBlockSize(128)
                .WithVolatileStorage()
                .WithBlockCapacity(4)
                .Build())
            .WithInMemoryStdio()
            .WithMips()
            .WithMipsMonocycle()
            .WithAnonymousOs(e => {
                if (e.Signal == SignalExceptionEventArgs.SignalType.Breakpoint) {
                    Break @break = new();
                    @break.FromInt((uint)e.Instruction);
                    breakValues.Add(@break.Code);
                }
                return Task.CompletedTask;
            })
            .Build();
        
        machine.LoadProgram(code.AsSpan(), Span<int>.Empty);
        while (!machine.IsClockingFinished()) {
            machine.ClockAsync().GetAwaiter().GetResult();
        }
        
        Assert.HasCount(1, breakValues);
        Assert.AreEqual(0, breakValues[0]);
    }

    [TestMethod(DisplayName = "Test Division Instructions")]
    public void TestDivision() {
        /*
        addi $t0, $zero, 10
addi $t1, $zero, 2
addi $t3, $zero, 5
div $t0, $t1
nop
mfhi $t2
beq $t2, $zero, div_1_ok
nop
break 1
div_1_ok:
mflo $t2
beq $t2, $t3, div_2_ok
nop
break 2
div_2_ok:
addi $t0, $zero, -10
addi $t1, $zero, 2
addi $t3, $zero, -5
div $t0, $t1
nop
mfhi $t2
beq $t2, $zero, div_3_ok
nop
break 3
div_3_ok:
mflo $t2
beq $t2, $t3, div_4_ok
nop
break 4
div_4_ok:
addi $t0, $zero, 5
addi $t1, $zero, 3
addi $t2, $zero, 1
addi $t3, $zero, 2
div $t0, $t1
nop
mfhi $t4
beq $t4, $t3, div_5_ok
nop
break 5
div_5_ok:
mflo $t4
beq $t4, $t2, div_6_ok
nop
break 6
div_6_ok:
li $t0, 0xff4d92a2
li $t1, 8
li $t2, 0x1fe9b254
li $t4, 2
divu $t0, $t1
mfhi $t3
beq $t3, $t4, divu_1_ok
nop
break 7
divu_1_ok:
mflo $t3
beq $t3, $t2, divu_2_ok
nop
break 8
divu_2_ok:
break 0
         */
        int[] code = [
            0x2008000a,
            0x20090002,
            0x200b0005,
            0x0109001a,
            0x00000000,
            0x00005010,
            0x11400002,
            0x00000000,
            0x0000004d,
            0x00005012,
            0x114b0002,
            0x00000000,
            0x0000008d,
            0x2008fff6,
            0x20090002,
            0x200bfffb,
            0x0109001a,
            0x00000000,
            0x00005010,
            0x11400002,
            0x00000000,
            0x000000cd,
            0x00005012,
            0x114b0002,
            0x00000000,
            0x0000010d,
            0x20080005,
            0x20090003,
            0x200a0001,
            0x200b0002,
            0x0109001a,
            0x00000000,
            0x00006010,
            0x118b0002,
            0x00000000,
            0x0000014d,
            0x00006012,
            0x118a0002,
            0x00000000,
            0x0000018d,
            0x3c01ff4d,
            0x342892a2,
            0x24090008,
            0x3c011fe9,
            0x342ab254,
            0x240c0002,
            0x0109001b,
            0x00005810,
            0x116c0002,
            0x00000000,
            0x000001cd,
            0x00005812,
            0x116a0002,
            0x00000000,
            0x0000020d,
            0x0000000d
        ];
        
        List<int> breakValues = [];
        using MipsMachine machine = new MachineBuilder()
            .WithMemory(new MemoryBuilder()
                .With4Gb()
                .WithBigEndian()
                .WithBlockSize(128)
                .WithVolatileStorage()
                .WithBlockCapacity(4)
                .Build())
            .WithInMemoryStdio()
            .WithMips()
            .WithMipsMonocycle()
            .WithAnonymousOs(e => {
                if (e.Signal == SignalExceptionEventArgs.SignalType.Breakpoint) {
                    Break @break = new();
                    @break.FromInt((uint)e.Instruction);
                    breakValues.Add(@break.Code);
                }
                return Task.CompletedTask;
            })
            .Build();
        
        machine.LoadProgram(code.AsSpan(), Span<int>.Empty);
        while (!machine.IsClockingFinished()) {
            machine.ClockAsync().GetAwaiter().GetResult();
        }
        
        Assert.HasCount(1, breakValues);
        Assert.AreEqual(0, breakValues[0]);
    }
}