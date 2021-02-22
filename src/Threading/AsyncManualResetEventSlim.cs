using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;

// ReSharper disable MethodSupportsCancellation

namespace Neuralia.Blockchains.Tools.Threading {
	public class AsyncManualResetEventSlim : IDisposable {

		private volatile TaskCompletionSource<object> tcs;
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
			this.tcs = new TaskCompletionSource<object>();

			if(initialState) {
				this.tcs.TrySetResult(null);
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

			if(this.IsSet) {
				return true;
			}

			using var timeoutCts = new CancellationTokenSource(); //Do not create a linked CTS with the cancellationToken, they serve different purpose.
		
			//We register the timeout only if it's not infinity
			registrationBuffer.Add(cancellationToken, timeoutCts);

			if(timeout == TimeSpan.MaxValue) {
				timeout = TimeSpan.FromMilliseconds(-1);
			}
			var timeoutTask = Task.Delay(timeout, timeoutCts.Token);
			Task waitSignalTask = this.WaitAsync(); // we DON'T dispose that one since it is shared among all threads.

			//We wait until one task complete. WhenAny return at the first task completing.
			Task completedTask = await Task.WhenAny(waitSignalTask, timeoutTask).ConfigureAwait(false); //We wait for one task to complete

			//We first check if the signal has been received, since it could've completed at the same time of the timeout. 
			// Else, we return the timeout task.
			try {
				if(completedTask == waitSignalTask) {
					if(completedTask.IsFaulted && completedTask.Exception != null && completedTask.Exception.InnerException != null) {
						throw completedTask.Exception.InnerException;
					}

					return waitSignalTask.IsCompletedSuccessfully;
				}

				return false;
			} finally {
				registrationBuffer.Clear(cancellationToken, timeoutCts, timeoutTask);
			}
		}
		
		/// <summary>
		/// Suspend the current thread execution until the current <see cref="AsyncManualResetEventSlim"/> is set.
		/// </summary>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
		/// <returns>A task that completes successfully only when this <see cref="AsyncManualResetEventSlim"/> is in the signaled state.</returns>
		/// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
		/// <exception cref="ObjectDisposedException">The object has already been disposed or the <see cref="CancellationTokenSource"/> that created <paramref name="cancellationToken"/> has been disposed.</exception>
		public Task<bool> WaitAsync(CancellationToken cancellationToken) {
			return WaitAsync(TimeSpan.MaxValue, cancellationToken);
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
			var task = this.SetAsync();

			if(!fireAndForget) {
				// We wait for the task to complete, which does not include any continuation on this new thread.
				task.WaitAndUnwrapException();
			}
		}

		public Task SetAsync() {
			// https://stackoverflow.com/questions/12693046/configuring-the-continuation-behaviour-of-a-taskcompletionsources-task
			// We set the result on another thread.
			// This ensure that any continuation of the Task of TaskCompletionSource does not resume on the thread that called "SetAsync".
			return Task.Run(() => this.tcs.TrySetResult(null));
		}

		/// <summary>
		/// Sets the state of the event to nonsignaled, which causes threads to suspend their execution asynchronously.
		/// </summary>
		/// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
		public void Reset() {
			this.ThrowIfDisposed();

			//We make sure to only return if:
			// it's not in a signaled state
			// OR a new TaskCompletionSource has been inserted atomically.
			if(!this.IsSet) {
				return;
			}

			lock(this.resetLock) {
				if(this.IsSet) {
					using var oldTask = this.tcs.Task;
					this.tcs = new TaskCompletionSource<object>();
				}
			}
		}

		private object resetLock = new object();

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
					if(!this.tcs.Task.IsCompleted) {
						var task = Task.Run(() => {
							this.tcs.TrySetResult(false);
							this.tcs.Task.Dispose();
						});
					}
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
			private readonly ConcurrentDictionary<CancellationToken, ConcurrentDictionary<CancellationTokenSource, bool>> registers = new ConcurrentDictionary<CancellationToken, ConcurrentDictionary<CancellationTokenSource, bool>>();

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Add(CancellationToken cancellationToken, CancellationTokenSource timeoutCts) {

				if(this.registers.TryAdd(cancellationToken, new ConcurrentDictionary<CancellationTokenSource, bool>())) {
					cancellationToken.Register(() => {
						try {
							registrationBuffer.Clear(cancellationToken);
						} catch {}
					}, false);
				}
				var inner = this.registers[cancellationToken];
				
				inner.TryAdd(timeoutCts, true);
			}

			public void Clear(CancellationToken cancellationToken) {

				if(this.registers.TryRemove(cancellationToken, out var inner)) {
					foreach(var timeoutCts in inner.Keys) {
						try {
							if(!timeoutCts.IsCancellationRequested) {
								timeoutCts.Cancel();
							}
						} catch {
						
						}
					}
				}
			}

			public void Clear(CancellationToken cancellationToken, CancellationTokenSource timeoutCts, Task timeoutTask) {
				
				if(this.registers.TryGetValue(cancellationToken, out var inner)) {
					if(inner.TryRemove(timeoutCts, out var _)) {
						try {
							if(!timeoutTask.IsCompleted && !timeoutCts.IsCancellationRequested) {
								timeoutCts.Cancel();
							}
						} catch { }
					}
				}
			}
		}
	}
}