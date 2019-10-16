using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Neuralia.Blockchains.Tools.Data {


	public class AllocatorInitializer {
		public List<(int index, int initialCount)> entries = new List<(int index, int initialCount)>();
	}
	
	/// <summary>
	///     a simple and very fast presized allocator for small objects
	/// </summary>
	public class FixedAllocator : IDisposable2{

		public const int SMALL_SIZE = 1200;
		private readonly object locker = new object();

		private readonly int arraySizeIncrements = 10;
		private readonly int maxArraySize;

		private byte[] bufferMap;

		private AllocatorBuffer[] buffers;

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

			
			for(byte i = 0; i < (this.maxArraySize / this.arraySizeIncrements); i++) {
				for(int j = 0; j < this.arraySizeIncrements; j++) {
					this.bufferMap[(i * this.arraySizeIncrements) + j] = i;
				}

				this.buffers[i] = new AllocatorBuffer(this, (i + 1) * this.arraySizeIncrements, i, initialCounts);
			}
		}

		public void ReturnOffset(int length, int offset, int bufferIndex, int memory_context_id) {
			if(memory_context_id == this.MemoryContextId) {
				this.buffers[this.bufferMap[length]].ReturnOffset(offset, bufferIndex);
			}
		}

		/// <summary>
		///     Well it is what it is, and there are memory leaks. here we recover all the leaks to ensure we can reuse the memory
		/// </summary>
		public void RecoverLeakedMemory() {

			Console.WriteLine("Careful, recovering leaked memory!!");

			lock(this.locker) {
				this.MemoryContextId += 1;

				foreach(var buffer in this.buffers) {

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
				foreach(var buffer in this.buffers) {

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

			foreach(var entry in this.buffers) {

				entry.PrintStructure();
			}
		}

		internal sealed class AllocatorBuffer : IDisposable2{

			private readonly FixedAllocator allocator;
			private readonly int blockLength;
			private readonly List<byte[]> buffersSets = new List<byte[]>();
			private readonly Stack<(int offset, int bufferIndex)> freeOffsets = new Stack<(int offset, int bufferIndex)>();

			private readonly int initialCounts;

			/// <summary>
			///     offset as key, and buffer index as value
			/// </summary>
			private readonly HashSet<int> keys = new HashSet<int>();

			private readonly object locker = new object();
			private int freeBlocks;

			private int totalBlocks;

			public AllocatorBuffer(FixedAllocator allocator, int blockLength, int index, int initialCounts = 100) {
				this.initialCounts = initialCounts;
				this.blockLength = blockLength;
				this.totalBlocks = 0;
				this.Index = index;
				this.allocator = allocator;
			}

			public void ReturnOffset(int offset, int bufferIndex) {
				lock(this.locker) {
					if(!this.keys.Contains(offset)) {
						this.freeOffsets.Push((offset, bufferIndex));
						this.keys.Add(offset);
					}
				}
			}

			/// <summary>
			///     recovert the lost entries
			/// </summary>
			/// <param name="availableEntries"></param>
			public void RecoverLeakedMemory() {

				// get rid of the slow ConcurrentBag

				int bufferindex = 0;

				lock(this.locker) {
					foreach(var buffer in this.buffersSets) {

						var availableLocalOffsets = this.freeOffsets.Where(e => e.bufferIndex == bufferindex).Select(i => i.offset).ToList();

						for(int i = 0; i < this.initialCounts; i++) {

							int offset = i * this.blockLength;

							if(!availableLocalOffsets.Contains(offset)) {
								// thats it, we reinsert it
								this.ReturnOffset(offset, bufferindex);
							}
						}

						bufferindex++;
					}
				}
			}

			/// <summary>
			///     Clear all buffers, reset all memory to 0
			/// </summary>
			public void Wipe() {

				foreach(var buffer in this.buffersSets) {

					Array.Clear(buffer, 0, buffer.Length);
				}
			}

			public void PrintStructure() {

				lock(this.locker) {
					int leaks = this.totalBlocks - this.freeOffsets.Count;

					Console.WriteLine($"buffer index: {this.Index}, memory: [0] to [{this.blockLength}]. total entries: {this.totalBlocks} of {this.blockLength} bytes each vs total returned spaces: {this.freeOffsets.Count}. Leaks: {leaks}. total buffer count: {this.buffersSets.Count}. total memory in bytes: {this.buffersSets.Sum(b => b.Length)}");
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void EnsureCapacity() {

				lock(this.locker) {
					if(this.freeOffsets.Count == 0) {

						this.Expand(this.initialCounts);
					}
				}

			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Expand(int blockCount) {
				int newIndex = this.buffersSets.Count;
				var bufferSet = ArrayPool<byte>.Shared.Rent(blockCount * this.blockLength);

				this.buffersSets.Add(bufferSet);

				for(int i = 0; i < blockCount; i++) {
					this.ReturnOffset(i * this.blockLength, newIndex);
				}

				this.totalBlocks += blockCount;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private MappedByteArray GetEntry(int length) {

				int offset = 0;
				int bufferIndex = 0;

				lock(this.locker) {
					if(this.freeOffsets.Count == 0) {
						return null;
					}

					(offset, bufferIndex) = this.freeOffsets.Pop();
					this.keys.Remove(offset);
				}

				MappedByteArray returnBlock = MappedByteArray.CreateNew();
				
#if DEBUG && DETECT_LEAKS
				lock(this.locker) {
					returnBlock.SetId(this.allocator.NextId());
				}
#endif
				returnBlock.SetContent(this.buffersSets[bufferIndex], offset, bufferIndex, length);

				return returnBlock;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public MappedByteArray Take(int length) {

				do {
					MappedByteArray entry = this.GetEntry(length);

					if(entry != (ByteArray) null) {
						return entry;
					}

					this.EnsureCapacity();

				} while(true);
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

					foreach(var buf in this.buffersSets) {
						ArrayPool<byte>.Shared.Return(buf);
					}

					this.buffersSets.Clear();

					this.freeBlocks = 0;
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

		public int MemoryContextId { get; private set; } = 1;

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {

			if(disposing && !this.IsDisposed) {
	
				foreach(var buf in this.buffers) {
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

	}
}