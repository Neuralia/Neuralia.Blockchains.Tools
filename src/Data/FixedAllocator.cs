using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Neuralia.Blockchains.Tools.Data.Arrays;
using Neuralia.Blockchains.Tools.Data.Pools;

namespace Neuralia.Blockchains.Tools.Data {

	public class AllocatorInitializer {
		public List<(int index, int initialCount)> entries = new();
	}

	/// <summary>
	///     a simple and very fast presized allocator for small objects. optimized for speed of take/return
	/// </summary>
	public class FixedAllocator : IDisposableExtended {

		public const int SMALL_SIZE = 1200;
		private readonly object locker = new();

		private readonly int arraySizeIncrements = 10;
		private readonly int maxArraySize;

		private byte[] bufferMap;

		private AllocatorBuffer[] buffers;

		//WARNING: to enable pooling, it requires rock solid dispose discipline. right now, it causes bugs because of disposed objects still being used
		//internal SecureObjectPool<MappedByteArray> BlockPool { get; }
		// this pool allows to defer the more expensive object creation to async batches. improves on demand performance for most cases.
		internal static ConcurrentCreationPool<MappedByteArray> BlockPool { get; } = new ConcurrentCreationPool<MappedByteArray>(100000, () => MappedByteArray.CreatePooled());

		public FixedAllocator(AllocatorInitializer initializer, int initialCounts = 100, int arraySizeIncrements = 5) : this(initialCounts, arraySizeIncrements) {

			foreach((int index, int initialCount) entry in initializer.entries) {
				this.buffers[entry.index].Expand(entry.initialCount);
			}
		}

		public FixedAllocator(int initialCounts = 100, int arraySizeIncrements = 100) {
			this.maxArraySize = SMALL_SIZE;
			this.arraySizeIncrements = arraySizeIncrements;

			this.bufferMap = new byte[this.maxArraySize];
			this.buffers = new AllocatorBuffer[this.maxArraySize / this.arraySizeIncrements];
			
			//this.BlockPool = new SecureObjectPool<MappedByteArray>(MappedByteArray.CreatePooled, initialCounts, arraySizeIncrements);

			for(byte i = 0; i < (this.maxArraySize / this.arraySizeIncrements); i++) {
				for(int j = 0; j < this.arraySizeIncrements; j++) {
					this.bufferMap[(i * this.arraySizeIncrements) + j] = i;
				}

				this.buffers[i] = new AllocatorBuffer(this, (i + 1) * this.arraySizeIncrements, i, initialCounts);
			}
		}

		public void ReturnOffset(int length, int offset, int bufferIndex) {
			this.buffers[this.bufferMap[length]].ReturnOffset(offset, bufferIndex);
			
		}

		/// <summary>
		///     Well it is what it is, and there are memory leaks. here we recover all the leaks to ensure we can reuse the memory
		/// </summary>
		public void RecoverLeakedMemory() {

			Console.WriteLine("Careful, recovering leaked memory!!");

			lock(this.locker) {

				foreach(AllocatorBuffer buffer in this.buffers) {

					buffer.RecoverLeakedMemory();
				}
			}

			// any memory assigned before this is now obsolete. we dotn want it back.		
		}

		/// <summary>
		///     Clear all buffers, reset all memory to 0
		/// </summary>
		public void Wipe() {
			lock(this.locker) {
				foreach(AllocatorBuffer buffer in this.buffers) {

					buffer.Wipe();
				}
			}
		}

#if DEBUG && DETECT_LEAKS
		public static bool LogLeaks = true;
		public int ids = 1;

		public Dictionary<int, MemoryBlock> Leaks { get; } = new Dictionary<int, MemoryBlock>();

		public int NextId() {
			return this.ids++;
		}
#endif
		public void LogMemoryLeaks(string filepath) {
#if DEBUG && DETECT_LEAKS
			if(LogLeaks) {
				var items = this.Leaks.Values.ToList();

//				foreach(var it in items) {
//					List<string> newlines = new List<string>();
//					var lines = it.stack.Split(
//						new[] { Environment.NewLine },
//						StringSplitOptions.None
//					);
//
//					bool insert = false;
//				
//					foreach(var line in lines) {
//
//
//						if(line.Contains("FixedAllocator.cs:line")) {
//							insert = true;
//						}	
//						else if(insert) {
//							newlines.Add(line);
//						}
//					}
//
//					it.stack = string.Join(Environment.NewLine, newlines);
//				}
			
				if(File.Exists(filepath))
					System.IO.File.Delete(filepath);
				
				System.IO.File.WriteAllText(filepath, "");
				foreach(var VARIABLE in items.GroupBy(s => s.stack).OrderByDescending(s => s.Count())) {
					string header = $"Count: {VARIABLE.Count().ToString()}, ids: ({string.Join(",", VARIABLE.Select(i => i.id))})";
					System.IO.File.AppendAllLines(filepath, new string[]{header, VARIABLE.Key});
				}
			}
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteArray Take<T>(int length) {

			int realSize = Marshal.SizeOf<T>() * length;

			return this.Take(realSize);

		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteArray Take(int length) {

			return length >= SMALL_SIZE ? ByteArray.CreateSimpleArray(length) : this.buffers[this.bufferMap[length]].Take(length);
		}

		public void PrintStructure() {

			Console.WriteLine("");

			foreach(AllocatorBuffer entry in this.buffers) {

				entry.PrintStructure();
			}
		}

		internal sealed class AllocatorBuffer : IDisposableExtended {
			private readonly FixedAllocator allocator;
			private readonly int blockLength;

			private readonly object locker = new();
			private readonly BufferIndexStack indexStack;
			private (byte[] buffer, FastBitIndex index)[] buffersSets = Array.Empty<(byte[] buffer, FastBitIndex index)>();

			private readonly int initialCounts;

			public AllocatorBuffer(FixedAllocator allocator, int blockLength, int index, int initialCounts = 100) {
				this.initialCounts = initialCounts;
				this.blockLength = blockLength;
				this.Index = index;
				this.allocator = allocator;
				this.indexStack = new(initialCounts);
			}

			/// <summary>
			///     recovert the lost entries
			/// </summary>
			/// <param name="availableEntries"></param>
			public void RecoverLeakedMemory() {

				// get rid of the slow ConcurrentBag

				int bufferIndex = 0;

				lock(this.locker) {
					foreach(var buffer in this.buffersSets) {

						throw new NotImplementedException();

						// List<int> availableLocalOffsets = this.indexStack.Where(e => e.bufferIndex == bufferIndex).Select(i => i.offset).ToList();
						//
						// for(int i = 0; i < this.initialCounts; i++) {
						//
						// 	int offset = i * this.blockLength;
						//
						// 	if(!availableLocalOffsets.Contains(offset)) {
						// 		// thats it, we reinsert it
						// 		this.ReturnOffset(offset, bufferIndex);
						// 	}
						// }
						//
						// bufferIndex++;
					}
				}
			}

			/// <summary>
			///     Clear all buffers, reset all memory to 0
			/// </summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Wipe() {

				lock(this.locker) {
					foreach(var buffersSet in this.buffersSets) {

						Array.Clear(buffersSet.buffer, 0, buffersSet.buffer.Length);
					}
				}
			}

			public void PrintStructure() {

				// lock(this.locker) {
				// 	long leaks = this.totalBlocks - this.freeOffsets.Count;
				//
				// 	Console.WriteLine($"buffer index: {this.Index}, memory: [0] to [{this.blockLength}]. total entries: {this.totalBlocks} of {this.blockLength} bytes each vs total returned spaces: {this.freeOffsets.Count}. Leaks: {leaks}. total buffer count: {this.buffersSets.Length}. total memory in bytes: {this.buffersSets.Sum(b => b.buffer.Length)}");
				// }
			}

			public long Count {
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get {
					lock(this.locker) {
						return this.indexStack.Length + 1;
					}
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void EnsureCapacity() {

				lock(this.locker) {
					if(this.Count == 0) {
						this.Expand(this.initialCounts);
					}
				}
			}

			public void Expand(int count) {

				byte[] buffer = ArrayPool<byte>.Shared.Rent(count * this.blockLength);

				lock(this.locker) {
					int index = this.buffersSets.Length;
					Array.Resize(ref this.buffersSets, index + 1);

					FastBitIndex hashSet = new(count, this.blockLength);

					this.indexStack.ExpandPush(count, this.blockLength, index);

					this.buffersSets[index] = (buffer, hashSet);
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private MappedByteArray GetEntry(int length) {

				int offset = 0;
				int bufferIndex = 0;
				FastBitIndex index = null;
				byte[] buffer = null;

				lock(this.locker) {
					if(this.Count == 0) {
						return null;
					}

					(offset, bufferIndex) = this.indexStack.Pop();
					(buffer, index) = this.buffersSets[bufferIndex];
					index.Remove(offset);
				}

				//WARNING: to enable pooling, it requires rock solid dispose discipline. right now, it causes bugs because of disposed objects still being used
				//MappedByteArray returnBlock = this.allocator.BlockPool.GetObject();
				//TODO: allocating a finalized object is slow. pooling is much faster but pooling causes issues with objects returned but still used. fix this in the future.

				// until fixed, we use a concurrent creation pool for faster on demand speed
				MappedByteArray returnBlock = BlockPool.GetObject();

#if DEBUG && DETECT_LEAKS
				lock(this.locker) {
					returnBlock.SetId(this.allocator.NextId());
				}
#endif
				returnBlock.SetContent(buffer, offset, bufferIndex, length);

				return returnBlock;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public MappedByteArray Take(int length) {

				do {
					MappedByteArray entry = this.GetEntry(length);

					if(entry != null) {
						return entry;
					}

					this.EnsureCapacity();

				} while(true);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void ReturnOffset(int offset, int bufferIndex) {

				lock(this.locker) {
					if(this.buffersSets[bufferIndex].index.Add(in offset) == 0) {
						this.indexStack.Push(offset, bufferIndex);
					}
				}
			}

		#region disposable

			public bool IsDisposed { get; private set; }

			public int Index { get; }

			public void Dispose() {
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}

			private void Dispose(bool disposing) {

				if(disposing && !this.IsDisposed) {

					foreach(var buffersSet in this.buffersSets) {
						ArrayPool<byte>.Shared.Return(buffersSet.buffer);
					}
				}

				this.IsDisposed = true;
			}

			~AllocatorBuffer() {
				this.Dispose(false);
			}

		#endregion

		}

	#region disposable

		public bool IsDisposed { get; private set; }
		
		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {

			if(disposing && !this.IsDisposed) {

				foreach(AllocatorBuffer buf in this.buffers) {
					buf.Dispose();
				}

				this.buffers = null;
				this.bufferMap = null;
			}

			this.IsDisposed = true;
		}

		~FixedAllocator() {
			this.Dispose(false);
		}

	#endregion

		/// <summary>
		/// a simple and highly optimized stack for the allocator purposes
		/// </summary>
		internal class BufferIndexStack {

			private (int offset, int bufferIndex)[] cache = null;
			private long length = -1;
			private int totalLength = 0;
			public long Length {
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => this.length;
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				private set => this.length = value;
			}

			public BufferIndexStack(int initialCounts) {
				this.totalLength = initialCounts;
				this.cache = ArrayPool<(int offset, int bufferIndex)>.Shared.Rent(this.totalLength);
			}

			public void ExpandPush(int count, int blockLength, int bufferindex) {
				this.totalLength += count;
				var nextCache = ArrayPool<(int offset, int bufferIndex)>.Shared.Rent(this.totalLength);
				Array.Copy(this.cache, 0, nextCache, 0, this.Length + 1);
				ArrayPool<(int offset, int bufferIndex)>.Shared.Return(this.cache);
				this.cache = nextCache;

				for(int i = 0; i < count; i++) {
					this.cache[++this.Length] = (i * blockLength, bufferindex);
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Push(int offset, int bufferIndex) {
				this.cache[++this.Length] = (offset, bufferIndex);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public (int offset, int bufferIndex) Pop() {
				void ThrowEmptyException() {
					throw new IndexOutOfRangeException();
				}

				if(this.Length == -1) {
					ThrowEmptyException();
				}

				return this.cache[this.Length--];
			}
		}

		/// <summary>
		/// a minimal bit index custom built for the fixed allocator
		/// </summary>
		public class FastBitIndex {

			private readonly int blockCount;
			private readonly int blockLength;
			private readonly byte[] buffer;
			private const int BITS_MASK = 0x7;

			public FastBitIndex(int blockCount, int blockLength) {
				this.blockCount = blockCount;
				this.blockLength = blockLength;

				this.buffer = new byte[(this.blockCount >> 3) + 1];
				Array.Fill(this.buffer, byte.MaxValue);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private (int byteIndex, int bitIndex) GetItemAddress(in int item) {
				int index = item / this.blockLength;

				return (index >> 3, (index & BITS_MASK));
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private int GetItemBitSet(int byteIndex, int bitIndex) {
				return (this.buffer[byteIndex] & (1 << bitIndex));
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public int Add(in int item) {

				(int byteIndex, int bitIndex) = this.GetItemAddress(in item);

				int bitSet = this.GetItemBitSet(byteIndex, bitIndex);
				this.buffer[byteIndex] |= (byte) (1 << bitIndex);

				return bitSet;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove(in int item) {

				(int byteIndex, int bitIndex) = this.GetItemAddress(in item);

				this.buffer[byteIndex] &= (byte) ~(1 << bitIndex);
			}
		}
	}
}