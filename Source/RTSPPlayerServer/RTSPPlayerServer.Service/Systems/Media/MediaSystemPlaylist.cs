using System;
using System.Collections.Concurrent;
using RTSPPlayerServer.Service.Base.Extensions;
using RTSPPlayerServer.Service.Streams.Media;

namespace RTSPPlayerServer.Service.Systems.Media
{
    /// <summary>
    /// A class that provides a media system playlist implementation.
    /// </summary>
    public class MediaSystemPlaylist
    {
        /// <summary>
        /// Collection of media streams.
        /// </summary>
        private readonly ConcurrentDictionary<IMediaStream, bool> _mediaStreams =
            new ConcurrentDictionary<IMediaStream, bool>();

        /// <summary>
        /// Adds the specified media stream to the playlist.
        /// </summary>
        /// <param name="mediaStream">Media stream.</param>
        /// <returns>
        /// <c>true</c> if the <paramref name="mediaStream"/> was successfully added to the playlist;
        /// <c>false</c> otherwise.
        /// </returns>
        public bool TryAdd(IMediaStream mediaStream)
        {
            return !TryGet(mediaStream.Name, out _)
                   && _mediaStreams.TryAdd(mediaStream, true);
        }

        /// <summary>
        /// Removes a media stream with the specified name from the playlist.
        /// </summary>
        /// <remarks>
        /// This method simply marks media streams as removed, but does not actually deletes them from the collection.
        /// </remarks>
        /// <param name="name">Media stream name.</param>
        /// <returns>
        /// <c>true</c> if a media stream with the specified name was successfully found and removed from the playlist;
        /// <c>false</c> otherwise.
        /// </returns>
        public bool TryRemove(string name)
        {
            return TryGet(name, out var mediaStream)
                   && mediaStream != null
                   && _mediaStreams.TryUpdate(mediaStream, false, true);
        }

        /// <summary>
        /// Starts a media stream with the specified name.
        /// </summary>
        /// <param name="name">Media stream name.</param>
        /// <returns>
        /// <c>true</c> if a media stream with the specified name was successfully found in the playlist and started;
        /// <c>false</c> otherwise.
        /// </returns>
        public bool TryStart(string name)
        {
            if (!TryGet(name, out var mediaStream) || mediaStream == null)
                return false;

            mediaStream.Start();

            return true;
        }

        /// <summary>
        /// Stops a media stream with the specified name.
        /// </summary>
        /// <param name="name">Media stream name.</param>
        /// <returns>
        /// <c>true</c> if a media stream with the specified name was successfully found in the playlist and stopped;
        /// <c>false</c> otherwise.
        /// </returns>
        public bool TryStop(string name)
        {
            if (!TryGet(name, out var mediaStream) || mediaStream == null)
                return false;

            mediaStream.Stop();

            return true;
        }

        /// <summary>
        /// Starts all media streams in the playlist.
        /// </summary>
        public void StartAll()
        {
            foreach (var (mediaStream, actual) in _mediaStreams)
            {
                if (actual)
                    mediaStream.Start();
            }
        }

        /// <summary>
        /// Stops all media streams in the playlist.
        /// </summary>
        public void StopAll()
        {
            foreach (var (mediaStream, actual) in _mediaStreams)
            {
                if (actual)
                    mediaStream.Stop();
            }
        }

        /// <summary>
        /// Waits for all media streams to complete.
        /// </summary>
        public void WaitAll()
        {
            foreach (var (mediaStream, _) in _mediaStreams)
                mediaStream.WaitForFinished();
        }

        /// <summary>
        /// Finds and returns a media stream by its name in the playlist.
        /// </summary>
        /// <remarks>
        /// This method ignores media streams marked as removed.
        /// </remarks>
        /// <param name="name">Media stream name.</param>
        /// <param name="mediaStream">
        /// When this method returns, contains a default value if there is no media stream whose name is equal to the
        /// given <paramref name="name"/>; otherwise, the first element found.
        /// </param>
        /// <returns>
        /// <c>true</c> if a media stream with the specified name was successfully found in the playlist;
        /// <c>false</c> otherwise.
        /// </returns>
        private bool TryGet(string name, out IMediaStream? mediaStream)
        {
            if (_mediaStreams.TryFirstOrDefault(
                entry => entry.Value && entry.Key.Name.Equals(name, StringComparison.Ordinal), out var pair))
            {
                mediaStream = pair.Key;
                return true;
            }

            mediaStream = default;
            return false;
        }
    }
}
