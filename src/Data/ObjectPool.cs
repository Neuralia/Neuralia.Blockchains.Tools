using System;
using System.Collections.Generic;
using System.Linq;

namespace Neuralia.Blockchains.Tools.Data {

	public interface IObjectPool<T> 
		where T : class {
		T GetObject();
		void PutObject(T item);
	}
	/// <summary>
	///     A fairly simple object pool for items that will be created a lot.
	/// </summary>
	/// <typeparam name="T">The type that is pooled.</typeparam>
	public class ObjectPool<T> : IDisposable2, IObjectPool<T>
		where T : class {
		
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
		/// <remarks>that it is a queue is important here. objecst freshly returned to the queue should sleep there a bit, so finailizers hit their queue reference.</remarks>
		protected readonly Queue<T> pool = new Queue<T>();

		public ObjectPool(Func<T> objectFactory, int initialCount = 0, int expandCount = 10) {
			this.expandCount = expandCount;
			this.objectFactory = objectFactory;

			this.CreateMore(initialCount);
		}

		public int TotalCreated { get; protected set; }

		public virtual void CreateMore(int amount) {
			lock(this.locker) {
				for(int i = amount; i != 0; i--) {
					this.TotalCreated++;
					T newEntry = this.objectFactory.Invoke();
					this.PutObject(newEntry);
				}
			}
		}

		public bool Contains(T entry) {
			lock(this.locker) {

				return false;
			}
		}

		/// <summary>
		///     Returns a pooled object of type T, if none are available another is created.
		/// </summary>
		/// <returns>An instance of T.</returns>
		public virtual T GetObject() {
			lock(this.locker) {
				if(this.pool.Count != 0) {
					T item = this.pool.Dequeue();
					
					return item;
				}

				this.CreateMore(this.expandCount);

				return this.pool.Dequeue();
			}
		}

		public List<TResult> RunAll<TResult>(Func<T, TResult> select) {
			lock(this.locker) {
				return this.pool.Select(select).ToList();
			}
		}

		/// <summary>
		///     Returns an object to the pool.
		/// </summary>
		/// <param name="item">The item to return.</param>
		public virtual void PutObject(T item) {
			if(item == null || (item is IDisposable2 dispo && dispo.IsDisposed)) {
				return;
			}
			
			lock(this.locker) {
				this.pool.Enqueue(item);
			}
		}

		///// <summary>
		///// a method to use in rare cases where a certain object must be removed from the pool
		///// </summary>
		///// <param name="item"></param>
		//public void RemoveObject(T item) {
		//	lock(this.locker) {
		//		var node = this.pool.Find(item);

		//		if(node != null) {
		//			this.pool.Remove(node);
		//		}
		//	}
		//}

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
					var copy = this.pool.ToArray();
					this.pool.Clear();

					foreach(T obj in copy) {

						this.DisposeEntry(obj);

						if(obj is IDisposable dispo) {
							dispo.Dispose();
						}
					}
				}
			}

			this.IsDisposed = true;
		}

		protected virtual void DisposeEntry(T entry) {

		}

		~ObjectPool() {
			this.Dispose(false);
		}

	#endregion

	}
}