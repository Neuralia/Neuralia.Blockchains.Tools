using System;
using System.Threading;
using System.Threading.Tasks;

namespace Neuralia.Blockchains.Tools.Threading
{
    public class AsyncManualResetEventSlim : IDisposable
    {
        private static readonly TimeSpan INFINITE_TIMEOUT_TIMESPAN = TimeSpan.FromMilliseconds(Timeout.Infinite);

        private volatile TaskCompletionSource<bool> tcs;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncManualResetEventSlim"/> class with an initial state of nonsignaled.
        /// </summary>
        public AsyncManualResetEventSlim()
        {
            this.tcs = new TaskCompletionSource<bool>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncManualResetEventSlim"/> class with a Boolean value indicating whether to set the initial state to signaled.
        /// </summary>
        /// <param name="initialState"></param>
        public AsyncManualResetEventSlim(bool initialState) : this()
        {
            if (initialState)
            {
                this.tcs.TrySetResult(true);
            }
        }

        /// <summary>
        /// Gets whether the event is set.
        /// </summary>
        /// <returns>true if the event is set; otherwise, false.</returns>
        public bool IsSet { get => this.tcs.Task.IsCompleted; }

        /// <summary>
        /// Suspend the current thread execution until the current <see cref="AsyncManualResetEventSlim"/> is set.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/>(-1) to wait indefinitely.</param>
        /// <returns>A task with a bool result set to true if released by the signaled state, or false if otherwise.</returns>
        /// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
        public Task<bool> WaitAsync(int millisecondsTimeout)
        {
            return this.WaitAsync(millisecondsTimeout, CancellationToken.None);
        }

        /// <summary>
        /// Suspend the current thread execution until the current <see cref="AsyncManualResetEventSlim"/> is set.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/>(-1) to wait indefinitely.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task with a bool result set to true if released by the signaled state, or false if otherwise.</returns>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        /// <exception cref="ObjectDisposedException">The object has already been disposed or the <see cref="CancellationTokenSource"/> that created <paramref name="cancellationToken"/> has been disposed.</exception>
        public Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            return this.WaitAsync(TimeSpan.FromMilliseconds(millisecondsTimeout), cancellationToken);
        }

        /// <summary>
        /// Suspend the current thread execution until the current <see cref="AsyncManualResetEventSlim"/> is set.
        /// </summary>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
        /// <returns>A task with a bool result set to true if released by the signaled state, or false if otherwise.</returns>
        /// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
        public Task<bool> WaitAsync(TimeSpan timeout)
        {
            return this.WaitAsync(timeout, CancellationToken.None);
        }

        /// <summary>
        /// Suspend the current thread execution until the current <see cref="AsyncManualResetEventSlim"/> is set.
        /// </summary>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task with a bool result set to true if released by the signaled state, or false if otherwise.</returns>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        /// <exception cref="ObjectDisposedException">The object has already been disposed or the <see cref="CancellationTokenSource"/> that created <paramref name="cancellationToken"/> has been disposed.</exception>
        public async Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            this.throwIfDisposed();

            Task<bool> completedTask;
            using (var timeoutCts = new CancellationTokenSource()) //Do not create a linked CTS with the cancellationToken, they serve different purpose.
            {
                TaskCompletionSource<bool> expirationTask = new TaskCompletionSource<bool>();

                if (timeout != INFINITE_TIMEOUT_TIMESPAN)
                {
                    timeoutCts.Token.Register(() => expirationTask.TrySetResult(false));
                    timeoutCts.CancelAfter(timeout);
                }

                Func<Task<bool>> waitWrapper = async () =>
                {
                    await this.WaitAsync(cancellationToken).ConfigureAwait(false);
                    return true;
                };

                completedTask = await Task.WhenAny(waitWrapper(), expirationTask.Task).ConfigureAwait(false);
            }

            return await completedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Suspend the current thread execution until the current <see cref="AsyncManualResetEventSlim"/> is set.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes successfully only when this <see cref="AsyncManualResetEventSlim"/> is in the signaled state.</returns>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        /// <exception cref="ObjectDisposedException">The object has already been disposed or the <see cref="CancellationTokenSource"/> that created <paramref name="cancellationToken"/> has been disposed.</exception>
        public async Task WaitAsync(CancellationToken cancellationToken)
        {
            this.throwIfDisposed();

            TaskCompletionSource<object> cancellationTask = new TaskCompletionSource<object>();
            cancellationToken.Register(() => cancellationTask.TrySetCanceled(cancellationToken));

            Task completedTask = await Task.WhenAny(this.WaitAsync(), cancellationTask.Task).ConfigureAwait(false);
            await completedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Suspend the current thread execution until the current <see cref="AsyncManualResetEventSlim"/> is set.
        /// </summary>
        /// <returns>A task that completes successfully only when this <see cref="AsyncManualResetEventSlim"/> is in the signaled state.</returns>
        /// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
        public Task WaitAsync()
        {
            this.throwIfDisposed();

            //The task will complete successfully only when set is called.
            return this.tcs.Task;
        }

        /// <summary>
        /// Sets the state of the event to signaled, which allows one or more threads awaiting on the event to resume their execution.
        /// </summary>
        public void Set()
        {
            this.Set(false);
        }

        /// <summary>
        /// Sets the state of the event to signaled, which allows one or more threads awaiting on the event to resume their execution.
        /// </summary>
        /// <param name="fireAndForget">true to return immediately without waiting for the event to have his state set to signaled; false otherwise.</param>
        public void Set(bool fireAndForget)
        {
            // https://stackoverflow.com/questions/12693046/configuring-the-continuation-behaviour-of-a-taskcompletionsources-task
            // We set the result on another thread.
            // This ensure that any continuation of the Task of TaskCompletionSource does not resume on the thread that called "Set".
            // We then wait for the task to complete, which does not include any continuation on this new thread.
            var task = Task.Run(() => this.tcs.TrySetResult(true));

            if (!fireAndForget)
            {
                task.GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Sets the state of the event to nonsignaled, which causes threads to suspend their execution asynchronously.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
        public void Reset()
        {
            this.throwIfDisposed();

            //We make sure to only return if:
            // it's not in a signaled state
            // OR a new TaskCompletionSource has been inserted atomically.
            while (true)
            {
                var tcs = this.tcs;
                if (!tcs.Task.IsCompleted || Interlocked.CompareExchange(ref this.tcs, new TaskCompletionSource<bool>(), tcs) == tcs)
                {
                    return;
                }
            }
        }

        private void throwIfDisposed()
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(nameof(AsyncManualResetEventSlim));
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="AsyncManualResetEventSlim"/>, and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //We free the awaiting threads, but with a disposed exception.
                    var task = Task.Run(() => this.tcs.TrySetException(new ObjectDisposedException(nameof(AsyncManualResetEventSlim))));
                    task.GetAwaiter().GetResult();
                }

                disposedValue = true;
            }
        }
        private bool disposedValue = false;

        ~AsyncManualResetEventSlim()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AsyncManualResetEventSlim"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
