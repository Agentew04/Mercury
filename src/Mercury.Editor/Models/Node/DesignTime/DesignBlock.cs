using System.Collections.Generic;

namespace Mercury.Editor.Models.Node.DesignTime;

public record DesignBlock(string Name, List<IoItem> Inputs, List<IoItem> Outputs, bool IsBarrier, string Source);
