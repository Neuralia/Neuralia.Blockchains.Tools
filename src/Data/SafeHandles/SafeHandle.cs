using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Neuralia.Blockchains.Tools.Data {

	public interface ISafeHandled : IDisposable {
		SafeHandledEntry SafeHandledEntry { get; }
		void GiveOwnership();
		void TakeOwnership();
		
	}
	
	public interface ISafeHandled<out T> : ISafeHandled where T  :ISafeHandled<T> {

		T Clone();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <remarks>Since this object is recycled on the finalizer, any disposable objects inside will be automatically disposed also. be careful!</remarks>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="U"></typeparam>
	public abstract class SafeHandle<T, U> : IDisposable2
		where T: class, ISafeHandled<T>
		where U : SafeHandle<T, U>, new(){

		protected readonly object locker = new object();
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

		protected SafeHandle<T, U> SetData(U other) {
			return this.SetData(other.Entry);
		}
		
		protected SafeHandle<T, U> SetData(T entry) {
			
			this.Entry = entry;
			
			return this;
		}
		
		public U Clone() {

			return Create(this.Entry?.Clone());
		}
		
		public U Branch() {

			return Create((U)this);
		}

		public T Release() {
			

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


		public static U Create() {
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
			
			
			return this.Entry.Equals(other);
		}

		public override bool Equals(object obj) {
			if(ReferenceEquals(null, obj)) {
				return false;
			}

			if(ReferenceEquals(this, obj)) {
				return true;
			}
			
			
			if(obj.GetType() != this.GetType()) {
				return false;
			}

			return this.Equals((U) obj);
		}

		public override int GetHashCode() {
			
			
			return this.Entry.GetHashCode();
		}

	#region Disposable

		public void Return() {
			this.Dispose();
		}

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private readonly object disposeLocker = new object();
		public void Dispose(bool disposing) {

			lock(this.disposeLocker) {

				if(!this.IsDisposed && disposing) {

					// check if this has already been called.
					this.Entry = null;
				}
				this.IsDisposed = true;
			}
		}

		~SafeHandle() {
			this.Dispose(false);
		}
		
	#endregion

		public bool IsDisposed { get; private set; }
	}
}