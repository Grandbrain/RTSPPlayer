using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RTSPPlayerServer.Serializers.Network;
using RTSPPlayerServer.Utilities.Extensions;

namespace RTSPPlayerServer.Streams.Network
{
    /// <summary>
    /// A class that provides network frame stream implementation.
    /// </summary>
    internal class NetworkStream : INetworkStream
    {
	    /// <summary>
		/// A thread-safe collection for storing network frames to be sent.
		/// </summary>
		private readonly BlockingCollection<Tuple<NetworkFrame, EndPoint>> _blockingCollection =
			new BlockingCollection<Tuple<NetworkFrame, EndPoint>>();
	    
	    /// <summary>
	    /// Network serializer.
	    /// </summary>
	    private readonly INetworkSerializer _networkSerializer;
	    
	    /// <summary>
	    /// Work task.
	    /// </summary>
	    private Task _task = Task.CompletedTask;
		
		/// <summary>
		/// Cancellation token source.
		/// </summary>
		private CancellationTokenSource _cancellationTokenSource;

		/// <summary>
		/// Indicates whether the network stream is active.
		/// </summary>
		public bool IsActive => !_cancellationTokenSource?.IsCancellationRequested ?? false;
		
		/// <summary>
		/// Indicates whether the network stream is healthy.
		/// </summary>
		public bool IsHealthy { get; private set; } = true;

		/// <summary>
		/// Constructs a network stream with the specified network serializer.
		/// </summary>
		/// <param name="networkSerializer">Network serializer.</param>
		public NetworkStream(INetworkSerializer networkSerializer)
		{
			_networkSerializer = networkSerializer;
		}

		/// <summary>
		/// Starts the network stream.
		/// </summary>
		public void Start()
		{
			if (IsActive) return;

			IsHealthy = true;
			
			_cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = _cancellationTokenSource.Token;

			_task = _task.ContinueWith(_ => SendAsync(cancellationToken), cancellationToken).Unwrap();
		}

		/// <summary>
		/// Stops the network stream.
		/// </summary>
		public void Stop()
		{
			try
			{
				_cancellationTokenSource?.Cancel();
				_cancellationTokenSource?.Dispose();
			}
			catch (ObjectDisposedException)
			{
                
			}
		}

		/// <summary>
		/// Waits until the network stream finishes work.
		/// </summary>
		public void Wait()
		{
			try
			{
				_task?.Wait(_cancellationTokenSource?.Token ?? new CancellationToken(true));
			}
			catch (OperationCanceledException)
			{

			}
			catch (ObjectDisposedException)
			{

			}
		}

		/// <summary>
		/// Sends a network frame to the specified endpoint.
		/// </summary>
		/// <param name="networkFrame">Network frame.</param>
		/// <param name="endPoint">End point.</param>
		public bool TrySend(NetworkFrame networkFrame, EndPoint endPoint)
		{
			if (!_networkSerializer.ValidateFrame(networkFrame) || !(endPoint is IPEndPoint)) 
				return false;
			
			_blockingCollection.Add(new Tuple<NetworkFrame, EndPoint>(networkFrame, endPoint));

			return true;
		}

		/// <summary>
		/// Asynchronously sends network frames.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Asynchronous task.</returns>
		private async Task SendAsync(CancellationToken cancellationToken)
		{
			try
			{
				using var udpClient = new UdpClient();

				while (true)
				{
					var (networkFrame, endPoint) = _blockingCollection.Take(cancellationToken);
					var packets = _networkSerializer.Serialize(networkFrame);

					foreach (var packet in packets)
					{
						await udpClient.SendAsync(packet, packet.Length, endPoint as IPEndPoint)
							.WithCancellation(cancellationToken);
					}
				}
			}
			catch (OperationCanceledException)
			{
				
			}
			catch (Exception)
			{
				IsHealthy = false;
				Stop();
			}
		}
    }
}
