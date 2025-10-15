using SAAE.Engine.Common.Pipeline;

namespace SAAE.Engine.Common;

public class test {
    public static void Test() {
        TemporalBarrier<string> input = new();
        TemporalBarrier<string> ifid = new();
        TemporalBarrier<string> idex = new();
        TemporalBarrier<string> exmem = new();
        TemporalBarrier<string> memwb = new();
        TemporalBarrier<string> output = new();
        
        PipelineStage<string,string> @if = new(input, ifid,x=> x?? "nop");
        PipelineStage<string,string> id = new(ifid, idex,x=> x?? "nop");
        PipelineStage<string,string> ex = new(idex, exmem,x=> x?? "nop");
        PipelineStage<string,string> mem = new(exmem, memwb,x=> x?? "nop");
        PipelineStage<string,string> wb = new(memwb, output,x=> x??"nop");

        List<string> instructions = new() {
            "add",
            "addi",
            "sub",
            "mul"
        };
        for (int i = 0; i < 20; i++) {
            if (i < instructions.Count) {
                input.Write(instructions[i]);
            }
            Tick();
            
            Console.WriteLine($"  {new string(' ',(i+1).ToString().Length)} |\tIF\tID\tEX\tMEM\tWB");
            Console.WriteLine($"t={(i+1).ToString()} |\t{ifid.Read()}\t{ idex.Read()}\t{ exmem.Read()}\t{ memwb.Read()}\t{ output.Read()}");
            Console.WriteLine("----------------------------------------------");
        }
        
        return;
        void Tick() {
            @if.Tick();
            id.Tick();
            ex.Tick();
            mem.Tick();
            wb.Tick();
            
            input.Advance();
            ifid.Advance();
            idex.Advance();
            exmem.Advance();
            memwb.Advance();
            output.Advance();
        }
        
    }
}