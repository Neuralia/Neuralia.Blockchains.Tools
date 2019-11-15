using System;

namespace Neuralia.Blockchains.Tools.Data.Pools {
	public interface IPoolEntry : IDisposableExtended {

		PoolEntry PoolEntry { get; }
	}

	public static class PoolEntryEntities {
		
		public enum StorageStates:byte {Stored=1,Retreived=2, Loose=3}
	}

	public class PoolEntry {

		private PoolEntryEntities.StorageStates StorageState { get; set; } = PoolEntryEntities.StorageStates.Retreived;
		
		private readonly object locker = new object();
		
		public void TestPoolStored() {
#if DEBUG
			if(!this.Stored) {
				throw new ApplicationException();
			}
#endif
		}

		public void TestPoolRetreived() {
#if DEBUG
			if(this.StorageState != PoolEntryEntities.StorageStates.Retreived) {
				throw new ApplicationException();
			}
#endif
		}

		public bool Stored => this.StorageState == PoolEntryEntities.StorageStates.Stored;

		
		public void SetStored() {
			this.StorageState = PoolEntryEntities.StorageStates.Stored;
		}

		public void SetRetreived() {
			
			this.StorageState = PoolEntryEntities.StorageStates.Retreived;
		}
	}
}