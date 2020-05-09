using System;
using System.Threading;
using System.Threading.Tasks;

namespace Neuralia.Blockchains.Tools.Locking {
	public class LockContextInstance : IDisposableExtended {

		public Guid Uuid { get; private set; }
		public bool InLock { get; set; }
		private CancellationTokenSource TokenSource { get; set; }
		private TimeSpan Timeout { get; set; }
		private LockContext Parent { get; set; }

		private CancellationToken Token => this.TokenSource?.Token ?? default;

		public void Initialize(Guid uuid, LockContext parent, TimeSpan timeout) {
			this.Parent = parent;
			this.Uuid = uuid;
			this.Timeout = timeout;

			this.Parent.AddContext(this);

			if(this.Timeout != default) {
				this.TokenSource = new CancellationTokenSource();
			}
		}

		public void Initialize(Guid uuid, LockContext parent) {
			this.Initialize(uuid, parent, TimeSpan.FromSeconds(60));
		}

		private void StartTimer() {

			if(this.Timeout != default) {
				this.TokenSource?.CancelAfter(this.Timeout);
			}
		}

		private void StopTimer() {

			this.TokenSource?.Dispose();
		}

		public static async Task<LockHandle> PrepareLock<HANDLE, CONTEXT, LOGICS_TYPE>(LockContext context, Guid uuid, TimeSpan timeout, Func<CancellationToken, Task<IDisposable>> setLock, Action<CONTEXT, LOGICS_TYPE> prepare = null, LOGICS_TYPE logicsState = default)
			where HANDLE : LockHandle, new()
			where CONTEXT : LockContextInstance, new() {

			LockContextInstance copyContext = context.GetContext(uuid);

			if(copyContext == null) {
				copyContext = new CONTEXT();
				copyContext.Initialize(uuid, context, timeout);
			}

			if(copyContext.IsDisposed) {
				throw new ObjectDisposedException(nameof(copyContext));
			}

			if(copyContext is CONTEXT rwContext) {
				if(prepare != null) {
					prepare(rwContext, logicsState);
				}

				HANDLE handle = new HANDLE();
				IDisposable locker = null;

				if(!rwContext.InLock) {
					rwContext.StartTimer();

					try {
						locker = await setLock(rwContext.Token).ConfigureAwait(false);
					} catch(TaskCanceledException tcex) {
						throw new LockTimeoutException($"Failed to acquire lock. Timed out after {rwContext.Timeout}");
					}

					rwContext.StopTimer();
				}

				handle.Initialize(locker, context, rwContext);

				return handle;
			}

			throw new InvalidCastException();
		}

	#region Dispose

		public bool IsDisposed { get; private set; }

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {

			if(disposing && !this.IsDisposed) {

				this.Parent.Remove(this);

				try {
					this.TokenSource?.Dispose();
				} catch(Exception ex) {
				}
			}

			this.IsDisposed = true;
		}

		~LockContextInstance() {
			this.Dispose(false);
		}

	#endregion

	}
}