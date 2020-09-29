using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Neuralia.Blockchains.Tools.Data {

	public interface ISafeHandled : IDisposableExtended {
		SafeHandledEntry SafeHandledEntry { get; }
		bool IsNull {get;}
		bool IsEmpty {get;}
		bool HasData {get;}
		bool IsCleared {get;}
	}

	public interface ISafeHandled<out T> : ISafeHandled
		where T : ISafeHandled<T> {

		T Clone();
	}

	/// <summary>
	/// </summary>
	/// <remarks>
	///     Since this object is recycled on the finalizer, any disposable objects inside will be automatically disposed
	///     also. be careful!
	/// </remarks>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="U"></typeparam>
	public abstract class SafeHandle<T, U> : IDisposableExtended
		where T : class, ISafeHandled<T>
		where U : SafeHandle<T, U>, new() {

		protected readonly object locker = new object();
		private T entry;
		private bool own = true;
		
		public T Entry {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				lock(this.locker) {
					return this.entry;
				}
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => this.SetEntry(value, true);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetEntry(T value, bool own = true) {
			T previous = default;
			lock(this.locker) {
				if(ReferenceEquals(this.entry, value)) {
					return;
				}

				if(this.own) {
					previous = this.entry;
				}

				this.entry = value;

				this.own = own;

				if(this.own) {
					this.entry?.SafeHandledEntry.Increment();
				}
			}
				
			previous?.SafeHandledEntry.Decrement();
		}

		public bool IsDisposed { get; private set; }

		protected SafeHandle<T, U> SetData(U other) {
			var result = this.SetData(other.Entry, other.own);

			return result;
		}

		protected SafeHandle<T, U> SetData(T entry, bool own = true) {

			this.SetEntry(entry, own);

			return this;
		}

		public U Clone() {

			return Create(this.Entry?.Clone());
		}

		public U Branch() {

			return Create((U) this);
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

			return entry;
		}
		
		internal static readonly Dictionary<Type, Func<U>> creators = new Dictionary<Type, Func<U>>();
		
		public static U Create() {

			return creators[typeof(U)]();
		}

		public static U Create(U other) {
			return (U) Create().SetData(other);
		}

		public static U Create(T other) {
			return (U) Create().SetData(other);
		}

		public bool Equals(U other) {

			if(ReferenceEquals(this, null)) {
				return ReferenceEquals(null, other);
			}

			if(ReferenceEquals(null, other)) {
				return this.Entry == null || this.Entry.IsEmpty;
			}

			return this.Entry?.Equals(other.Entry) ?? ReferenceEquals(null, other.Entry);

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

			return this.Entry?.GetHashCode() ?? 0;
		}

	#region Disposable

		public void Return() {
			this.Dispose();
		}

		public virtual void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private readonly object disposeLocker = new object();

		protected void Dispose(bool disposing) {

			lock(this.disposeLocker) {

				if(!this.IsDisposed) {

					if(disposing) {
						this.Entry = null;
					} else {
						// the GC will take care of it if it should
						this.entry?.SafeHandledEntry.DecrementNoClear();
						this.entry = null;
					}
				}

				this.IsDisposed = true;
			}
		}

		~SafeHandle() {
			this.Dispose(false);
		}

	#endregion

	}
}