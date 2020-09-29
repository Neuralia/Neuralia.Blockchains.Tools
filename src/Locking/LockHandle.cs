using System;
using Neuralia.Blockchains.Tools.Data;

namespace Neuralia.Blockchains.Tools.Locking {
	public class LockHandle : IDisposableExtended {

		public static ObjectPool<LockHandle> HandlePool { get; } = new ObjectPool<LockHandle>(() => new LockHandle(), 0, 10);
		
		private bool OriginalLock { get; set; }
		public LockContext Context { get; private set; }
		public LockContextInstance InnerContext { get; private set; }
		public IDisposable Handle { get; private set; }

		private bool inherited;
		
		public void Initialize(IDisposable handle, LockContext context, bool inherited, LockContextInstance innerContext) {
			this.inherited = inherited;
			this.Handle = handle;
			this.Context = context;
			this.InnerContext = innerContext;

			if((innerContext != null) && !innerContext.InLock) {
				this.OriginalLock = true;
			}

			innerContext.InLock = true;
		}

		public void Reset() {

			try {
				this.Handle?.Dispose();
			} catch(Exception ex) {
				//TODO: what to do?
				//NLog.Default.Error(ex, "failed to dispose of lock handle");
			} finally {
				if(this.OriginalLock) {
					this.InnerContext.InLock = false;
					this.InnerContext?.Dispose();

					if(!this.inherited && this.Context.Empty) {
						// seems we have the responsibility to clear this
						this.Context?.Dispose();
						this.Context = null;
					}
				}
			}
			
			this.inherited = false;
			this.Handle = null;
			this.Context = null;
			this.InnerContext = null;
			this.OriginalLock = false;
		}

		public static implicit operator LockContext(LockHandle handle) {
			return handle.Context;
		}

	#region Dispose

		public bool IsDisposed { get; private set; }

		public void Dispose() {
			this.Reset();
			HandlePool.PutObject(this);
		}

		private void Dispose(bool disposing) {

			if(disposing && !this.IsDisposed) {

				this.Reset();
			}

			this.IsDisposed = true;
		}

		~LockHandle() {
			this.Dispose(false);
		}

	#endregion

	}
}