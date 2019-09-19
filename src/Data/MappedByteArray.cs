using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Neuralia.Blockchains.Tools.Data {
	
	internal class MappedByteArray : ByteArray {

		public static readonly FixedAllocator ALLOCATOR = new FixedAllocator(1000, 10);

		internal static MappedByteArray CreatePooled() {
			return new MappedByteArray();
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
			this.PoolEntry.TestPoolRetreived();
			
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
			this.PoolEntry.TestPoolRetreived();
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
			this.PoolEntry.TestPoolRetreived();

			return ByteArray.Create(this.Bytes, this.Offset + offset, length);
		}
		

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(MappedByteArray array1, MappedByteArray array2) {

			if(ReferenceEquals(array1, null)) {
				return ReferenceEquals(array2, null);
			}

			if(ReferenceEquals(array2, null)) {
				return false;
			}

			return array1.Equals(array2);
		}

		public bool Equals(MappedByteArray other) {
			this.PoolEntry.TestPoolRetreived();
			return this.Span.SequenceEqual(other.Span);
		}
		
		public override bool Equals(object obj) {
			this.PoolEntry.TestPoolRetreived();
			var result = base.Equals(obj);

			if(result) {
				return true;
			}
			
			if(obj is MappedByteArray other) {
				return this.Equals(other);
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(MappedByteArray array1, MappedByteArray array2) {

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

			// this must be the last operation, as once in, it will go on for it's next life...
			ALLOCATOR?.BlockPool.PutObject(this, () => {
				this.UpdateGCRegistration(disposing);
			});
			
			// once we put back the object, others will access it. a backup local variable is better.
			
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