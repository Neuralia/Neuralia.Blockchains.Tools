using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Neuralia.Blockchains.Tools.Data.Pools {
	public interface ISecureObjectPool<T> : IDisposableExtended
		where T : class, IPoolEntry {
	}

	/// <summary>
	/// an object pool with checks and locks to ensure single entry of it's items
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SecureObjectPool<T> : ISecureObjectPool<T>
		where T : class, IPoolEntry {

		protected readonly int expandCount;

		protected readonly object locker = new object();

		/// <summary>
		///     The generator for creating new objects.
		/// </summary>
		/// <returns></returns>
		private readonly Func<T> objectFactory;

		/// <summary>
		///     Our pool of objects
		/// </summary>
		private readonly SimpleStack<T> pool;

		public SecureObjectPool(Func<T> objectFactory, int initialCount = 10, int expandCount = 10) {
			this.expandCount = expandCount;
			this.pool = new SimpleStack<T>(initialCount, expandCount);
			this.objectFactory = objectFactory;

			this.CreateMore(initialCount);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetObject() {
			T item;
			
			lock(this.locker) {

				if(this.pool.Count == 0) {
					this.CreateMore(this.expandCount);
				}

				item = this.pool.Pop();
			}
			
			item.PoolEntry.LockRetrieve();

			return item;
		}

		/// <summary>
		///     Returns an object to the pool.
		/// </summary>
		/// <param name="item">The item to return.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PutObject(T item, Action<T> reset) {
			
			// if explicit disposing, go back to the pool. if its a destructor, nothing to do, let it die
			item.PoolEntry.LockStore<T>(item, (i) => {
				lock(this.locker) {
					
					reset(i);
					
					// now it is ready to be freed
					//note:  when inserting here, it MUST be ready, as it will be taken up very quickly for it's next use. can cause bugs if not ready
					this.pool.Push(i);
				}
			});
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void CreateMore(int amount) {
			lock(this.locker) {
				for(int i = amount; i != 0; i--) {
					var entry = this.objectFactory();
					this.PutObject(entry, (item) => {
					});
				}
			}
		}
		
	#region disposable

		public bool IsDisposed { get; private set; }

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {

			if(disposing && !this.IsDisposed) {
				lock(this.locker) {

					// this is a fake dispose, we do a simple memory return
					for(long i = this.pool.Count; i != 0; --i) {
						T entry = this.pool.Pop();
						
						if(entry is IDisposable dispo) {
							dispo.Dispose();
						}
					}
				}
			}

			this.IsDisposed = true;
		}

		~SecureObjectPool() {
			this.Dispose(false);
		}

	#endregion
	}
}