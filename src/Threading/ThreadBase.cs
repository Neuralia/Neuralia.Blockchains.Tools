using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Neuralia.Blockchains.Tools.Extensions;
using Neuralia.Blockchains.Tools.Locking;

namespace Neuralia.Blockchains.Tools.Threading {
	public interface IThreadBase : IDisposableExtended {
		CancellationTokenSource CancelTokenSource { get; }
		CancellationToken CancelToken { get; }

		Task Task { get; }

		bool IsCompleted { get; }
		bool IsStarted { get; }

		Task<bool> CompletionTask { get; }

		void WaitStop(TimeSpan timeout);

		void RenewCancelNeuralium();

		Task Stop();

		Task Start();

		Task StartSync();
		Task StopSync();

		void Awaken();

		event Func<bool, object, Task> Completed;

		/// <summary>
		///     trigger when workflow ended in success
		/// </summary>
		event Func<object, Task> Success;

		/// <summary>
		///     trigger when workflow ended in error
		/// </summary>
		event Func<object, Exception, Task> Error;
	}

	public interface IThreadBase<out T> : IThreadBase
		where T : IThreadBase<T> {

		event Func<bool, T, Task> Completed2;

		/// <summary>
		///     trigger when workflow ended in success
		/// </summary>
		event Func<T, Task> Success2;

		/// <summary>
		///     trigger when workflow ended in error
		/// </summary>
		event Func<T, Exception, Task> Error2;
	}

	public abstract class ThreadBase<T> : IThreadBase<T>
		where T : class, IThreadBase<T> {

		protected readonly object locker = new object();

		/// <summary>
		///     how long the workflow will wait for something before timing out and giving up
		/// </summary>
		protected TimeSpan hibernateTimeoutSpan;

		/// <summary>
		///     We hold our own task
		/// </summary>
		private Task task;

		public ThreadBase() {
			// how long do we wait while hibernating until we declare this thread as dead?
			// by default we wait forever, children must override this and set their own value
			this.hibernateTimeoutSpan = TimeSpan.MaxValue;

			// wire up the events to make sure one calls the other
			this.Completed2 += async (a, b) => {

				if(this.Completed != null) {
					await this.Completed(a, b).ConfigureAwait(false);
				}
			};

			this.Success2 += async a => {
				if(this.Success != null) {
					await this.Success(a).ConfigureAwait(false);
				}
			};

			this.Error2 += async (a, b) => {
				if(this.Error != null) {
					await this.Error(a, b).ConfigureAwait(false);
				}
			};

		}

		protected List<AsyncManualResetEventSlim> AutoEvents { get; } = new List<AsyncManualResetEventSlim>();
		protected AsyncManualResetEventSlim AutoEvent { get; private set; }

		protected TaskCompletionSource<bool> TaskCompletionSource { get; private set; }

		protected bool Stopping { get; private set; }

		protected bool Initialized { get; private set; }

		protected virtual TaskCreationOptions TaskCreationOptions => TaskCreationOptions.LongRunning;

		public bool IsDisposed { get; private set; }

		/// <summary>
		///     called every time when completed, whether an error happened or not
		/// </summary>
		public event Func<bool, object, Task> Completed;

		public event Func<bool, T, Task> Completed2;

		/// <summary>
		///     trigger when workflow ended in success
		/// </summary>
		public event Func<object, Task> Success;

		public event Func<T, Task> Success2;

		/// <summary>
		///     trigger when workflow ended in error
		/// </summary>
		public event Func<object, Exception, Task> Error;

		public event Func<T, Exception, Task> Error2;

		/// <summary>
		///     The actual task. do
		/// </summary>
		public Task Task { get; private set; }

		/// <summary>
		///     the task to know the completion state of the task
		/// </summary>
		public Task<bool> CompletionTask => this.TaskCompletionSource.Task;

		public bool IsCompleted => this.Task?.IsCompleted ?? false;
		public bool IsStarted { get; private set; }

		public CancellationTokenSource CancelTokenSource { get; private set; }
		public CancellationToken CancelToken { get; private set; }

		public virtual async Task Stop() {
			this.Stopping = true;

			try {
				lock(this.locker) {
					if(!this.CancelTokenSource?.IsCancellationRequested ?? false) {
						this.CancelTokenSource?.Cancel();
					}
				}
			} catch {
				// we can let this die
			}

			if((this.task != null) && !this.task.IsCompleted) {
				try {
					// ReSharper disable once AsyncConverter.AsyncWait
					await this.task.HandleTimeout(TimeSpan.FromMilliseconds(6000)).ConfigureAwait(false);
				} catch(TaskCanceledException tce) {

				} catch(OperationCanceledException tce) {

				} catch(Exception ex) {

					void DefaultHandle(Exception exception) {
						Console.WriteLine(exception);
					}

					switch(ex) {
						case TaskCanceledException _:
						case OperationCanceledException _: return;
						case AggregateException aggregateException:
							aggregateException.Handle(ex2 => {

								if(ex2 is TaskCanceledException || ex2 is OperationCanceledException) {
									return true;
								}

								DefaultHandle(ex2);

								return true;
							});

							return;
						default:
							DefaultHandle(ex);

							break;
					}

				}
			}

			this.Stopping = false;
		}

		public virtual Task StopSync() {
			return this.Terminate(true, null);
		}

		/// <summary>
		///     wait for the thread to stop
		/// </summary>
		public void WaitStop(TimeSpan timeout) {
			if(this.Stopping && !this.IsCompleted) {
				// ReSharper disable once AsyncConverter.AsyncWait
				this.Task?.Wait(timeout);
			}
		}

		public void RenewCancelNeuralium() {
			this.CancelTokenSource?.Dispose();

			this.CancelTokenSource = new CancellationTokenSource();
			this.CancelToken = this.CancelTokenSource.Token;
		}

		/// <summary>
		///     Main method to start the workflow thread
		/// </summary>
		public virtual async Task Start() {
			this.IsStarted = true;
			this.InitializeTask();

			// we start the thread of the task in this case
			this.task = Task<Task>.Factory.StartNew(this.ThreadRun, this.CancelToken, this.TaskCreationOptions, TaskScheduler.Default).Unwrap();

			this.Task = this.task.ContinueWith(async previousTask => {
				// workflow is done, lets trigger a removal
				bool success = false;

				try {
					success = !(this.TaskCompletionSource.Task.IsFaulted || this.TaskCompletionSource.Task.IsCanceled);
				} catch {

				}

				try {
					await this.TriggerCompleted(success).ConfigureAwait(false);
				} catch {
					success = false;
				}

				if(success) {
					await this.TriggerSuccess().ConfigureAwait(false);
				} else {
					await this.TriggerError(this.TaskCompletionSource.Task.Exception?.Flatten()).ConfigureAwait(false);
				}
			}, this.CancelToken, TaskContinuationOptions.None, TaskScheduler.Default);
		}

		/// <summary>
		///     Prepare to run asynchronously
		/// </summary>
		public virtual Task StartSync() {
			return this.Initialize(null);
		}

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Awaken() {

			AsyncManualResetEventSlim[] resetEvents = null;

			lock(this.locker) {
				resetEvents = this.AutoEvents.ToArray();
			}

			foreach(AsyncManualResetEventSlim autoEvent in resetEvents) {
				autoEvent.Set();
			}
		}

		private void InitializeTask() {
			this.RenewCancelNeuralium();

			this.TaskCompletionSource = new TaskCompletionSource<bool>();

		}

		protected AsyncManualResetEventSlim RegisterNewAutoEvent() {
			lock(this.locker) {
				AsyncManualResetEventSlim autoEvent = new AsyncManualResetEventSlim(false);

				this.AutoEvents.Add(autoEvent);

				return autoEvent;
			}
		}

		protected void ClearAutoEvent(AsyncManualResetEventSlim autoEvent) {
			lock(this.locker) {
				autoEvent.Set();

				this.AutoEvents.Remove(autoEvent);
			}
		}

		protected Task<bool> Hibernate() {

			return this.Hibernate(this.hibernateTimeoutSpan);
		}

		protected Task<bool> Hibernate(TimeSpan? timeout) {
			return this.Hibernate(timeout, this.AutoEvent);
		}

		/// <summary>
		///     calling this method we go to sleep until we are awoken explicitely
		/// </summary>
		protected Task Hibernate(AsyncManualResetEventSlim autoEvent) {

			return this.Hibernate(this.hibernateTimeoutSpan, this.AutoEvent);
		}

		protected async Task<bool> Hibernate(TimeSpan? timeout, AsyncManualResetEventSlim autoEvent) {

			if(!timeout.HasValue) {
				timeout = this.hibernateTimeoutSpan;
			}

			if(autoEvent == null) {
				throw new ApplicationException("Network AutoEvent awaiter can not be null");
			}

			DateTime timeoutLimit = DateTimeEx.CurrentTime + timeout.Value;

			autoEvent.Reset();
			await autoEvent.WaitAsync(timeout.Value, this.CancelToken).ConfigureAwait(false);
			autoEvent.Reset();

			//TODO: is the precision of datetime high enough here?
			if(DateTimeEx.CurrentTime > timeoutLimit) {
				// we timed out, event was not set
				return false;
			}

			this.CheckShouldCancel();

			// event was set
			return true;
		}

		private async Task ThreadRun() {
			LockContext lockContext = null;
			Thread.CurrentThread.Name = this.GetType().Name;

			this.AutoEvent = this.RegisterNewAutoEvent();

			try {
				await this.Initialize(lockContext).ConfigureAwait(false);

				await this.PerformWork(lockContext).ConfigureAwait(false);

				this.TaskCompletionSource.SetResult(true);
			} catch(TaskCanceledException) {
#if NETSTANDARD2_1
				this.TaskCompletionSource.SetCanceled();
#else
				this.TaskCompletionSource.SetCanceled(this.CancelToken);
#endif
			} catch(OperationCanceledException) {
#if NETSTANDARD2_1
				this.TaskCompletionSource.SetCanceled();
#else
				this.TaskCompletionSource.SetCanceled(this.CancelToken);
#endif
			} catch(Exception ex) {
				this.TaskCompletionSource.SetException(ex);
			} finally {
				await this.Terminate(true, lockContext).ConfigureAwait(false);
			}
		}

		/// <summary>
		///     the method to override to perform the actual thread work
		/// </summary>
		/// <param name="lockContext"></param>
		/// <returns></returns>
		protected abstract Task PerformWork(LockContext lockContext);

		/// <summary>
		///     Check if a cancel was requested.abstract if it did, we will stop the thread with an exception
		/// </summary>
		protected void CheckShouldCancel() {
			// Poll on this property if you have to do
			// other cleanup before throwing.
			if(this.CheckCancelRequested()) {
				this.Terminate(false, null).GetAwaiter().GetResult();

				// Clean up here, then...
				//if(throwException) {
				this.CancelToken.ThrowIfCancellationRequested();

				/*} else {
					return new OperationCanceledException();
				}*/
			}
		}

		protected bool CheckCancelRequested() {
			return this.CancelToken.IsCancellationRequested || this.Stopping;
		}

		protected virtual Task Initialize(LockContext lockContext) {
			this.Initialized = true;

			return Task.CompletedTask;
		}

		/// <summary>
		///     terminate.
		/// </summary>
		/// <param name="clean">if true, the tread ends normally. if false, then it was a hard cancel</param>
		protected Task Terminate() {
			return this.Terminate(true, null);
		}

		protected virtual Task Terminate(bool clean, LockContext lockContext) {
			return Task.CompletedTask;
		}

		protected virtual async Task TriggerCompleted(bool success) {
			if(this.Completed2 != null) {
				await this.Completed2(success, this as T).ConfigureAwait(false);

			}
		}

		protected virtual async Task TriggerSuccess() {
			if(this.Success2 != null) {
				await this.Success2(this as T).ConfigureAwait(true);
			}
		}

		protected virtual async Task TriggerError(Exception ex) {
			if(this.Error2 != null) {
				await this.Error2(this as T, ex).ConfigureAwait(false);
			}
		}

		protected virtual void Dispose(bool disposing) {

			if(!this.IsDisposed && disposing) {

				this.DisposeAllAsync().GetAwaiter().GetResult();
			}

			this.IsDisposed = true;
		}

		protected virtual async Task DisposeAllAsync() {
			await this.Stop().ConfigureAwait(false);

			this.CancelTokenSource?.Dispose();

			if((this.Task == null) || (this.IsStarted == false)) {
				// we never ran this workflow. lets at least alert that it failed
				await this.TriggerCompleted(false).ConfigureAwait(false);
			}

			foreach(AsyncManualResetEventSlim entry in this.AutoEvents) {
				try {
					entry?.Dispose();
				} catch {

				}
			}

			this.AutoEvents.Clear();
		}

		~ThreadBase() {
			this.Dispose(false);
		}
	}
}