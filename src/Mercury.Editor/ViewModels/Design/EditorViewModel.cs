using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Editor.Extensions;
using Mercury.Editor.Utils;
using Microsoft.Extensions.Logging;

namespace Mercury.Editor.ViewModels.Design;

public partial class EditorViewModel : ObservableObject {
    public TopLevel? TopLevel { get; set; }
    public ObservableCollection<NodeViewModel> Nodes { get; set; } = [];
    public ObservableCollection<ConnectionViewModel> Connections { get; set; } = [];
    public PendingConnectionViewModel PendingConnection { get; }

    [ObservableProperty]
    private ObservableCollectionEx<ConnectionViewModel> selectedConnections = [];

    [ObservableProperty]
    private ObservableCollectionEx<NodeViewModel> selectedNodes = [];

    private readonly ILogger<EditorViewModel> logger;
    private Dictionary<ConnectorViewModel, NodeViewModel> connectorToNode = [];

    public EditorViewModel(ILogger<EditorViewModel> logger) {
        this.logger = logger;
        PendingConnection = new PendingConnectionViewModel(this);
        var welcome = new NodeViewModel {
            Name = "Welcome23",
            Input = [
                new ConnectorViewModel {
                    Title = "Input 1",
                    Type = ConnectorType.Input
                }
            ],
            Output = [
                new ConnectorViewModel {
                    Title = "Output 1",
                    Type = ConnectorType.Output
                }
            ],
            Location = new Point(200, 200),
        };
        welcome.Output.ForEachExt(x => connectorToNode[x] = welcome);
        welcome.Input.ForEachExt(x => connectorToNode[x] = welcome);
        var nodify = new NodeViewModel {
            Name = "Nodify",
            Input = [
                new ConnectorViewModel {
                    Title = "Input"
                }
            ],
            Output = [
                new ConnectorViewModel {
                    Title = "Output"
                }
            ],
            Location = new Point(400, 400)
        };
        nodify.Output.ForEachExt(x => connectorToNode[x] = nodify);
        nodify.Input.ForEachExt(x => connectorToNode[x] = nodify);
        Nodes.Add(welcome);
        Nodes.Add(nodify);
        SelectedNodes.Add(welcome);

        Connections.Add(new ConnectionViewModel(welcome.Output[0], nodify.Input[0]));
        // Connections.Add(new ConnectionViewModel(nodify.Output[0], welcome.Input[0]));
    }

    partial void OnSelectedConnectionsChanged(ObservableCollectionEx<ConnectionViewModel> value) {
        logger.LogDebug("Selected connections changed");
    }

    partial void OnSelectedNodesChanged(ObservableCollectionEx<NodeViewModel> value) {
        logger.LogDebug("Selected nodes changed");
    }

    public void UseTopLevel() {
        foreach (NodeViewModel node in Nodes) {
            node.TopLevel = TopLevel;
        }
    }

    public void Connect(ConnectorViewModel source, ConnectorViewModel target) {
        if (source.Type != ConnectorType.Output || target.Type != ConnectorType.Input) {
            logger.LogError(
                "Cannot connect \"{SourceTitle}\" (type {ConnectorType}) to \"{TargetTitle}\" (type {TargetType})",
                source.Title, source.Type, target.Title, target.Type);
            return;
        }

        NodeViewModel node = connectorToNode[source];
        NodeViewModel targetNode = connectorToNode[target];
        if (node == targetNode) {
            logger.LogError(
                "Cannot connect two connections in the same node together (\"{source}\" -> \"{target}\") on {node}",
                source.Title, target.Title, targetNode.Name);
            return;
        }

        Connections.Add(new ConnectionViewModel(source, target));
    }

    [RelayCommand]
    private void DisconnectConnector(ConnectorViewModel connector) {
        ConnectionViewModel connection = Connections.First(x => x.Source == connector || x.Target == connector);
        connection.Source.IsConnected = false;
        connection.Target.IsConnected = false;
        Connections.Remove(connection);
    }
}