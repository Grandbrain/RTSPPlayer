using System.Threading;
using System.Threading.Tasks;

namespace RTSPPlayerServer.Utilities.Extensions
{
    internal static class TaskExtensions
    {
        /// <summary>
        /// Wraps a task with one that will complete as cancelled based on a cancellation token, allowing someone to
        /// await a task but be able to break out early by cancelling the token.
        /// </summary>
        /// <param name="task">The task to wrap.</param>
        /// <param name="token">The token that can be canceled to break out of the await.</param>
        /// <returns>The wrapping task.</returns>
        public static Task WithCancellation(this Task task, CancellationToken token)
        {
            return task.ContinueWith(t => t.GetAwaiter().GetResult(), token);
        }
        
        /// <summary>
        /// Wraps a task with one that will complete as cancelled based on a cancellation token, allowing someone to
        /// await a task but be able to break out early by cancelling the token.
        /// </summary>
        /// <param name="task">The task to wrap.</param>
        /// <param name="token">The token that can be canceled to break out of the await.</param>
        /// <typeparam name="T">The type of value returned by the task.</typeparam>
        /// <returns>The wrapping task.</returns>
        public static Task<T> WithCancellation<T>(this Task<T> task, CancellationToken token)
        {
            return task.ContinueWith(t => t.GetAwaiter().GetResult(), token);
        }
    }
}
