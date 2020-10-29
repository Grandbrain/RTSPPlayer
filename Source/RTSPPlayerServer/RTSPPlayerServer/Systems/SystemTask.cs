using System;
using System.Threading.Tasks;

namespace RTSPPlayerServer.Systems
{
    /// <summary>
    /// A class that provides a system task wrapper implementation.
    /// </summary>
    public abstract class SystemTask
    {
        /// <summary>
        /// System task.
        /// </summary>
        private Task _systemTask = Task.CompletedTask;

        /// <summary>
        /// Task completion source.
        /// </summary>
        private TaskCompletionSource<bool>? _taskCompletionSource;

        /// <summary>
        /// System task.
        /// </summary>
        public Task Task => _systemTask;

        /// <summary>
        /// Starts the system task.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The system task is not finished yet.
        /// </exception>
        protected void Start()
        {
            if (!_systemTask.IsCompleted)
                throw new InvalidOperationException();

            _taskCompletionSource = new TaskCompletionSource<bool>();
            var task = _taskCompletionSource.Task;

            _systemTask = _systemTask
                .ContinueWith(_ => OnTaskStatusChanged(task), TaskContinuationOptions.ExecuteSynchronously)
                .ContinueWith(_ => task, TaskContinuationOptions.None).Unwrap()
                .ContinueWith(_ => OnTaskStatusChanged(task), TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Stops the system task.
        /// </summary>
        protected void Stop()
        {
            try
            {
                _taskCompletionSource?.TrySetCanceled();
            }
            catch (ObjectDisposedException)
            {

            }
        }

        /// <summary>
        /// Stops the system task with an error.
        /// </summary>
        /// <param name="message">Error message.</param>
        protected void StopWithError(string message)
        {
            try
            {
                _taskCompletionSource?.TrySetException(new Exception(message));
            }
            catch (ObjectDisposedException)
            {

            }
        }

        /// <summary>
        /// Waits until the system task finishes work.
        /// </summary>
        protected void WaitForFinished()
        {
            try
            {
                _systemTask.Wait();
            }
            catch (ObjectDisposedException)
            {

            }
            catch (AggregateException)
            {

            }
        }

        /// <summary>
        /// Receives the updated status of the system task.
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
