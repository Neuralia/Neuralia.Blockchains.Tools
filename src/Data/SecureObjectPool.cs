using System;
using System.Runtime.CompilerServices;

namespace Neuralia.Blockchains.Tools.Data {
	public interface ISecureObjectPool<T> : IObjectPool<T>
		where T : class, IPoolEntry {

	}
	
	public class SecureObjectPool<T> : ObjectPool<T> , ISecureObjectPool<T> 
		where T : class, IPoolEntry {
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override T GetObject() {
			T item = default;
			lock(this.locker) {
				if(this.pool.Count != 0) {
					item = this.pool.Dequeue();

					this.PrepareItem(ref item);
					return item;
				}

				this.CreateMore(this.expandCount);

				item = this.pool.Dequeue();
				this.PrepareItem(ref item);
				return item;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PrepareItem(ref T item) {
			item.PoolEntry.TestPoolStored();
			item.PoolEntry.SetRetreived();
		}

		/// <summary>
		///  used to lock references
		/// </summary>
		private T temp;
		
		/// <summary>
		///     Returns an object to the pool.
		/// </summary>
		/// <param name="item">The item to return.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PutObject(T item, Action callback) {
			if(item == null) {
				return;
			}
			
			lock(this.locker) {
				if(item.PoolEntry.Stored) {
					return;
				}

				// lock a reference so the GC will resurect the object
				this.temp = item;
				callback.Invoke();
				item.PoolEntry.SetStored();
				
				// now it is ready to be freed
				//note:  when inserting here, it MUST be ready, as it will be taken up very quickly for it's next use. can cause bugs if not ready
				this.pool.Enqueue(this.temp);
				this.temp = null;
			}
		}
		
		/// <summary>
		///     Returns an object to the pool.
		/// </summary>
		/// <param name="item">The item to return.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void PutObject(T item) {
			this.PutObject(item, () => {
			});
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void CreateMore(int amount) {
			lock(this.locker) {
				for(int i = amount; i != 0; i--) {
					this.TotalCreated++;
					T newEntry = this.objectFactory.Invoke();
					newEntry.PoolEntry.SetRetreived(); 
					
					this.PutObject(newEntry);
				}
			}
		}

		public SecureObjectPool(Func<T> objectFactory, int initialCount = 0, int expandCount = 10) : base(objectFactory, initialCount, expandCount) {
		}
	}
}