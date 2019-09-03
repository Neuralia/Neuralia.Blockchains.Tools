using System;
using System.Threading;

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

		public LoopThread() {
		}

		public LoopThread(int sleepTime) : this() {
			this.sleepTime = sleepTime;
		}

		protected override void PerformWork() {
			// Were we already canceled?
			this.CheckShouldCancel();

			while(true) {
				Thread.Sleep(this.sleepTime);

				this.ProcessLoop();

				// Poll on this property if you have to do
				// other cleanup before throwing.
				this.CheckShouldCancel();
			}
		}

		protected abstract void ProcessLoop();

		/// <summary>
		///     this method allows to check if its time to act, or if we should sleep more
		/// </summary>
		/// <returns></returns>
		protected bool ShouldAct(ref DateTime? action) {
			if(!action.HasValue) {
				return true;
			}

			if(action.Value < DateTime.Now) {
				action = null;

				return true;
			}

			return false;
		}
	}

}