using System.Collections.Generic;

namespace Neuralia.Blockchains.Tools.Data.Pools {


	/// <summary>
	///     A very simple and non thread safe object pool for items that will be created a lot.
	/// </summary>
	/// <typeparam name="T">The type that is pooled.</typeparam>
	public class SimpleObjectPool<T> : IObjectPool<T> 
		where T : class, new(){

		protected readonly int expandCount;

		/// <summary>
		///     Our pool of objects
		/// </summary>
		private readonly SimpleStack<T> pool;
		
		public SimpleObjectPool(int initialCount = 10, int expandCount = 10) {
			this.pool = new SimpleStack<T>(initialCount, expandCount);
			this.expandCount = expandCount;

			this.CreateMore(initialCount);
		}
		
		/// <summary>
		///     Returns a pooled object of type T, if none are available another is created.
		/// </summary>
		/// <returns>An instance of T.</returns>
		public T GetObject() {

			if(this.pool.Count == 0) {
				this.CreateMore(this.expandCount);
			}
			
			return this.pool.Pop();
		}

		/// <summary>
		///     Returns an object to the pool.
		/// </summary>
		/// <param name="item">The item to return.</param>
		public void PutObject(T item) {
			this.pool.Push(item);
		}

		public void CreateMore(int amount) {
			
			for(int i = amount; i != 0; i--) {
				this.PutObject(new T());
			}
		}
	}
}