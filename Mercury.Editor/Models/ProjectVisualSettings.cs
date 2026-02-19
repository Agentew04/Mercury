using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Mercury.Editor.Models;

/// <summary>
/// A class that holds all the visual settings of a project, such as custom sizes of elements and opened nodes in
/// the project view interface.
/// </summary>
public class ProjectVisualSettings {
    
    /// <summary>
    /// List with all custom sizes of different elements throughout the UI.
    /// </summary>
    [XmlArray("ElementSizes")]
    public List<ElementSize> ElementSizes { get; set; } = [];
    
    /// <summary>
    /// A list of nodes that the user opened in the file tree. Do not assume this list is ordered or that is contains
    /// only valid items. Folders and files can be moved/deleted with the app closed.
    /// </summary>
    [XmlArray("OpenProjectNodes")]
    public List<OpenProjectNode> OpenProjectNodes { get; set; } = [];
}

[XmlRoot("Element")]
public class ElementSize {
    
    [XmlAttribute("Name")]
    public string Name { get; set; } = string.Empty;
    
    [XmlAttribute("Size")]
    public int Size { get; set; } = -1;
}

[XmlRoot("Node")]
public class OpenProjectNode {
    
    [XmlAttribute("Id")]
    public Guid NodeId { get; set; } = Guid.Empty;
    
    [XmlAttribute("Open")]
    public bool IsOpen { get; set; }
}