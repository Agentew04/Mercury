using Mercury.Engine.Common.Events;

namespace Mercury.Engine.Common;

public class ConsoleStdOutModule : IModule {
    
    private EventBus eventBus = null!;
    private readonly List<IDisposable> subscriptions = [];
    
    public void SubscribeToEvents(EventBus bus) {
        eventBus = bus;

        subscriptions.Add(eventBus.Subscribe<StdOutWriteEvent>(e => {
            Console.Write(e.Data.Span);
        }));
    }
    public void UnsubscribeFromEvents() {
        foreach (IDisposable sub in subscriptions) {
            sub.Dispose();
        }
        subscriptions.Clear();
    }

    public void Dispose() {
        UnsubscribeFromEvents();
    }
}