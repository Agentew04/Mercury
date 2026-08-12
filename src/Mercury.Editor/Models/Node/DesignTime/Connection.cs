namespace Mercury.Editor.Models.Node.DesignTime;

public record Connection(DesignBlock Start, int StartOutputIndex, DesignBlock End, int EndInputIndex);
