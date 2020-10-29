using System;
using System.Threading;
using System.Threading.Tasks;

namespace RTSPPlayerServer.Streams
{
    /// <summary>
    /// A class that provides a stream task wrapper implementation.
    /// </summary>
    public abstract class StreamTask
    {
        /// <summary>
        /// Stream task.
        /// </summary>
        private Task _streamTask = Task.CompletedTask;

        /// <summary>
        /// Cancellation token source.
        /// </summary>
        private CancellationTokenSource? _cancellationTokenSource;

        /// <summary>
        /// Stream task.
        /// </summary>
        public Task Task => _streamTask;

        /// <summary>
        /// Starts the stream task.
        /// </summary>
        /// <param name="task">Asynchronous task.</param>
        /// <param name="cancellationTokenSource">Cancellation token source.</param>
        /// <exception cref="InvalidOperationException">
        /// The stream task is not finished yet.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The token has had cancellation requested.
        /// </exception>
        protected void Start(Task task, CancellationTokenSource cancellationTokenSource)
        {
            if (!_streamTask.IsCompleted)
                throw new InvalidOperationException();

            cancellationTokenSource.Token.ThrowIfCancellationRequested();

            _cancellationTokenSource = cancellationTokenSource;

            _streamTask = _streamTask
                .ContinueWith(_ => OnTaskStatusChanged(task), TaskContinuationOptions.ExecuteSynchronously)
                .ContinueWith(_ => task, TaskContinuationOptions.None).Unwrap()
                .ContinueWith(_ => OnTaskStatusChanged(task), TaskContinuationOptions.ExecuteSynchronously)
                .ContinueWith(_ => Stop(), TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Stops the stream task.
        /// </summary>
        protected void Stop()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
            catch (ObjectDisposedException)
            {

            }
            catch (AggregateException)
            {

            }
        }

        /// <summary>
        /// Waits until the stream task finishes work.
        /// </summary>
        protected void WaitForFinished()
        {
            try
            {
                _streamTask.Wait();
            }
            catch (ObjectDisposedException)
            {

            }
            catch (AggregateException)
            {

            }
        }

        /// <summary>
        /// Receives the updated status of the stream task.
        /// </summary>
        /// <param name="status">Task status.</param>
        /// <param name="message">Error message.</param>
        protected abstract void OnStatusChanged(TaskStatus status, string? message);

        /// <summary>
        /// Performs the appropriate action when the status of the task changes.
        /// </summary>
        /// <param name="task">Asynchronous task.</param>
        private void OnTaskStatusChanged(Task task)
        {
            task = task.IsCompleted && task is Task<Task> future ? future.Result : task;

            OnStatusChanged(task.Status, task.Exception?.Message);
        }
    }
}
