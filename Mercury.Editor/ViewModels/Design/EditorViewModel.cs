using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;

namespace Mercury.Editor.ViewModels.Design;

public partial class EditorViewModel {
    
    public TopLevel? TopLevel { get; set; }
    public ObservableCollection<NodeViewModel> Nodes { get; set; } = [];
    public ObservableCollection<ConnectionViewModel> Connections { get; set; } = [];
    public PendingConnectionViewModel PendingConnection { get; }

    public EditorViewModel() {
        PendingConnection = new PendingConnectionViewModel(this);
        var welcome = new NodeViewModel {
            Name = "Welcome23",
            Input = [
                new ConnectorViewModel {
                    Title = "Input 1"
                }
            ],
            Output = [
                new ConnectorViewModel {
                    Title = "Output 1"
                }
            ],
            Location = new Point(200,200),
        };
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
            Location = new Point(400,400)
        };
        Nodes.Add(welcome);
        Nodes.Add(nodify);
        
        Connections.Add(new ConnectionViewModel(welcome.Output[0],nodify.Input[0]));
        // Connections.Add(new ConnectionViewModel(nodify.Output[0], welcome.Input[0]));
    }

    public void UseTopLevel() {
        foreach (NodeViewModel node in Nodes) {
            node.TopLevel = TopLevel;
        }
    }

    public void Connect(ConnectorViewModel source, ConnectorViewModel target) {
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