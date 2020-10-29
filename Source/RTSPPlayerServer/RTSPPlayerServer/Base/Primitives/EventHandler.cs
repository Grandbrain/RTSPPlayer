namespace RTSPPlayerServer.Base.Primitives
{
    /// <summary>
    /// Represents the method that will handle an event when the event provides data.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="param1">The first parameter to pass.</param>
    /// <param name="param2">The second parameter to pass.</param>
    /// <typeparam name="T1">The type of the <paramref name="param1"/>.</typeparam>
    /// <typeparam name="T2">The type of the <paramref name="param2"/>.</typeparam>
    public delegate void EventHandler<in T1, in T2>(object sender, T1 param1, T2 param2);
}
