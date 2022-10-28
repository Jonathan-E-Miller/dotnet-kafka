namespace ApiProducer
{
    public sealed class UnknownTopicException : Exception
    {
        public UnknownTopicException(string message) : base(message) { }
    }
}
