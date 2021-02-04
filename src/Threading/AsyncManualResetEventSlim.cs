using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;

namespace Neuralia.Blockchains.Tools.Threading {
	public class AsyncManualResetEventSlim : IDisposable {
		private static readonly TimeSpan INFINITE_TIMEOUT_TIMESPAN = TimeSpan.FromMilliseconds(Timeout.Infinite);

		private volatile TaskCompletionSource<bool> tcs;
		private static readonly TaskRegistrationBuffer registrationBuffer = new TaskRegistrationBuffer();

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncManualResetEventSlim"/> class with an initial state of nonsignaled.
		/// </summary>
		public AsyncManualResetEventSlim() : this(false) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncManualResetEventSlim"/> class with a Boolean value indicating whether to set the initial state to signaled.
		/// </summary>
		/// <param name="initialState"></param>
		public AsyncManualResetEventSlim(bool initialState) {
			this.tcs = new TaskCompletionSource<bool>();

			if(initialState) {
				this.tcs.TrySetResult(true);
			}
		}

		/// <summary>
		/// Gets whether the event is set.
		/// </summary>
		/// <returns>true if the event is set; otherwise, false.</returns>
		public bool IsSet => this.tcs.Task.IsCompleted;

		/// <summary>
		/// Suspend the current thread execution until the current <see cref="AsyncManualResetEventSlim"/> is set.
		/// </summary>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/>(-1) to wait indefinitely.</param>
		/// <returns>A task with a bool result set to true if released by the signaled state, or false if otherwise.</returns>
		/// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
		public Task<bool> WaitAsync(int millisecondsTimeout) {
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
		public Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken cancellationToken) {
			return this.WaitAsync(TimeSpan.FromMilliseconds(millisecondsTimeout), cancellationToken);
		}

		/// <summary>
		/// Suspend the current thread execution until the current <see cref="AsyncManualResetEventSlim"/> is set.
		/// </summary>
		/// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
		/// <returns>A task with a bool result set to true if released by the signaled state, or false if otherwise.</returns>
		/// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
		public Task<bool> WaitAsync(TimeSpan timeout) {
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
		public async Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken) {
			this.ThrowIfDisposed();

			if(timeout == INFINITE_TIMEOUT_TIMESPAN) {
				return await this.WaitAsync(cancellationToken).ConfigureAwait(false);
			}

			using(var timeoutCts = new CancellationTokenSource()) //Do not create a linked CTS with the cancellationToken, they serve different purpose.
			{
				//Here we setup the task that will complete on timeout
				TaskCompletionSource<object> expirationTCS = new TaskCompletionSource<object>();

				//We register the timeout only if it's not infinity
				await using CancellationTokenRegistration ctRegistration = timeoutCts.Token.Register(() => expirationTCS.TrySetResult(false));

				if(ctRegistration != default(CancellationTokenRegistration)) {
					timeoutCts.CancelAfter(timeout);
				}

				Task<bool> waitSignalTask = this.WaitAsync(cancellationToken, timeoutCts.Token);

				using(Task timeoutTask = expirationTCS.Task) {
					try {
						//We wait until one task complete. WhenAny return at the first task completing.
						using Task<Task> allTasksTask = Task.WhenAny(waitSignalTask, timeoutTask);
						await allTasksTask.ConfigureAwait(false); //We wait for one task to complete

						//We first check if the signal has been received, since it could've completed at the same time of the timeout. 
						// Else, we return the timeout task.
						if(waitSignalTask.IsCompleted) //The task could be canceled or have an exception, so we must use IsCompleted and not IsCompletedSuccessfully
						{
							await waitSignalTask.ConfigureAwait(false); //We await in-case there's an exception.

							expirationTCS.SetResult(null);

							return waitSignalTask.IsCompletedSuccessfully && waitSignalTask.Result;
						} else {
							return false;
						}
					} finally {
						//We ensure the cancellableTasks is completed before disposing
						expirationTCS.TrySetResult(null);

						try {
							timeoutCts.Cancel();
						} catch {
							// we dont want to bubble up a a time expired task cancelled exception
						}
					}
				}
			}
		}
		
		public Task<bool> WaitAsync(CancellationToken cancellationToken) {
			return this.WaitAsync(cancellationToken, null);
		}
		
		/// <summary>
		/// Suspend the current thread execution until the current <see cref="AsyncManualResetEventSlim"/> is set.
		/// </summary>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
		/// <returns>A task that completes successfully only when this <see cref="AsyncManualResetEventSlim"/> is in the signaled state.</returns>
		/// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
		/// <exception cref="ObjectDisposedException">The object has already been disposed or the <see cref="CancellationTokenSource"/> that created <paramref name="cancellationToken"/> has been disposed.</exception>
		private async Task<bool> WaitAsync(CancellationToken cancellationToken, CancellationToken? cancellationToken2) {
			this.ThrowIfDisposed();

			TaskCompletionSource<object> cancellationTask = new TaskCompletionSource<object>();
			
			cancellationToken2?.Register(() => {
				cancellationTask.TrySetCanceled(cancellationToken);
			});
			
			try {
				await using CancellationTokenRegistration ctRegistration = cancellationToken.Register(() => {
					registrationBuffer.Add(cancellationToken, cancellationTask);

					if(cancellationToken2 != null) {
						registrationBuffer.Add(cancellationToken2.Value, cancellationTask);
					}
				});

				// ReSharper disable once MethodSupportsCancellation
				Task waitSignalTask = this.WaitAsync(); // we DON'T dispose that one since it is shared among all threads.

				using Task cancellableTask = cancellationTask.Task;

				try {
					using Task<Task> allTasksTask = Task.WhenAny(waitSignalTask, cancellableTask);
					Task completedTask = await allTasksTask.ConfigureAwait(false); //We don't dispose that one, it's one of our previous tasks.

					await completedTask.ConfigureAwait(false);

					return completedTask == waitSignalTask;
				} catch(TaskCanceledException tex) {
					// ignore timeout exceptions
					if(!cancellationToken2.HasValue || !cancellationToken2.Value.IsCancellationRequested) {
						throw;
					}
				} finally {
					//We ensure the cancellableTasks is completed before disposing
					cancellationTask.TrySetResult(null);
				}

			} finally {
				// just to be sure that CancellationTokenRegistration does its job, we double check here to clear registers.
				registrationBuffer.Clear(cancellationToken,cancellationTask);

				if(cancellationToken2 != null) {
					registrationBuffer.Clear(cancellationToken2.Value, cancellationTask);
				}
			}

			return false;
		}

		/// <summary>
		/// Suspend the current thread execution until the current <see cref="AsyncManualResetEventSlim"/> is set.
		/// </summary>
		/// <returns>A task that completes successfully only when this <see cref="AsyncManualResetEventSlim"/> is in the signaled state.</returns>
		/// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
		public Task WaitAsync() {
			this.ThrowIfDisposed();

			//The task will complete successfully only when set is called.
			return this.tcs.Task;
		}

		/// <summary>
		/// Sets the state of the event to signaled, which allows one or more threads awaiting on the event to resume their execution.
		/// </summary>
		public void Set() {
			this.Set(false); //We wait for the signal to be sent by default.
		}

		/// <summary>
		/// Sets the state of the event to signaled, which allows one or more threads awaiting on the event to resume their execution.
		/// </summary>
		/// <param name="fireAndForget">true to return immediately without waiting for the event to have his state set to signaled; false otherwise.</param>
		public void Set(bool fireAndForget) {
			// https://stackoverflow.com/questions/12693046/configuring-the-continuation-behaviour-of-a-taskcompletionsources-task
			// We set the result on another thread.
			var task = this.SetAsync();
			
			// This ensure that any continuation of the Task of TaskCompletionSource does not resume on the thread that called "Set".
			if(!fireAndForget) {
				// We wait for the task to complete, which does not include any continuation on this new thread.
				task.WaitAndUnwrapException();
			} 
		}

		public Task SetAsync() {
			return Task.Run(() => this.tcs.TrySetResult(true));
		}

		/// <summary>
		/// Sets the state of the event to nonsignaled, which causes threads to suspend their execution asynchronously.
		/// </summary>
		/// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
		private object resetLock = new object();

		public void Reset() {
			this.ThrowIfDisposed();

			//We make sure to only return if:
			// it's not in a signaled state
			// OR a new TaskCompletionSource has been inserted atomically.
			lock(this.resetLock) {
				if(this.tcs.Task.IsCompleted) {
					using var oldTask = this.tcs.Task;
					this.tcs = new TaskCompletionSource<bool>();
				}
			}
		}

		private void ThrowIfDisposed() {
			if(this.disposedValue) {
				throw new ObjectDisposedException(nameof(AsyncManualResetEventSlim));
			}
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="AsyncManualResetEventSlim"/>, and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if(!this.disposedValue) {
				if(disposing) {
					//We free the awaiting threads, but with a disposed exception.
					var task = Task.Run(() => this.tcs.TrySetException(new ObjectDisposedException(nameof(AsyncManualResetEventSlim))));

					task.WaitAndUnwrapException();

					task.Dispose();
					this.tcs.Task.Dispose();
				}

				this.disposedValue = true;
			}
		}

		private bool disposedValue = false;

		~AsyncManualResetEventSlim() {
			this.Dispose(disposing: false);
		}

		/// <summary>
		/// Releases all resources used by the current instance of the <see cref="AsyncManualResetEventSlim"/> class.
		/// </summary>
		public void Dispose() {
			this.Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		
		/// <summary>
		/// Use to prevent multi registers on a long running CancellationToken.
		/// </summary>
		/// <remarks>THis might not be needed, but it is used as a security policy to ensure there are no memory leaks</remarks>
		private class TaskRegistrationBuffer {
			private ConcurrentDictionary<CancellationToken, ConcurrentDictionary<TaskCompletionSource<object>, bool>> registers = new ConcurrentDictionary<CancellationToken, ConcurrentDictionary<TaskCompletionSource<object>, bool>>();

			public void Add(CancellationToken cancellationToken, TaskCompletionSource<object> cancellationTask) {
				var inner = this.registers.GetOrAdd(cancellationToken, new ConcurrentDictionary<TaskCompletionSource<object>, bool>());

				inner.TryAdd(cancellationTask, true);
			}

			public void Clear(CancellationToken cancellationToken, TaskCompletionSource<object> cancellationTask) {
				if(this.registers.TryRemove(cancellationToken, out var inner)) {

					if(inner.TryRemove(cancellationTask, out var _)) {
						try {
							cancellationTask.TrySetCanceled(cancellationToken);
						} catch {
							
						}
					}
				}
			}
		}
	}
}