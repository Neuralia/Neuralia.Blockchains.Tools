using System;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;

namespace Neuralia.Blockchains.Tools.Locking {
	public class RecursiveAsyncLock : RecursiveAsyncLockBase {
		private readonly AsyncLock locker = new AsyncLock();

		public Task<LockHandle> LockAsync(LockContext context = null, TimeSpan timeout = default) {

			return LockContext.PrepareLock<LockHandle, LockContextInstance, bool>(context, this.Uuid, timeout, (t) => this.locker.LockAsync(t));
		}

		public LockHandle Lock(LockContext context = null, TimeSpan timeout = default) {
			return LockContext.PrepareLock<LockHandle, LockContextInstance, bool>(context, this.Uuid, timeout, (t) => Task.FromResult(this.locker.Lock(t))).WaitAndUnwrapException();
		}

	}
}