using System;
using System.Runtime.CompilerServices;

namespace Neuralia.Blockchains.Tools.Data.Arrays {

	internal class MappedByteArray : ByteArray {

		public static readonly FixedAllocator ALLOCATOR = new FixedAllocator(1000, 10);

		private MappedByteArray() {
			
		}

		internal static MappedByteArray CreatePooled() {
			return new MappedByteArray();
		}

		public static ByteArray CreateNew(int size) {
			return ALLOCATOR.Take(size);
		}
		
		public int BufferIndex {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			private set;
		} = -1;

#if DEBUG && (DETECT_LEAKS || LOG_STACK)
		public int id = 0;	
		private static object locker = new Object();

		public string stack;
	
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
			
		//	stack = System.Environment.StackTrace;
		
			
#if DEBUG && DETECT_LEAKS
			lock(locker) {
				if(FixedAllocator<A, P, ByteArrayBase>.LogLeaks) {
					
					this.allocator.Leaks.Add(this.id, (P)this);
				}
			}
#endif
#if DEBUG && LOG_STACK
			lock(locker) {
				this.stack = Environment.StackTrace;
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
			

			return ByteArray.Create(this.Bytes, this.Offset + offset, length);
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

		protected override void DisposeSafeHandle(bool disposing) {
			
			// the most important to avoid memory leaks. we need to restore our offset so it is available again
			if(this.Bytes != null) {
				ALLOCATOR.ReturnOffset(this.Length, this.Offset, this.BufferIndex, ALLOCATOR.MemoryContextId);
			}

			this.Bytes = null;
			this.Length = 0;
			this.Offset = 0;
			this.BufferIndex = -1;

			// once we put back the object, others will access it. a backup local variable is better.

			// if(disposing) {
			// 	// if explicit disposing, go back to the pool. if its a destructor, nothing to do, let it die
			// 	ALLOCATOR?.BlockPool.PutObject(this);
			// }
#if DEBUG && DETECT_LEAKS
			lock(locker) {
				// return has been called, so its not a leak
				if(ALLOCATOR.LogLeaks)
					ALLOCATOR.Leaks.Remove(this.id);
			}
#endif
			
		}
	}
	
	
	

}