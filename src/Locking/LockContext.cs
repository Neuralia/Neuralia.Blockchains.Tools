using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Neuralia.Blockchains.Tools.Locking {
	public class LockContext : IDisposableExtended {

		private readonly ConcurrentDictionary<Guid, LockContextInstance> contexts = new ConcurrentDictionary<Guid, LockContextInstance>();
		private Guid Uuid { get; } = Guid.NewGuid();
		
		public LockContext() {

		}

		public LockContextInstance GetContext(Guid uuid) {
			this.contexts.TryGetValue(uuid, out var context);

			return context;
		}

		public void AddContext(LockContextInstance instanceContext) {
			this.contexts.TryAdd(instanceContext.Uuid, instanceContext);
		}

		public void Remove(LockContextInstance instanceContext) {
			this.contexts.TryRemove(instanceContext.Uuid, out LockContextInstance _);

			if(this.contexts.IsEmpty) {
				this.Dispose();
			}
		}

		public static Task<LockHandle> PrepareLock<HANDLE, CONTEXT, LOGICS_TYPE>(LockContext context, Guid uuid, TimeSpan timeout, Func<CancellationToken, Task<IDisposable>> setLock, Action<CONTEXT, LOGICS_TYPE> prepare = null, LOGICS_TYPE logicsState = default)
			where HANDLE : LockHandle, new()
			where CONTEXT : LockContextInstance, new() {

			LockContext copyContext = context;

			if(copyContext == null) {
				copyContext = new LockContext();
			}

			if(copyContext.IsDisposed) {
				throw new ObjectDisposedException(nameof(copyContext));
			}

			return LockContextInstance.PrepareLock<HANDLE, CONTEXT, LOGICS_TYPE>(copyContext, uuid, timeout, setLock, prepare, logicsState);
		}

	#region Dispose

		public bool IsDisposed { get; private set; }

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {

			if(disposing && !this.IsDisposed) {

				try {
					foreach(var entry in contexts.ToArray()) {
						entry.Value?.Dispose();
					}
				} catch(Exception ex) {
				} finally {
				}
			}

			this.IsDisposed = true;
		}

		~LockContext() {
			this.Dispose(false);
		}

	#endregion

	}
}