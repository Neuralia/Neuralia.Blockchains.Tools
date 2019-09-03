using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Neuralia.Blockchains.Tools.Cryptography;
using Neuralia.Blockchains.Tools.Cryptography.Encodings;

namespace Neuralia.Blockchains.Tools.Data.Allocation {

	public interface IMemoryBlock<A, P, U> : IByteArray<A, U>
		where P : MemoryBlock<A, P, U>, U, new()
		where A : IEquatable<A>, IComparable<A>
		where U : IByteArray<A, U> {
		int BufferIndex { get; }

		void SetContent(A[] buffer, int offset, IAllocator<A, P, U> allocator, int bufferIndex, int length);
		void SetContent(A[] buffer, int length);
	}

	public abstract class MemoryBlock<A, P, U> : IMemoryBlock<A, P, U>
		where P : MemoryBlock<A, P, U>, U, new()
		where A : IEquatable<A>, IComparable<A>
		where U : IByteArray<A, U> {

		IEnumerator IEnumerable.GetEnumerator() {
			return this.GetEnumerator();
		}

		protected IAllocator<A, P, U> allocator;

		public Memory<A> Memory => ((Memory<A>) this.Bytes).Slice(this.Offset, this.Length);
		public Span<A> Span => ((Span<A>) this.Bytes).Slice(this.Offset, this.Length);

		public A[] Bytes {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			private set;
		}

		public bool IsExactSize => false;

		public int Length {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private set;
		}

		public int Offset {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private set;
		}

		public bool IsNull => this.Bytes == null;

		public bool IsEmpty => this.IsNull || (this.Length == 0);
		public bool IsActive => !this.IsDisposed && this.HasData;

		public bool HasData => !this.IsEmpty;

		public int GetArrayHash() {
			return MemoryAllocators.xxHash32.Hash(this.ToExactByteArray());
		}

		public A this[int i] {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				void ThrowException() {
					throw new IndexOutOfRangeException();
				}

				if(i >= this.Length) {
					ThrowException();
				}

				return this.Bytes[this.Offset + i];
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set {
				void ThrowException() {
					throw new IndexOutOfRangeException();
				}

				if(i >= this.Length) {
					ThrowException();
				}

				this.Bytes[this.Offset + i] = value;
			}
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
		public void SetContent(A[] buffer, int offset, IAllocator<A, P, U> allocator, int bufferIndex, int length) {
			void ThrowDisposed() {
				throw new ApplicationException("reusing a disposed block");
			}

			if(this.IsDisposed) {
				ThrowDisposed();
			}

			if(this.Bytes != null) {
				int g = 0;
			}

			this.Bytes = buffer;
			this.Offset = offset;
			this.Length = length;
			this.BufferIndex = bufferIndex;
			this.allocator = allocator;

			this.Clear();

#if DEBUG && DETECT_LEAKS
			lock(locker) {
				if(FixedAllocator<A, P, U>.LogLeaks) {
					
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetContent(A[] buffer, int length) {

			if(this.IsDisposed) {
				throw new ApplicationException("reusing a disposed block");
			}

			this.Bytes = buffer;
			this.Offset = 0;
			this.Length = length;
			this.BufferIndex = -1;
			this.allocator = null;

			this.Clear();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Return() {

			if(this.IsDisposed) {
				return;
			}

			// the most important to avoid memory leaks. we need to restore our offset so it is available again
			if(this.Bytes != null) {
				this.allocator?.ReturnOffset(this.Length, this.Offset, this.BufferIndex, this.allocator.MemoryContextId);
			}

			if(this.Length > FixedAllocator<A, P, U>.SMALL_SIZE) {
				// its a rented array. lets return it to the pool
				ArrayPool<A>.Shared.Return(this.Bytes);
			}

			this.Bytes = null;
			this.Length = 0;
			this.Offset = 0;
			this.BufferIndex = -1;

			// once we put back the object, others will access it. a backup local variable is better.

			var tempAllocator = this.allocator;
			this.allocator = null;

#if DEBUG && DETECT_LEAKS
			lock(locker) {
				// return has been called, so its not a leak
				if(FixedAllocator<A, P, U>.LogLeaks)
					tempAllocator?.Leaks.Remove(this.id);
			}

#endif

			// if we are really disposing, we dont bother returning the object.
			if(!this.isDisposing && !this.IsDisposed) {
				tempAllocator?.BlockPool.PutObject((P) this);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(MemoryBlock<A, P, U> array1, MemoryBlock<A, P, U> array2) {

			if(ReferenceEquals(array1, null)) {
				return ReferenceEquals(array2, null);
			}

			if(ReferenceEquals(array2, null)) {
				return false;
			}

			return array1.Memory.Span.SequenceEqual(array2.Memory.Span);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(MemoryBlock<A, P, U> array1, MemoryBlock<A, P, U> array2) {

			return !(array1 == array2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(MemoryBlock<A, P, U> array1, IByteArray array2) {

			if(ReferenceEquals(array1, null)) {
				return ReferenceEquals(array2, null);
			}

			if(ReferenceEquals(array2, null)) {
				return false;
			}

			if(ReferenceEquals(array1, array2)) {
				return true;
			}

			if(array2 is IMemoryBlock<A, P, U> casted) {
				return array1.Span.SequenceEqual(casted.Span);
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(MemoryBlock<A, P, U> array1, IByteArray array2) {

			return !(array1 == array2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(U other) {
			if(ReferenceEquals(null, other)) {
				return false;
			}

			if(ReferenceEquals(this, other)) {
				return true;
			}

			return this.Span.SequenceEqual(other.Span);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(IByteArray other) {
			return this == other;
		}

		public int CompareTo(IMemoryBlock<A, P, U> other) {
			return this.Span.SequenceCompareTo(other.Span);
		}

		public int CompareTo(A[] other) {
			return this.Span.SequenceCompareTo(other);
		}

		public int CompareTo(U other) {
			return this.Span.SequenceCompareTo(other.Span);
		}

		public bool Equals(IMemoryBlock<A, P, U> other) {
			return this.Span.SequenceEqual(other.Span);
		}

		public bool Equals(A other) {
			return false;
		}

		public IEnumerator<A> GetEnumerator() {
			return new ByteArrayEnumerator<A, U>((P) this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj) {
			if(ReferenceEquals(null, obj)) {
				return false;
			}

			if(ReferenceEquals(this, obj)) {
				return true;
			}

			if(obj is IMemoryBlock<A, P, U> other) {
				return this.Equals(other);
			}

			if(obj is U other2) {
				return this.Equals(other2);
			}

			if(obj.GetType() != this.GetType()) {
				return false;
			}

			return false;
		}

	#region Copies

		public void SetLength(int length) {
			if(length > this.Bytes.Length) {
				throw new ApplicationException("New length is bigger than available bytes.");
			}

			this.Length = length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public abstract Span<T> CastedArray<T>()
			where T : struct;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T ReadCasted<T>(int index)
			where T : struct {

			return this.CastedArray<T>()[index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteCasted<T>(int index, T value)
			where T : struct {

			this.CastedArray<T>()[index] = value;
		}

		//TODO: !!! do a major cleaning and testing of CopyTo and CopyFrom. Make sure they all point to the Span version to avoind playing with offsets.

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(in Span<A> dest, int srcOffset, int destOffset, int length) {
			this.Memory.Span.Slice(srcOffset, length).CopyTo(dest.Slice(destOffset, length));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(in Span<A> dest, int destOffset) {
			this.CopyTo(dest, 0, destOffset, this.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(in Span<A> dest) {
			this.CopyTo(dest, 0, 0, this.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(ref A[] dest, int srcOffset, int destOffset, int length) {
			this.CopyTo((Span<A>) dest, srcOffset, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(ref A[] dest, int destOffset) {
			this.CopyTo(ref dest, 0, destOffset, this.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(ref A[] dest) {
			this.CopyTo(ref dest, 0, 0, this.Length);
		}

		public void CopyTo(U dest) {
			this.CopyTo(dest.Span);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySpan<A> src, int srcOffset, int destOffset, int length) {
			src.Slice(srcOffset, length).CopyTo(this.Span.Slice(destOffset, length));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySpan<A> src, int srcOffset, int length) {
			this.CopyFrom(src, srcOffset, 0, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySpan<A> src, int destOffset) {
			this.CopyFrom(src, 0, destOffset, src.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySpan<A> src) {
			this.CopyFrom(src, 0, 0, src.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ref A[] src, int srcOffset, int destOffset, int length) {
			this.CopyFrom((ReadOnlySpan<A>) src, srcOffset, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ref A[] src, int srcOffset, int length) {
			this.CopyFrom(ref src, srcOffset, 0, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ref A[] src, int destOffset) {
			this.CopyFrom(ref src, 0, destOffset, src.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ref A[] src) {
			this.CopyFrom(ref src, 0, 0, src.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(U src, int srcOffset, int length) {
			this.CopyFrom(src.Span, srcOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(U src, int destOffset) {
			this.CopyFrom(src.Span, destOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(U src) {
			this.CopyFrom(src.Span);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract P GetAllocatedEntry(int length);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual U Clone() {
			P other = this.allocator?.Take(this.Length);

			if(other == (P) null) {
				other = this.GetAllocatedEntry(this.Length);
			}

			other?.CopyFrom((P) this);

			return other;
		}

		public U Slice(int offset, int length) {
			P slice = this.allocator?.Take(length);

			if(slice == (P) null) {
				slice = this.GetAllocatedEntry(length);
			}

			this.CopyTo(slice, offset, 0, length);

			return slice;
		}

		public U Slice(int offset) {
			return this.Slice(offset, this.Length - offset);
		}

		public abstract U SliceReference(int offset, int length);

		public abstract byte[] ToExactByteArrayCopy();
		public abstract byte[] ToExactByteArray();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySequence<A> src, int srcOffset, int destOffset, int length) {
			src.Slice(srcOffset, length).CopyTo(this.Span.Slice(destOffset, length));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySequence<A> src, int srcOffset, int length) {
			this.CopyFrom(src, srcOffset, 0, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySequence<A> src, int destOffset) {
			this.CopyFrom(src, 0, destOffset, (int) src.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySequence<A> src) {
			this.CopyFrom(src, 0, 0, (int) src.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(Array src, int srcOffset, int destOffset, int length) {
			Buffer.BlockCopy(src, srcOffset, this.Bytes, this.Offset + destOffset, length);

		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(U src, int srcOffset, int destOffset, int length) {
			this.CopyFrom(src.Span, srcOffset, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(Array dest, int srcOffset, int destOffset, int length) {
			Buffer.BlockCopy(this.Bytes, this.Offset + srcOffset, dest, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(U dest, int srcOffset, int destOffset, int length) {
			this.CopyTo(dest.Span, srcOffset, destOffset, length);
		}

		public void CopyTo(U dest, int destOffset) {
			this.CopyTo(dest.Span, destOffset);
		}

	#endregion

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(int offset, int length) {
			if(length <= 76) {
				for(int i = length; i != 0; --i) {
					this.Bytes[(this.Offset + offset + i) - 1] = default;
				}
			} else {
				// 77+
				Array.Clear(this.Bytes, this.Offset + offset, length);
			}

		}

		public abstract void FillSafeRandom();

		public abstract string ToBase64();

		public abstract string ToBase58();
		public abstract string ToBase30();

		public abstract string ToBase32();

		public abstract string ToBase35();
		public abstract string ToBase85();
		public abstract string ToBase94();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear() {
			this.Clear(0, this.Length);
		}

	#region disposable

		public bool IsDisposed { get; private set; }

		public void Dispose() {
			// by a manual dispose, we simply return the block.
			this.Dispose(true);

			// here we coopt the dispose process to possibly return. never call supress finalize here. we reuse the object
			//GC.SuppressFinalize(X);
		}

		protected bool isDisposing;

		protected void Dispose(bool disposing) {
			if(!this.IsDisposed) {

				if(disposing) {
					// this is a fake dispose, so we just treat it like a return
					this.Return();
				} else {
					try {
						// this is a real final dispose. clean up things for real here
						this.isDisposing = true;
						this.Return();
					} finally {
						this.IsDisposed = true;
					}
				}
			}
		}

		~MemoryBlock() {
			// the real dispose called by the GC
			this.Dispose(false);
		}

	#endregion

	}

	[DebuggerDisplay("{Bytes[Offset]}, {Bytes[Offset+1]}, {Bytes[Offset+2]}")]
	public class MemoryBlock : MemoryBlock<byte, MemoryBlock, IByteArray>, IByteArray {

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override Span<T> CastedArray<T>() {
			void ThrowException() {
				throw new ApplicationException($"Not enough memory to cast to array of type {typeof(T)}");
			}

			if((this.Length % Marshal.SizeOf<T>()) != 0) {
				ThrowException();
			}

			return MemoryMarshal.Cast<byte, T>(this.Memory.Span);
		}

		/// <summary>
		///     Slice the contents, but return a reference to the inner data. copies no data
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public override IByteArray SliceReference(int offset, int length) {
			return new ByteArray(this.Bytes, this.Offset + offset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override byte[] ToExactByteArrayCopy() {
			var buffer = new byte[this.Length];

			this.CopyTo(ref buffer);

			return buffer;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override byte[] ToExactByteArray() {
			return this.ToExactByteArrayCopy();
		}

		public override void FillSafeRandom() {
			GlobalRandom.GetNextBytes(this.Bytes, this.Length);
		}

		public override string ToBase85() {
			return new Base85().Encode(this);
		}

		public override string ToBase94() {
			return new Base94().Encode(this);
		}

		public override string ToBase64() {
			return Convert.ToBase64String(this.Bytes, this.Offset, this.Length);
		}

		public override string ToBase58() {
			return new Base58().Encode(this);
		}

		public override string ToBase30() {
			return new Base30().Encode(this);
		}

		public override string ToBase32() {
			return new Base32().Encode(this);
		}

		public override string ToBase35() {
			return new Base35().Encode(this);
		}

		protected override MemoryBlock GetAllocatedEntry(int length) {
			return MemoryAllocators.Instance.allocator.Take(length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(MemoryBlock array1, MemoryBlock array2) {

			if(ReferenceEquals(array1, null)) {
				return ReferenceEquals(array2, null);
			}

			if(ReferenceEquals(array2, null)) {
				return false;
			}

			return array1.Span.SequenceEqual(array2.Span);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(MemoryBlock array1, MemoryBlock array2) {

			return !(array1 == array2);
		}
	}

}