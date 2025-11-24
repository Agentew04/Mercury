using System.Text;
using System.Threading.Channels;

namespace Mercury.Engine.Common;

public class ChannelStream : Stream {

    private readonly Channel<char> channel;
    private readonly Encoding encoding;

    public ChannelStream(Channel<char> channel) {
        this.channel = channel;
        encoding = Encoding.GetEncoding("ASCII", EncoderFallback.ReplacementFallback,
            DecoderFallback.ReplacementFallback);
    }

    public override void Flush() {
        // nothing
    }

    public override int Read(byte[] buffer, int offset, int count) {
        int read = Math.Min(count, channel.Reader.Count);
        char[] buf = new char[1];
        for (int i = 0; i < read; i++) {
            if (!channel.Reader.TryRead(out buf[0])) {
                return i;
            }
            buffer[offset + i] = encoding.GetBytes(buf)[0];
        }
        return read;
    }

    public override long Seek(long offset, SeekOrigin origin) {
        throw new NotSupportedException();
    }

    public override void SetLength(long value) {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count) {
        for (int i = offset; i < offset + count; i++) {
            _ = channel.Writer.TryWrite((char)buffer[i]);
        }
    }

    public override bool CanRead { get; } = true;
    public override bool CanSeek { get; } = false;
    public override bool CanWrite { get; } = true;
    public override long Length { get; } = -1;
    public override long Position { get; set; } = -1;
}

/// <summary>
/// A channel that uses a stream as the underlying device.
/// </summary>
/// <typeparam name="T">The type of data trasferred</typeparam>
public sealed class StreamChannel : Channel<char>, IDisposable {

    public StreamChannel(Stream stream) {
        if (stream.CanWrite) {
            Writer = new StreamCharWriter(stream);
        }
        if (stream.CanRead) {
            Reader = new StreamCharReader(stream);
        }
    }

    private sealed class StreamCharReader(Stream s) : ChannelReader<char>, IDisposable {

        private readonly StreamReader reader = new(s);

        public override bool TryRead(out char item) {
            int result = reader.Read();
            if (result == -1) {
                item = '\0';
                return false;
            }
            item = (char)result;
            return true;
        }

        public override ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = new CancellationToken()) {
            return new ValueTask<bool>(true);
        }

        public void Dispose() {
            reader.Dispose();
        }
    }

    private sealed class StreamCharWriter(Stream s) : ChannelWriter<char>, IDisposable {

        private readonly StreamWriter writer = new(s);

        public override bool TryWrite(char item) {
            writer.Write(item);
            return true;
        }

        public override ValueTask<bool> WaitToWriteAsync(CancellationToken cancellationToken = new CancellationToken()) {
            return new ValueTask<bool>(true);
        }
        
        public void Dispose() {
            s.Dispose();
        }
    }

    public void Dispose() {
        if(Reader is IDisposable d1) { d1.Dispose(); }
        if(Writer is IDisposable d2) { d2.Dispose(); }
    }
}