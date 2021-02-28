using System;
using RTSPPlayerServer.Service.Base.Primitives;

namespace RTSPPlayerServer.Service.Systems
{
    /// <summary>
    /// An interface that defines an abstract system.
    /// </summary>
    public interface ISystem
    {
        /// <summary>
        /// System name.
        /// </summary>
        /// <value>
        /// This property contains the (possibly empty) name of the system.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Current status of the system.
        /// </summary>
        SystemStatus Status { get; }

        /// <summary>
        /// Raised when the system status changes.
        /// </summary>
        event EventHandler<SystemStatus, string?>? StatusChanged;

        /// <summary>
        /// Starts the system.
        /// </summary>
        /// <returns>
        /// <c>true</c> true if the system was successfully started;
        /// <c>false</c> otherwise.
        /// </returns>
        bool TryStart();

        /// <summary>
        /// Starts the system.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The system is not finished yet.
        /// </exception>
        void Start();

        /// <summary>
        /// Stops the system.
        /// </summary>
        void Stop();

        /// <summary>
        /// Waits until the system finishes work.
        /// </summary>
        void WaitForFinished();
    }
}
