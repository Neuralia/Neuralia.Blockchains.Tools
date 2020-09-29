using System;
using System.Threading;
using System.Threading.Tasks;
using Neuralia.Blockchains.Tools.Data;

namespace Neuralia.Blockchains.Tools.Locking {
	public class LockContextInstance : IDisposableExtended {

		public static ObjectPool<LockContextInstance> ContextPool { get; } = new ObjectPool<LockContextInstance>(() => new LockContextInstance(), 0, 10);
		
		public Guid Uuid { get; private set; }
		public bool InLock { get; set; }
		private CancellationTokenSource TokenSource { get; set; }
		private TimeSpan Timeout { get; set; }
		private LockContext Parent { get; set; }

		private CancellationToken Token => this.TokenSource?.Token ?? default;
		private Action<LockContextInstance> returnToPool;
		
		public void Initialize(Guid uuid, LockContext parent,  Action<LockContextInstance> returnToPool, TimeSpan timeout) {
			this.returnToPool = returnToPool;
			this.Parent = parent;
			this.Uuid = uuid;
			this.Timeout = timeout;

			this.Parent.AddContext(this);

			if(this.Timeout != default) {
				this.TokenSource = new CancellationTokenSource();
			}
		}

		public void Initialize(Guid uuid, LockContext parent,  Action<LockContextInstance> returnToPool) {
			this.Initialize(uuid, parent, returnToPool, TimeSpan.FromSeconds(60));
		}

		private void StartTimer() {

			if(this.Timeout != default) {
				this.TokenSource?.CancelAfter(this.Timeout);
			}
		}

		private void StopTimer() {

			this.TokenSource?.Dispose();
		}

		public static async Task<LockHandle> PrepareLock<CONTEXT, LOGICS_TYPE>(LockContext context, bool inherited, Func<CONTEXT> getFromPool, Action<LockContextInstance> returnToPool, Guid uuid, TimeSpan timeout, Func<CancellationToken, Task<IDisposable>> setLock, Action<CONTEXT, LOGICS_TYPE> prepare = null, LOGICS_TYPE logicsState = default)
			where CONTEXT : LockContextInstance, new() {

			LockContextInstance copyContext = context.GetContext(uuid);

			if(copyContext == null) {
				copyContext = getFromPool();
				copyContext.Initialize(uuid, context, returnToPool, timeout);
			}

			if(copyContext.IsDisposed) {
				throw new ObjectDisposedException(nameof(copyContext));
			}

			if(copyContext is CONTEXT rwContext) {
				if(prepare != null) {
					prepare(rwContext, logicsState);
				}

				LockHandle handle = LockHandle.HandlePool.GetObject();
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

				handle.Initialize(locker, context, inherited, rwContext);

				return handle;
			}

			throw new InvalidCastException();
		}

		public void Reset() {
			this.returnToPool = null;
			this.Parent.Remove(this);
			this.Parent = null;
			this.Uuid = Guid.Empty;
			this.Timeout = TimeSpan.Zero;
			try {
				this.TokenSource?.Dispose();
			} catch(Exception ex) {
			}
			this.TokenSource = null;
		}
		
	#region Dispose

		public bool IsDisposed { get; private set; }

		public void Dispose() {
			var returnToPool = this.returnToPool;
			this.Reset();
			returnToPool(this);
		}

		private void Dispose(bool disposing) {

			if(disposing && !this.IsDisposed) {

				this.Reset();
			}

			this.IsDisposed = true;
		}

		~LockContextInstance() {
			this.Dispose(false);
		}

	#endregion

	}
}