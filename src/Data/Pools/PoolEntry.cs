using System;
using System.Runtime.CompilerServices;

namespace Neuralia.Blockchains.Tools.Data.Pools {
	public interface IPoolEntry : IDisposableExtended {

		IPoolContext PoolEntry { get; }
	}

	public interface IPoolContext {
		void LockStore<T>(T item, Action<T> completed) where T : IPoolEntry;
		void Lock(Action<ILockedPoolContext> action);
		void LockRetrieve(Action<ILockedPoolContext> action);
		void LockRetrieve();
	}
	
	public interface ILockedPoolContext {
		bool Stored { get; }
		bool Retrieved { get; }
		void TestPoolStored();
		void TestPoolRetrieved();
		void SetStored();
		void SetRetrieved();
	}

	public static class PoolEntryEntities {

		public enum StorageStates : byte {
			Stored = 1,
			Retrieved = 2,
			Loose = 3
		}
	}

	
	public class PoolEntry : IPoolContext,ILockedPoolContext {

		public void LockStore<T>(T item, Action<T> completed)
			where T : IPoolEntry {
			lock(this) {
				if(this.Stored) {
					return;
				}

				// lock a reference so the GC will resurect the object
				this.SetStored();
			}

			completed(item);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Lock(Action<ILockedPoolContext> action) {
			lock(this) {
				if(this.Stored) {
					return;
				}
				action(this);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void LockRetrieve(Action<ILockedPoolContext> action) {
			lock(this) {
				if(this.Retrieved) {
					return;
				}
				this.SetRetrieved();
			}
			action(this);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void LockRetrieve() {
			lock(this) {
				if(this.Retrieved) {
					return;
				}
				this.SetRetrieved();
			}
		}
		private PoolEntryEntities.StorageStates StorageState { get; set; } = PoolEntryEntities.StorageStates.Retrieved;

		public bool Stored {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => this.StorageState == PoolEntryEntities.StorageStates.Stored;
		}
		
		public bool Retrieved {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => this.StorageState == PoolEntryEntities.StorageStates.Retrieved;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void TestPoolStored() {
#if DEBUG
			if(!this.Stored) {
				throw new ApplicationException();
			}
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void TestPoolRetrieved() {
#if DEBUG
			if(this.StorageState != PoolEntryEntities.StorageStates.Retrieved) {
				throw new ApplicationException();
			}
#endif
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetStored() {
			this.StorageState = PoolEntryEntities.StorageStates.Stored;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetRetrieved() {

			this.StorageState = PoolEntryEntities.StorageStates.Retrieved;
		}
	}
}