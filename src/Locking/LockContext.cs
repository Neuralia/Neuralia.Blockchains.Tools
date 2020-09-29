using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Neuralia.Blockchains.Tools.Data;
	
namespace Neuralia.Blockchains.Tools.Locking {
	public class LockContext : IDisposableExtended {

		public static ObjectPool<LockContext> ContextPool { get; } = new ObjectPool<LockContext>(() => new LockContext(), 0, 10);

		public static LockContext GetNewContext() {
			return ContextPool.GetObject();
		}
		
		private readonly ConcurrentDictionary<Guid, LockContextInstance> contexts = new ConcurrentDictionary<Guid, LockContextInstance>();

		public bool Empty => !this.contexts.Any();
		
		private LockContext() {
		}
		
		public LockContextInstance GetContext(Guid uuid) {
			this.contexts.TryGetValue(uuid, out LockContextInstance context);

			return context;
		}

		public void AddContext(LockContextInstance instanceContext) {
			if(instanceContext != null) {
				this.contexts.TryAdd(instanceContext.Uuid, instanceContext);
			}
		}

		public void Remove(LockContextInstance instanceContext) {
			if(instanceContext != null) {
				this.contexts.TryRemove(instanceContext.Uuid, out LockContextInstance instance);
			}
		}

		public static Task<LockHandle> PrepareLock<CONTEXT, LOGICS_TYPE>(LockContext context, Func<CONTEXT> getFromPool, Action<LockContextInstance> returnToPool, Guid uuid, TimeSpan timeout, Func<CancellationToken, Task<IDisposable>> setLock, Action<CONTEXT, LOGICS_TYPE> prepare = null, LOGICS_TYPE logicsState = default)
			where CONTEXT : LockContextInstance, new() {

			LockContext copyContext = context;

			bool inherited = true;
			
			if(copyContext == null) {
				copyContext = GetNewContext();
				inherited = false;
			}

			if(copyContext.IsDisposed) {
				throw new ObjectDisposedException(nameof(copyContext));
			}

			return LockContextInstance.PrepareLock(copyContext, inherited, getFromPool, returnToPool, uuid, timeout, setLock, prepare, logicsState);
		}
		
		public void Reset() {
			try {
				foreach(KeyValuePair<Guid, LockContextInstance> entry in this.contexts.ToArray()) {
					entry.Value?.Dispose();
				}
			} catch(Exception ex) {
			}
		}

	#region Dispose

		public bool IsDisposed { get; private set; }

		public void Dispose() {
			this.Reset();
			ContextPool.PutObject(this);
		}

		private void Dispose(bool disposing) {

			if(disposing && !this.IsDisposed) {

				this.Reset();
			}

			this.IsDisposed = true;
		}

		~LockContext() {
			this.Dispose(false);
		}

	#endregion

	}
}