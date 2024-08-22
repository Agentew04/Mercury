namespace SAAE.Engine.Memory; 

[Serializable]
internal class InvalidAddressException : Exception {
    public InvalidAddressException() { }
    public InvalidAddressException(string message) : base(message) { }
    public InvalidAddressException(string message, Exception inner) : base(message, inner) { }
    protected InvalidAddressException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
