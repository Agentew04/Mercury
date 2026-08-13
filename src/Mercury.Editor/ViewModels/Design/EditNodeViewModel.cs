using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Editor.Extensions;
using Mercury.Editor.Views.Design;
using Microsoft.Extensions.Logging;

namespace Mercury.Editor.ViewModels.Design;

public partial class EditNodeViewModel : BaseViewModel<EditNodeViewModel, EditNodeView> {

    [ObservableProperty] private string name = string.Empty;

    [ObservableProperty] private int nodeTypeSelectedIndex;
    [ObservableProperty] private ObservableCollection<string> nodeTypeNames = [];
    
    
    [ObservableProperty] private ObservableCollection<Port> inputs = [];
    [ObservableProperty] private ObservableCollection<Port> outputs = [];
    
    [ObservableProperty] private TextDocument codeDocument = new();
    
    private Dictionary<string, NodeType> localizedNodeTypes = [];
    private Dictionary<NodeType, string> invLocalizedNodeTypes = [];
    private Dictionary<ConnectorViewModel, int> connectorCodes = [];
    private int currentCode = 0;
    private NodeViewModel node;

    public bool Applied { get; private set; } = false;

    public void Load(NodeViewModel node) {
        this.node = node;
        
        Name = node.Name;
        
        // get node type names
        Enum.GetValues<NodeType>()
            .ForEachExt(x => {
                string name = x switch {
                    NodeType.Sequential => Localization.EditNodeResources.NodeTypeSequentialValue,
                    NodeType.Combinational => Localization.EditNodeResources.NodeTypeCombinationalValue,
                    _ => throw new Exception("Invalid NodeType")
                };
                localizedNodeTypes[name] = x;
                invLocalizedNodeTypes[x] = name;
                NodeTypeNames.Add(name);
            });
        NodeTypeSelectedIndex = -1;
        NodeTypeSelectedIndex = NodeTypeNames.IndexOf(invLocalizedNodeTypes[NodeType.Combinational]);
        
        // import in out
        Inputs.Clear();
        Outputs.Clear();
        foreach (ConnectorViewModel input in node.Input) {
            Port p = new() {
                Name = input.Title,
                Bitwidth = input.BitWidth,
                Code = currentCode
            };
            currentCode++;
            connectorCodes[input] = p.Code;
            Inputs.Add(p);
        }
        foreach (ConnectorViewModel output in node.Output) {
            Port p = new() {
                Name = output.Title,
                Bitwidth = output.BitWidth,
                Code = currentCode
            };
            currentCode++;
            connectorCodes[output] = p.Code;
            Outputs.Add(p);
        }
        
        // code
        CodeDocument.Text = node.Code;
    }
    
    [RelayCommand]
    private void Apply() {
        
        // name
        node.Name = Name;
        
        // type
        node.NodeType = localizedNodeTypes[NodeTypeNames[NodeTypeSelectedIndex]];
        
        // in out
        // remove stale connectors and update modified connectors
        List<ConnectorViewModel> toDelete = [];
        foreach (ConnectorViewModel input in node.Input) {
            if (!connectorCodes.TryGetValue(input, out int code)) {
                toDelete.Add(input);
                continue;
            }
            Port port = Inputs.First(x => x.Code == code);
            input.Title = port.Name;
            input.BitWidth = port.Bitwidth;
        }
        toDelete.ForEach(x => node.Input.Remove(x));
        toDelete.Clear();
        foreach (ConnectorViewModel output in node.Output) {
            if (!connectorCodes.TryGetValue(output, out int code)) {
                toDelete.Add(output);
                continue;
            }
            Port port = Outputs.First(x => x.Code == code);
            output.Title = port.Name;
            output.BitWidth = port.Bitwidth;
        }
        toDelete.ForEach(x => node.Output.Remove(x));
        
        // add new connectors
        foreach (Port input in Inputs) {
            if (connectorCodes.ContainsValue(input.Code)) {
                // preexisting connector
                continue;
            }
            node.Input.Add(new ConnectorViewModel() {
                Title = input.Name,
                Type = ConnectorType.Input,
                BitWidth = input.Bitwidth,
                IsConnected = false
            });
        }
        foreach (Port output in Outputs) {
            if (connectorCodes.ContainsValue(output.Code)) {
                // preexisting connector
                continue;
            }
            node.Output.Add(new ConnectorViewModel() {
                Title = output.Name,
                Type = ConnectorType.Output,
                BitWidth = output.Bitwidth,
                IsConnected = false
            });
        }
        
        // code
        node.Code = CodeDocument.Text;

        Applied = true;
        GetView()?.Close();
    }

    [RelayCommand]
    private void AddInput() {
        Port port = new() {
            Name = "New input",
            Bitwidth = 1,
            Code = currentCode
        };
        currentCode++;
        Inputs.Add(port);
    }

    [RelayCommand]
    private void RemoveInput() {
        // check selected input
    }

    [RelayCommand]
    private void AddOutput() {
        Port port = new() {
            Name = "New output",
            Bitwidth = 1,
            Code = currentCode
        };
        currentCode++;
        Outputs.Add(port);
    }

    [RelayCommand]
    private void RemoveOutput() {
        // check selected output
    }

    [RelayCommand]
    private void Cancel() {
        EditNodeView? view = GetView();
        if (view is null) {
            Logger.LogError("Could not find view for EditNodeViewModel. How will we close?");
            return;
        }
        view.Close();
    }
}

public partial class Port : ObservableObject {
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private int bitwidth;
    public int Code { get; set; }
}

public enum NodeType {
    Combinational,
    Sequential
}