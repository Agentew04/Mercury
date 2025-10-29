namespace SAAE.Engine.Common.Pipeline;

public class PipelineStage<TIn, TOut> {
    private readonly Func<TIn?,TOut> process;
    private readonly TemporalBarrier<TIn> input;
    private readonly TemporalBarrier<TOut>? output;
    private readonly bool outputOnNull;

    public PipelineStage(TemporalBarrier<TIn> input, TemporalBarrier<TOut>? output, Func<TIn?, TOut> process, bool outputOnNull = true) {
        this.process = process;
        this.input = input;
        this.output = output;
        this.outputOnNull = outputOnNull;
    }

    public void Tick() {
        TIn? item = default;
        if (input.HasValue) {
            item = input.Read();
        }
        TOut result = process(item);
        if(!input.HasValue && !outputOnNull) {
            return;
        }
        output?.Write(result);
    }
}