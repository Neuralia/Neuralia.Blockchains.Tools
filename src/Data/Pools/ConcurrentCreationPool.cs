using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Neuralia.Blockchains.Tools.General;

namespace Neuralia.Blockchains.Tools.Data.Pools {
	/// <summary>
	///     a special object allocator that defers the creation in batches on another async thread, so that the objects are
	///     available when the requester needs it quickly.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ConcurrentCreationPool<T> {

		private readonly T[] cache;
		private readonly Func<T> creationFunc;
		private readonly int expandCount;
		private readonly object locker = new();
		private readonly ClosureWrapper<Task> task = new();
		private readonly object taskLocker = new();

		private long filling;

		public ConcurrentCreationPool(int expandCount, Func<T> creationFunc) {
			this.expandCount = expandCount;
			this.creationFunc = creationFunc;
			this.cache = new T[this.expandCount];
			this.Expand(expandCount);

		}

		public long Length {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private set;
		} = -1;

		private void Expand(int count) {
			
			Interlocked.Increment(ref this.filling);

			for(int i = 0; i < count; i++) {
				lock(this.locker) {
					this.cache[++this.Length] = this.creationFunc();
				}
			}
			
			Interlocked.Decrement(ref this.filling);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetObject() {

			if(Interlocked.Read(ref this.filling) == 0) {
				lock(this.locker) {
					
					if(this.Length != -1) {
						
						if(this.Length == 0) {
							this.TriggerAsyncExpand();
						}
						
						return this.PullOne();
					}
				}
			}
			
			// rare that we get here, lets create the usual way until we have a refill
			return this.creationFunc();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private T PullOne() {

			T node = this.cache[this.Length];
			--this.Length;

			return node;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void TriggerAsyncExpand() {
			// trigger an async expansion
			lock(this.taskLocker) {
				if(this.task.IsDefault) {
					this.task.Value = Task.Run(() => {
						this.Expand(this.expandCount);
						this.task.Value = null;
					});
				}
			}
		}
	}
}