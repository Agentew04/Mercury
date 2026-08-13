using System.Collections.Generic;

namespace Mercury.Editor.Models.Nodes.DesignTime;

public class Design {
    public List<DesignBlock> Blocks { get; set; } = [];
    public List<Connection> Connections { get; set; } = [];
}