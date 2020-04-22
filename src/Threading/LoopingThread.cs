using System;
using System.Threading;
using System.Threading.Tasks;
using Neuralia.Blockchains.Tools.Locking;

namespace Neuralia.Blockchains.Tools.Threading {
	public interface ILoopThread : IThreadBase {
	}

	public interface ILoopThread<out T> : IThreadBase<T>, ILoopThread
		where T : ILoopThread<T> {
	}

	// base class for all threads here
	public abstract class LoopThread<T> : ThreadBase<T>, ILoopThread<T>
		where T : class, ILoopThread<T> {
		protected int sleepTime = 100;
		private readonly ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
		protected override Task DisposeAllAsync() {

			try {
				this.resetEvent?.Dispose();
			} catch {
			}

			return base.DisposeAllAsync();
		}

		public LoopThread() {
		}

		public LoopThread(int sleepTime) : this() {
			this.sleepTime = sleepTime;
		}

		protected void ClearWait() {
			this.resetEvent.Set();
		}

		protected override async Task PerformWork(LockContext lockContext) {
			// Were we already canceled?
			this.CheckShouldCancel();

			while(true) {

				this.CheckShouldCancel();
				
				await this.ProcessLoop(lockContext).ConfigureAwait(false);

				this.CheckShouldCancel();
				
				if(this.resetEvent.Wait(TimeSpan.FromMilliseconds(this.sleepTime))) {
					this.resetEvent.Reset();
				}
			}
		}

		protected abstract Task ProcessLoop(LockContext lockContext);

		/// <summary>
		///     this method allows to check if its time to act, or if we should sleep more
		/// </summary>
		/// <returns></returns>
		protected bool ShouldAct(ref DateTime? action) {
			if(!action.HasValue) {
				return true;
			}

			if(action.Value < DateTime.UtcNow) {
				action = null;

				return true;
			}

			return false;
		}
	}

}