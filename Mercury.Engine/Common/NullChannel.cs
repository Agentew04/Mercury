using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;

namespace SAAE.Engine.Common;

public class NullChannel<T> : Channel<T> {
    public NullChannel() {
        Reader = new NullChannelReader();
        Writer = new NullChannelWriter();
    } 
        
    private class NullChannelReader : ChannelReader<T> {
        public override bool TryRead([MaybeNullWhen(false)] out T item) {
            item = default;
            return false;
        }

        public override ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = new()) {
            return ValueTask.FromResult(false);
        }

        public override bool CanCount => true;

        public override int Count => 0;
    }
        
    private class NullChannelWriter : ChannelWriter<T> {
        public override bool TryWrite(T item) {
            return false;
        }

        public override ValueTask<bool> WaitToWriteAsync(CancellationToken cancellationToken = new CancellationToken()) {
            return ValueTask.FromResult(false);
        }
    }
}