//#define LOG_STACK
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Neuralia.Blockchains.Tools.Data.Pools;

namespace Neuralia.Blockchains.Tools.Data.Arrays {

	public class MappedByteArray : ByteArray {

		public static readonly FixedAllocator ALLOCATOR = new FixedAllocator(1000, 10);
		private MappedByteArray() {

		}
		internal static MappedByteArray CreatePooled() {
			var entry = new MappedByteArray();

			return entry;
		}

		public static ByteArray CreateNew(int size) {
			return ALLOCATOR.Take(size);
		}

		public int BufferIndex {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			private set;
		} = -1;

#if  DEBUG && (DETECT_LEAKS || LOG_STACK)
		public int id = 0;	
		private static object locker = new Object();

		// public string initStack;
		// public List<string> lastInitStack = new List<string>();
		// public string returnStack;
		// public string lastReturnStack;
		
		public void SetId(int id) {
			this.id = id;
		}
#endif
		

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetContent(byte[] buffer, int offset, int bufferIndex, int length) {
			
			if(this.IsDisposed) {
				throw new ApplicationException();
			}

			this.Bytes = buffer;
			this.Offset = offset;
			this.Length = length;
			this.BufferIndex = bufferIndex;

			this.Clear();

				

#if  DEBUG && DETECT_LEAKS
			lock(locker) {
				initStack = System.Environment.StackTrace;
				// if(FixedAllocator<A, P, ByteArrayBase>.LogLeaks) {
				// 	
				// 	this.allocator.Leaks.Add(this.id, (P)this);
				// }
			}
#endif
#if DEBUG && LOG_STACK
			lock(locker) {
				// lastInitStack.Add(this.initStack);
				// this.initStack = Environment.StackTrace;
				// this.lastReturnStack = this.returnStack;
				// this.returnStack = null;
			}
#endif

		}

		public override int GetHashCode() {
			return this.Bytes.GetHashCode();
		}

		public void SetLength(int length) {
			if(length > this.Bytes.Length) {
				throw new ApplicationException("New length is bigger than available bytes.");
			}

			this.Length = length;
		}

		/// <summary>
		///     Slice the contents, but return a reference to the inner data. copies no data
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public override ByteArray SliceReference(int offset, int length) {
			return Create(this.Bytes, this.Offset + offset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(MappedByteArray array1, ByteArray array2) {

			if(ReferenceEquals(array1, null)) {
				return ReferenceEquals(array2, null);
			}

			if(ReferenceEquals(array2, null)) {
				return array1.IsEmpty;
			}

			if(ReferenceEquals(array1, array2)) {
				return true;
			}

			return array1.Equals(array2);
		}

		public bool Equals(MappedByteArray other) {

			return this == other;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(MappedByteArray array1, ByteArray array2) {

			return !(array1 == array2);
		}

		protected override void SetDisposedFlag(bool disposing) {
			if(!disposing) {
				// we return to the object pool in disposing mode
				this.IsDisposed = true;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Reset() {
			this.ResetOffsetIncrement();

			// the most important to avoid memory leaks. we need to restore our offset so it is available again
			if(this.Bytes != null) {
				ALLOCATOR.ReturnOffset(this.Length, this.Offset, this.BufferIndex);
			}

			this.Bytes = null;
			this.Length = 0;
			this.ResetOffset();
			this.BufferIndex = -1;
		}
		
		//public IPoolContext PoolEntry { get; } = new PoolEntry();

		// we set this only if pooling the objects
		// protected override void SuppressFinalize(bool disposing) {
		// 	// do nothing since we reuse it
		// }
		
		protected override void DisposeSafeHandle(bool disposing) {
			
			// once we put back the object, others will access it. a backup local variable is better.

			// if(disposing) {
			// 	ALLOCATOR?.BlockPool.PutObject(this, i => i.Reset());
			// } else {
			// 	// this is the end
			// 	this.Reset();
			// }
			this.Reset();

#if  DEBUG && LOG_STACK
			lock(locker) {
				
				//this.returnStack = System.Environment.StackTrace;
			}
#endif
			
#if DEBUG && DETECT_LEAKS
			lock(locker) {
				// return has been called, so its not a leak
				// if(ALLOCATOR.LogLeaks)
				// 	ALLOCATOR.Leaks.Remove(this.id);
			}
#endif

		}

	}

}