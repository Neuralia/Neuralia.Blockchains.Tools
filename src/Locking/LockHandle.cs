using System;

namespace Neuralia.Blockchains.Tools.Locking {
	public class LockHandle : IDisposableExtended {

		private Guid Uuid { get; } = Guid.NewGuid();
		private bool OriginalLock { get; set; }
		public LockContext Context { get; private set; }
		public LockContextInstance InnerContext { get; private set; }
		public IDisposable Handle { get; private set; }

		public void Initialize(IDisposable handle, LockContext context, LockContextInstance innerContext) {
			this.Handle = handle;
			this.Context = context;
			this.InnerContext = innerContext;

			if((innerContext != null) && !innerContext.InLock) {
				this.OriginalLock = true;
			}

			innerContext.InLock = true;
		}

		public static implicit operator LockContext(LockHandle handle) {
			return handle.Context;
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
					this.Handle?.Dispose();
				} catch(Exception ex) {
					//TODO: what to do?
					//NLog.Default.Error(ex, "failed to dispose of lock handle");
				} finally {
					if(this.OriginalLock) {
						this.InnerContext.InLock = false;
						this.InnerContext?.Dispose();
					}
				}
			}

			this.IsDisposed = true;
		}

		~LockHandle() {
			this.Dispose(false);
		}

	#endregion

	}
}