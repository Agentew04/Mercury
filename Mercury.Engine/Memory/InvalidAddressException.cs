namespace Mercury.Engine.Memory;

#pragma warning disable S3925
[Serializable]
public class InvalidAddressException : Exception {
    public InvalidAddressException() { }
    public InvalidAddressException(string message) : base(message) { }
    public InvalidAddressException(string message, Exception inner) : base(message, inner) { }
}
#pragma warning restore S3925
