using System;
using System.Runtime.CompilerServices;

namespace Neuralia.Blockchains.Tools.Data.Pools {
	
	/// <summary>
	/// a simple and highly speed optimized stack for the allocator purposes
	/// </summary>
	internal class SimpleStack<T> where T: class {

		private T[] buffer;
		private uint count;
		public uint Count => this.count;
		private readonly int expandCount;
		
		public SimpleStack(int initialCount = 10, int expandCount = 10) {
			this.expandCount = expandCount;

			this.buffer = Array.Empty<T>();
			this.Expand(initialCount);
		}

		public void Expand(int count) {
			Array.Resize(ref this.buffer, this.buffer.Length + count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Push(T item) {

			uint localCount = this.count;
			if (localCount == (uint)this.buffer.Length) {
				this.Expand(this.expandCount);
			}
			
			this.buffer[localCount] = item;
			++this.count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Pop() {
			if(this.Count == 0) {
				return null;
			}
			
			var localBuffer = this.buffer;
			--this.count;
			uint localCount = this.count;
			var entry = localBuffer[localCount];
			localBuffer[localCount] = null!;

			return entry;
		}
	}
}