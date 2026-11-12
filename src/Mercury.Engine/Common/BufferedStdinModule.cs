using System.Threading.Channels;
using Mercury.Engine.Common.Events;
using Mercury.Engine.Memory;

namespace Mercury.Engine.Common;

public class BufferedStdinModule : IModule {

    private readonly Channel<char> channel;
    private readonly List<IDisposable> subscriptions = [];
    public ChannelWriter<char> Writer => channel.Writer;

    public BufferedStdinModule() {
        channel = Channel.CreateUnbounded<char>();
    }
    
    public void SubscribeToEvents(EventBus bus) {
        subscriptions.Add(bus.Subscribe<BufferedStdinModule,StdInReadEvent>(this, Handle));
    }

    public void UnsubscribeFromEvents() {
        foreach (IDisposable sub in subscriptions) {
            sub.Dispose();
        }
        subscriptions.Clear();
    }

    public void Serialize(Stream s) { }
    public void Deserialize(Stream s) { }

    private static async ValueTask Handle(BufferedStdinModule state, StdInReadEvent evt) {
        ChannelReader<char> reader = state.channel.Reader;

        int i = 0;
        int max = evt.Buffer.Length;
        // read 
        while (await reader.WaitToReadAsync()) {
            _ = reader.TryRead(out char c);
            if (c == evt.Delimiter) {
                break;
            }
            evt.Buffer.Span[i] = c;
            i++;
            if (i >= max) {
                break;
            }
        }
        evt.OnReadComplete(i);
    }
}