using System.Text;
using System.Threading.Channels;

namespace Mercury.Engine.Common;

public class ChannelStream : Stream {

    private readonly Channel<char> channel;
    private Encoding encoding;

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