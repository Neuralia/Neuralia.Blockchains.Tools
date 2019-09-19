using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Neuralia.Blockchains.Tools.Data {

	public interface ISafeHandled : IDisposable {
		SafeHandledEntry SafeHandledEntry { get; }
		void GiveOwnership();
		void TakeOwnership();
	}
	
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>Since this object is recycled on the finalizer, any disposable objects inside will be automatically disposed also. be careful!</remarks>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="U"></typeparam>
	public class SafeHandle<T, U> : IPoolEntry
		where T: class, ISafeHandled
		where U : SafeHandle<T, U>, IPoolEntry, new(){

		public static readonly SecureObjectPool<U> EntryPool = new SecureObjectPool<U>(CreatePooled);
		private readonly object locker = new object();
		private T entry;

		protected SafeHandle() {
		}
		
		public T Entry {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				lock(this.locker) {
					return this.entry;
				}
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set {
				this.PoolEntry.TestPoolRetreived();

				T previous = default;
				lock(this.locker) {
					if(ReferenceEquals(this.entry, value)) {
						return;
					}

					previous = this.entry;
					this.entry = value;
					
					this.entry?.SafeHandledEntry.Increment();
					// now we own this entry, so we will control it's finalizer
					this.Entry?.GiveOwnership();
				}

				previous?.SafeHandledEntry.Decrement();
			}
		}

		public SafeHandle<T, U> SetData(U other) {
			return this.SetData(other.entry);
		}
		
		public SafeHandle<T, U> SetData(T entry) {
			this.PoolEntry.TestPoolRetreived();
			this.Entry = entry;
			
			return this;
		}
		public U Branch() {
			
			this.PoolEntry.TestPoolRetreived();
			
			return Create((U)this);
		}

		public T Release() {
			this.PoolEntry.TestPoolRetreived();

			T entry = default;
			lock(this.locker) {
				
				if(!this.entry.SafeHandledEntry.Singular) {
					throw new ApplicationException("Cannot release an entry that is shared between handles.");
				}
				
				entry = this.entry;
				// assign it directly to avoid a dispose.
				this.entry = null;
			}

			entry.SafeHandledEntry.Reset();
			entry.TakeOwnership();
			
			return entry;
		}


		public static U Create(){
			return EntryPool.GetObject();
		}
		
		private static U CreatePooled() {
			return new U();
		}

		public static U Create(U other) {
			return (U)Create().SetData(other);
		}

		public static U Create(T other) {
			return (U)Create().SetData(other);
		}

		public bool Equals(U other) {

			if(ReferenceEquals(this, null)) {
				return ReferenceEquals(null, other);
			}

			if(ReferenceEquals(null, other)) {
				return false;
			}

			return this.Equals(other.Entry);
		}
		
		public bool Equals(SafeHandle<T, U> other) {

			if(ReferenceEquals(this, null)) {
				return ReferenceEquals(null, other);
			}

			if(ReferenceEquals(null, other)) {
				return false;
			}

			return this.Equals(other.Entry);
		}

		public bool Equals(T other) {
			if(ReferenceEquals(this, null)) {
				return ReferenceEquals(null, other);
			}

			if(ReferenceEquals(null, other)) {
				return false;
			}
			this.PoolEntry.TestPoolRetreived();
			
			return this.Entry.Equals(other);
		}

		public override bool Equals(object obj) {
			if(ReferenceEquals(null, obj)) {
				return false;
			}

			if(ReferenceEquals(this, obj)) {
				return true;
			}
			this.PoolEntry.TestPoolRetreived();
			
			if(obj.GetType() != this.GetType()) {
				return false;
			}

			return this.Equals((U) obj);
		}

		public override int GetHashCode() {
			this.PoolEntry.TestPoolRetreived();
			
			return this.Entry.GetHashCode();
		}

	#region Disposable

		public void Return() {
			this.Dispose();
		}

		public void Dispose() {
			this.Dispose(true);
		}

		private readonly object disposeLocker = new object();
		public void Dispose(bool disposing) {

			lock(this.disposeLocker) {
				// check if this has already been called.
				if(this.PoolEntry.Stored) {
					return;
				}

				this.Entry = null;
				
				// this must be the last operation, as once in, it will go on for it's next life...
				EntryPool.PutObject((U) this, () => {
					if(!disposing) {
						GC.ReRegisterForFinalize(this);
					}
				});
			}
		}

		~SafeHandle() {
			this.Dispose(false);
		}
		
	#endregion
		

		public PoolEntry PoolEntry { get; } = new PoolEntry();
	}
}