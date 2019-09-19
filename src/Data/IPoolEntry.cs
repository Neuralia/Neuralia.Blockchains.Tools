using System;
using System.Threading;

namespace Neuralia.Blockchains.Tools.Data {
	
	public interface IPoolEntry : IDisposable {

		PoolEntry PoolEntry { get; }
	}

	public static class PoolEntryEntities {
		
		public enum StorageStates:byte {New=0, Stored=1,Retreived=2, Loose=3}
	}

	public class PoolEntry {

		public PoolEntryEntities.StorageStates StorageState { get; set; } = PoolEntryEntities.StorageStates.New;
		//TODO: this can be all stored on a single int and binary shifted.
		private int storeCounter = 0;
		
		public void TestPoolStored() {
#if DEBUG
			if(!this.Stored) {
				throw new ApplicationException();
			}
#endif
		}

		public void TestPoolRetreived() {
#if DEBUG
			if(this.StorageState != PoolEntryEntities.StorageStates.New && this.StorageState != PoolEntryEntities.StorageStates.Retreived) {
				throw new ApplicationException();
			}
#endif
		}

		public bool Stored => this.StorageState == PoolEntryEntities.StorageStates.Stored;

		public int StoreCounter => Interlocked.CompareExchange(ref this.storeCounter, 0, 0);

		public void IncrementStoreCounter() {
			int value = Interlocked.CompareExchange(ref this.storeCounter, 0, 0);
			if(value == int.MaxValue) {
				Interlocked.Exchange(ref this.storeCounter, 0);
			}

			Interlocked.Increment(ref this.storeCounter);
		}
		public void SetStored() {
			this.StorageState = PoolEntryEntities.StorageStates.Stored;
		}

		public void SetRetreived() {
			this.StorageState = PoolEntryEntities.StorageStates.Retreived;
		}
	}
	
}