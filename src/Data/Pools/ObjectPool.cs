using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Neuralia.Blockchains.Tools.Data.Pools {
	public interface IObjectPool<T> {
		T GetObject();
		void PutObject(T item);
	}

	/// <summary>
	///     A fairly simple object pool for items that will be created a lot.
	/// </summary>
	/// <typeparam name="T">The type that is pooled.</typeparam>
	public class ObjectPool<T> : IDisposableExtended, IObjectPool<T> where T: class {

		protected readonly int expandCount;

		protected readonly object locker = new object();

		/// <summary>
		///     The generator for creating new objects.
		/// </summary>
		/// <returns></returns>
		protected readonly Func<T> objectFactory;

		/// <summary>
		///     Our pool of objects
		/// </summary>
		private readonly SimpleStack<T> pool;

		public ObjectPool(Func<T> objectFactory, int initialCount = 10, int expandCount = 10) {
			this.pool = new SimpleStack<T>(initialCount, expandCount);
			this.expandCount = expandCount;
			this.objectFactory = objectFactory;

			this.CreateMore(initialCount);
		}
		
		/// <summary>
		///     Returns a pooled object of type T, if none are available another is created.
		/// </summary>
		/// <returns>An instance of T.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetObject() {
			lock(this.locker) {
				if(this.pool.Count != 0) {
					T item = this.pool.Pop();

					return item;
				}

				this.CreateMore(this.expandCount);

				return this.pool.Pop();
			}
		}

		/// <summary>
		///     Returns an object to the pool.
		/// </summary>
		/// <param name="item">The item to return.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PutObject(T item) {
			if((item == null) || (item is IDisposableExtended dispo && dispo.IsDisposed)) {
				return;
			}

			lock(this.locker) {
				this.pool.Push(item);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void CreateMore(int amount) {
			lock(this.locker) {
				for(int i = amount; i != 0; i--) {
					T newEntry = this.objectFactory.Invoke();
					this.PutObject(newEntry);
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

		~ObjectPool() {
			this.Dispose(false);
		}

	#endregion

	}
}