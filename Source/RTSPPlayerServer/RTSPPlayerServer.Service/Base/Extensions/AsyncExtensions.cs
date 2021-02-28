using System;
using System.Threading;
using System.Threading.Tasks;

namespace RTSPPlayerServer.Service.Base.Extensions
{
    /// <summary>
    /// An utility class that provides various extension methods for asynchronous primitives.
    /// </summary>
    public static class AsyncExtensions
    {
        /// <summary>
        /// Wraps a task with one that will complete as cancelled based on a cancellation token, allowing to await
        /// a task but be able to break out early by cancelling the token.
        /// </summary>
        /// <param name="task">The <see cref="Task"/> to wrap.</param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> that can be canceled to break out of the await.
        /// </param>
        /// <returns>A wrapped <see cref="Task"/>.</returns>
        /// <exception cref="ObjectDisposedException">
        /// The <see cref="Task"/> has been disposed or the <see cref="CancellationTokenSource"/> that created
        /// the <paramref name="cancellationToken"/> has already been disposed.
        /// </exception>
        public static Task WithCancellation(this Task task, CancellationToken cancellationToken)
        {
            return task.ContinueWith(t => t.GetAwaiter().GetResult(), cancellationToken);
        }

        /// <summary>
        /// Wraps a task with one that will complete as cancelled based on a cancellation token, allowing to await
        /// a task but be able to break out early by cancelling the token.
        /// </summary>
        /// <param name="task">The <see cref="Task{TResult}"/> to wrap.</param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> that can be canceled to break out of the await.
        /// </param>
        /// <typeparam name="TResult">The type of the value returned by the task.</typeparam>
        /// <returns>A wrapped <see cref="Task{TResult}"/>.</returns>
        /// <exception cref="ObjectDisposedException">
        /// The <see cref="Task{TResult}"/> has been disposed or the <see cref="CancellationTokenSource"/> that created
        /// the <paramref name="cancellationToken"/> has already been disposed.
        /// </exception>
        public static Task<TResult> WithCancellation<TResult>(this Task<TResult> task,
            CancellationToken cancellationToken)
        {
            return task.ContinueWith(t => t.GetAwaiter().GetResult(), cancellationToken);
        }
    }
}
