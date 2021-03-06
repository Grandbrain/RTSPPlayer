namespace RTSPPlayerServer.Service.Serializers.Interprocess
{
    /// <summary>
    /// An interface that defines an interprocess serializer.
    /// </summary>
    public interface IInterprocessSerializer
    {
        /// <summary>
        /// Serializes the interprocess frame into a string message.
        /// </summary>
        /// <param name="interprocessFrame">Interprocess frame.</param>
        /// <returns>Serialized message.</returns>
        string Serialize(InterprocessFrame interprocessFrame);

        /// <summary>
        /// Deserializes the string message into an interprocess frame.
        /// </summary>
        /// <param name="message">Serialized message.</param>
        /// <returns>Interprocess frame.</returns>
        InterprocessFrame Deserialize(string message);
    }
}
