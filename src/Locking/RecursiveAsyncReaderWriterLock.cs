using System;
using System.Threading.Tasks;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Data.Pools;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;

namespace Neuralia.Blockchains.Tools.Locking {
	public class RecursiveAsyncReaderWriterLock : RecursiveAsyncLockBase {
		private readonly AsyncReaderWriterLock asyncLocker = new AsyncReaderWriterLock();

		private void CheckLocksLogics(ReadWriteLockContext context, ReadWriteLockContext.Modes mode) {

			if(context.InLock) {
				// check logics
				if((context.Mode == ReadWriteLockContext.Modes.Read) && (mode == ReadWriteLockContext.Modes.Write)) {
					throw new ApplicationException("Attempted to upgrade a read lock into a write lock.");
				}

				return;
			}

			context.Mode = mode;
		}

		public LockHandle ReaderLock(LockContext context = null, TimeSpan timeout = default) {

			return LockContext.PrepareLock(context, () => ReadWriteLockContext.ReadWriteContextPool.GetObject(), (e) => ReadWriteLockContext.ReadWriteContextPool.PutObject(e as ReadWriteLockContext), this.Uuid, timeout, t => Task.FromResult(this.asyncLocker.ReaderLock(t)), this.CheckLocksLogics, ReadWriteLockContext.Modes.Read).WaitAndUnwrapException();
		}

		public Task<LockHandle> ReaderLockAsync(LockContext context = null, TimeSpan timeout = default) {

			return LockContext.PrepareLock(context, () => ReadWriteLockContext.ReadWriteContextPool.GetObject(), (e) => ReadWriteLockContext.ReadWriteContextPool.PutObject(e as ReadWriteLockContext), this.Uuid, timeout, t => this.asyncLocker.ReaderLockAsync(t), this.CheckLocksLogics, ReadWriteLockContext.Modes.Read);
		}

		public LockHandle WriterLock(LockContext context = null, TimeSpan timeout = default) {

			return LockContext.PrepareLock(context, () => ReadWriteLockContext.ReadWriteContextPool.GetObject(), (e) => ReadWriteLockContext.ReadWriteContextPool.PutObject(e as ReadWriteLockContext), this.Uuid, timeout, t => Task.FromResult(this.asyncLocker.WriterLock(t)), this.CheckLocksLogics, ReadWriteLockContext.Modes.Write).WaitAndUnwrapException();
		}

		public Task<LockHandle> WriterLockAsync(LockContext context = null, TimeSpan timeout = default) {

			return LockContext.PrepareLock(context, () => ReadWriteLockContext.ReadWriteContextPool.GetObject(), (e) => ReadWriteLockContext.ReadWriteContextPool.PutObject(e as ReadWriteLockContext), this.Uuid, timeout, t => this.asyncLocker.WriterLockAsync(t), this.CheckLocksLogics, ReadWriteLockContext.Modes.Write);
		}

		public class ReadWriteLockContext : LockContextInstance {

			public static ObjectPool<ReadWriteLockContext> ReadWriteContextPool { get; } = new ObjectPool<ReadWriteLockContext>(() => new ReadWriteLockContext(), 0, 10);

			
			public enum Modes {
				None,
				Read,
				Write
			}

			public Modes Mode { get; set; } = Modes.None;
		}
	}
}