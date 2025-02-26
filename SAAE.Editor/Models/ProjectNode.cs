using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SAAE.Editor.Models;

/// <summary>
/// Os tipos que um nó na arvore de projeto pode ser. 
/// </summary>
public enum ProjectNodeType {
    None,
    Category,
    Folder,
    AssemblyFile,
    UnknownFile
}

/// <summary>
/// Representa um nó na arvore de arquivos do projeto atual.
/// </summary>
public partial class ProjectNode : ObservableObject {

    [ObservableProperty]
    private string name = string.Empty;
    
    [ObservableProperty]
    private ProjectNodeType type = ProjectNodeType.None;
    
    [ObservableProperty]
    private ObservableCollection<ProjectNode> children = [];

    public Guid Id { get; set; }
}

