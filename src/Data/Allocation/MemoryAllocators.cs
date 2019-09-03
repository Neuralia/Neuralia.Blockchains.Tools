using System.Collections.Generic;
using Microsoft.IO;
using Neuralia.Blockchains.Tools.Cryptography.Hash;

namespace Neuralia.Blockchains.Tools.Data.Allocation {
	public class AllocatorInitializer {
		public List<(int index, int initialCount)> entries = new List<(int index, int initialCount)>();
	}

	public sealed class MemoryAllocators {

		public static xxHasher32 xxHash32 = new xxHasher32();

		public readonly FixedByteAllocator allocator;

		public readonly FixedByteAllocator cryptoAllocator;
		public readonly FixesdDoubleByteArrayAllocator doubleArrayCryptoAllocator;

		public readonly RecyclableMemoryStreamManager recyclableMemoryStreamManager;

		static MemoryAllocators() {

		}

		private MemoryAllocators() {
			this.recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
			this.recyclableMemoryStreamManager.AggressiveBufferReturn = true;

			this.allocator = new FixedByteAllocator(1000, 10, 10);

			AllocatorInitializer initializer = new AllocatorInitializer();

			initializer.entries.Add((6, 18000));
			initializer.entries.Add((12, 8000));
			initializer.entries.Add((14, 8000));
			initializer.entries.Add((25, 6000));
			initializer.entries.Add((27, 6000));
			initializer.entries.Add((40, 6000));

			int blockPoolInitialCount = 15300;

			this.cryptoAllocator = new FixedByteAllocator(initializer, blockPoolInitialCount, 1000);

			initializer = new AllocatorInitializer();
			initializer.entries.Add((0, 100));
			initializer.entries.Add((1, 100));
			initializer.entries.Add((2, 100));
			initializer.entries.Add((3, 100));
			initializer.entries.Add((13, 100));
			initializer.entries.Add((26, 25));

			blockPoolInitialCount = 70;

			this.doubleArrayCryptoAllocator = new FixesdDoubleByteArrayAllocator(initializer, blockPoolInitialCount, 5);
		}

		public static MemoryAllocators Instance { get; } = new MemoryAllocators();
	}
}