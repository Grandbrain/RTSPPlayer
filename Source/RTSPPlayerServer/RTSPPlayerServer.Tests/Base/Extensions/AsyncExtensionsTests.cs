using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RTSPPlayerServer.Base.Extensions;

namespace RTSPPlayerServer.Tests.Base.Extensions
{
    /// <summary>
    /// A class that contains tests for <see cref="AsyncExtensions"/> class.
    /// </summary>
    [TestFixture]
    public class AsyncExtensionsTests
    {
        /// <summary>
        /// Tests the <see cref="AsyncExtensions.WithCancellation"/> method with task cancellation.
        /// </summary>
        /// <param name="time">Wait time in seconds.</param>
        [TestCase(1D)]
        public void TestWithCancellation(double time)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var canceledTask = Task.WhenAll(
                TestTask(time).WithCancellation(cancellationToken),
                TestTask<bool>(time).WithCancellation(cancellationToken));

            cancellationTokenSource.Cancel();

            Assert.ThrowsAsync<TaskCanceledException>(async () => await canceledTask);
            Assert.That(canceledTask.IsCanceled);
        }

        /// <summary>
        /// Tests the <see cref="AsyncExtensions.WithCancellation"/> method without task cancellation.
        /// </summary>
        /// <param name="time">Wait time in seconds.</param>
        [TestCase(1D)]
        public void TestWithoutCancellation(double time)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var completedTask = Task.WhenAll(
                TestTask(time).WithCancellation(cancellationToken),
                TestTask<bool>(time).WithCancellation(cancellationToken));

            Assert.DoesNotThrowAsync(async () => await completedTask);
            Assert.That(completedTask.IsCompletedSuccessfully);
        }

        /// <summary>
        /// Asynchronously waits for the specified time.
        /// </summary>
        /// <param name="time">Wait time in seconds.</param>
        /// <returns>Asynchronous task.</returns>
        private static async Task TestTask(double time)
        {
            await Task.Delay(TimeSpan.FromSeconds(time));
        }

        /// <summary>
        /// Asynchronously waits for the specified time and returns the default value.
        /// </summary>
        /// <param name="time">Wait time in seconds.</param>
        /// <typeparam name="TResult">The type of the value returned by the task.</typeparam>
        /// <returns>Asynchronous task.</returns>
        private static async Task<TResult> TestTask<TResult>(double time)
        {
            await Task.Delay(TimeSpan.FromSeconds(time));
            return default!;
        }
    }
}
