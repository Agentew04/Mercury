namespace Mercury.Editor.Models.Nodes.DesignTime;

public record Connection(DesignBlock Start, int StartOutputIndex, DesignBlock End, int EndInputIndex) {
    public IoItem StartOutput => Start.Outputs[StartOutputIndex];
    public IoItem EndInput => End.Inputs[EndInputIndex];
}
