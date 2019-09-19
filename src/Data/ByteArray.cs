using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.IO;
using Neuralia.Blockchains.Tools.Cryptography;
using Neuralia.Blockchains.Tools.Cryptography.Encodings;
using Neuralia.Blockchains.Tools.Cryptography.Hash;

namespace Neuralia.Blockchains.Tools.Data {
	
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>Since this object is recycled on the finalizer, any disposable objects inside will be automatically disposed also. be careful!</remarks>
	[DebuggerDisplay("{HasData?Bytes[Offset].ToString():\"null\"}, {HasData?Bytes[Offset+1].ToString():\"null\"}, {HasData?Bytes[Offset+2].ToString():\"null\"}")]

	public abstract class ByteArray : IComparable<byte[]>, IEquatable<byte[]>, IEnumerable<byte>, ISafeHandled, IPoolEntry{

		public static bool RENT_LARGE_BUFFERS = true;
		internal static readonly SecureObjectPool<SimpleByteArray> SimpleByteArrayPool = new SecureObjectPool<SimpleByteArray>(SimpleByteArray.CreatePooled);

		public bool IsExactSize { 
			get {
				this.PoolEntry.TestPoolRetreived();
				return this.IsNull || (this.Length == this.Bytes.Length);
			}
		}

		public Memory<byte> Memory {
			get {
				this.PoolEntry.TestPoolRetreived();
				return ((Memory<byte>) this.Bytes).Slice(this.Offset, this.Length);
			}
		}

		public Span<byte> Span {
			get {
				this.PoolEntry.TestPoolRetreived();
				return ((Span<byte>) this.Bytes).Slice(this.Offset, this.Length);
			}
		}
		
		public int Offset { get; protected set;}
		
		public bool IsNull {
			get {
				this.PoolEntry.TestPoolRetreived();
				return this.Bytes == null;
			}
		}

		public bool IsEmpty {
			get {
				this.PoolEntry.TestPoolRetreived();
				return this.IsNull || (this.Length == 0);
			}
		}

		public bool HasData {
			get {
				this.PoolEntry.TestPoolRetreived();
				return !this.IsEmpty;
			}
		}

		public int Length { get; protected set;}

		public byte[] Bytes { get; protected set; }


		protected ByteArray() {
			this.SafeHandledEntry = new SafeHandledEntry(this.PreDisposeSafeHandle, this.disposeLocker);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Create(int size = 0) {
			
			if(size != 0 && size < 1200) {
				return MappedByteArray.ALLOCATOR.Take(size);
			}

			return CreateSimpleArray(size);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray CreateSimpleArray(int size = 0) {
			
			var entry = SimpleByteArrayPool.GetObject();

			entry.SetSize(size);

			return entry;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray CreateLargeBuffer(int size) {
			
			var entry = SimpleByteArrayPool.GetObject();

			entry.SetSize(size, true);

			return entry;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Create(ByteArray array) {
			var copy = Create(array?.Length??0); 
			
			copy.CopyFrom(array);

			return copy;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Create(ref byte[] array) {
			return Create(array, array?.Length??0); 
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Create(byte[] array) {
			return Create(array, array?.Length??0); 
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Create(byte[] array, int length) {

			return Create(array, 0, length);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Create(byte[] array, int offset, int length) {
			var entry = SimpleByteArrayPool.GetObject();
			
			entry.SetArray(array, offset, length);

			return entry;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Create(in RecyclableMemoryStream stream) {

			var byffer = stream.GetBuffer();

			var newEntry = Create( (int) stream.Length);

			newEntry.CopyFrom(byffer.AsSpan(), 0, newEntry.Length);
			
			return newEntry;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Create(in SafeArrayHandle safeArrayHandle) {
			
			return Create(safeArrayHandle.Entry);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Create<T>(int length) {

			int realSize = Marshal.SizeOf<T>() * length;

			return Create(realSize);
		}

		public static void RecoverLeakedMemory() {
			MappedByteArray.ALLOCATOR.RecoverLeakedMemory();
		}
		
		public byte this[int i] {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				this.PoolEntry.TestPoolRetreived();
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
				this.PoolEntry.TestPoolRetreived();
				void ThrowException() {
					throw new IndexOutOfRangeException();
				}

				if(i >= this.Length) {
					ThrowException();
				}

				this.Bytes[this.Offset + i] = value;
			}
		}

		public string ToBase58() {
			this.PoolEntry.TestPoolRetreived();
			return new Base58().Encode(this);
		}

		public string ToBase85() {
			this.PoolEntry.TestPoolRetreived();
			return new Base85().Encode(this);
		}

		public string ToBase94() {
			this.PoolEntry.TestPoolRetreived();
			return new Base94().Encode(this);
		}

		public string ToBase64() {
			this.PoolEntry.TestPoolRetreived();
			return Convert.ToBase64String(this.Bytes, this.Offset, this.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteArray Clone() {
			this.PoolEntry.TestPoolRetreived();
			var slice = Create(this.Length);

			this.CopyTo(slice);

			return slice;
		}

		/// <summary>
		///     Make a copy with the exact size of the expected array. no rented data
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte[] ToExactByteArrayCopy() {
			this.PoolEntry.TestPoolRetreived();

			var buffer = new byte[this.Length];

			this.CopyTo(ref buffer);

			return buffer;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte[] ToExactByteArray() {

			this.PoolEntry.TestPoolRetreived();
			if(this.IsExactSize && this.Offset == 0) {
				return this.Bytes;
			}

			return this.ToExactByteArrayCopy();
		}

		public int CompareTo(ByteArray other) {
			this.PoolEntry.TestPoolRetreived();
			return this.Span.SequenceCompareTo(other.Span);
		}

		public IEnumerator<byte> GetEnumerator() {
			this.PoolEntry.TestPoolRetreived();
			return new ByteArrayEnumerator(this);
		}
		
		private bool Equals(ByteArray other) {
			this.PoolEntry.TestPoolRetreived();
			return this == other;
		}

		public override int GetHashCode() {
			this.PoolEntry.TestPoolRetreived();
			return (this.Bytes != null ? this.Bytes.GetHashCode() : 0);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return this.GetEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj) {
			this.PoolEntry.TestPoolRetreived();

			if(obj == null) {
				return this.IsEmpty;
			}

			if(obj is ByteArray array) {
				return this == array;
			}

			if(obj is byte[] bytes) {
				return this.Span.SequenceEqual((Span<byte>) bytes);
			}

			if(obj is Memory<byte> memoryBytes) {
				return this.Memory.Span.SequenceEqual(memoryBytes.Span);
			}

			return base.Equals(obj);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(ByteArray array1, ByteArray array2) {

			if(ReferenceEquals(array1, null)) {
				return ReferenceEquals(array2, null);
			}

			if(ReferenceEquals(array2, null)) {
				return array1.IsNull;
			}

			if(ReferenceEquals(array1, array2)) {
				return true;
			}

			return array1.Span.SequenceEqual(array2.Span);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(ByteArray array1, ByteArray array2) {
			return !(array1 == array2);
		}


		//Im disabling this for now, its too confusing. Call either Bytes for the full array, ToExactByteArray() to get a compromise and ToExactByteArrayCopy to get a full copy.
		//		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//		public static explicit operator byte[](ByteArrayBase baw) {
		//			// no choice, otherwise we risk returning an array that is bigger than what is expect if it is rented.
		//			return baw.ToExactByteArray();
		//		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> CastedArray<T>()
			where T : struct {
			this.PoolEntry.TestPoolRetreived();
			if((this.Length % Marshal.SizeOf<T>()) != 0) {
				throw new ApplicationException($"Not enough memory to cast to array of type {typeof(T)}");
			}

			return MemoryMarshal.Cast<byte, T>(this.Memory.Span);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T ReadCasted<T>(int index)
			where T : struct {

			this.PoolEntry.TestPoolRetreived();
			return this.CastedArray<T>()[index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteCasted<T>(int index, T value)
			where T : struct {

			this.PoolEntry.TestPoolRetreived();
			this.CastedArray<T>()[index] = value;
		}

		public ByteArray Slice(int offset, int length) {
			this.PoolEntry.TestPoolRetreived();
			var slice = Create(length);

			this.CopyTo(slice, offset, 0, length);

			return slice;
		}

		public ByteArray Slice(int offset) {
			this.PoolEntry.TestPoolRetreived();
			return this.Slice(offset, this.Length - offset);
		}

		/// <summary>
		///     Slice the contents, but return a reference to the inner data. copies no data
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public abstract ByteArray SliceReference(int offset, int length);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual void CopyTo(in Span<byte> dest, int srcOffset, int destOffset, int length) {
			this.PoolEntry.TestPoolRetreived();
			this.Memory.Span.Slice(srcOffset, length).CopyTo(dest.Slice(destOffset, length));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(in Span<byte> dest, int destOffset) {
			this.CopyTo(dest, 0, destOffset, this.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(in Span<byte> dest) {
			this.CopyTo(dest, 0, 0, this.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(ref byte[] dest, int srcOffset, int destOffset, int length) {
			this.CopyTo((Span<byte>) dest, srcOffset, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(ref byte[] dest, int destOffset) {
			this.CopyTo(ref dest, 0, destOffset, this.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(ref byte[] dest) {
			this.CopyTo(ref dest, 0, 0, this.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(ByteArray dest, int srcOffset, int destOffset, int length) {
			this.CopyTo(dest.Span, srcOffset, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(ByteArray dest, int destOffset) {
			this.CopyTo(dest, 0, destOffset, this.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(ByteArray dest) {
			this.CopyTo(dest, 0, 0, this.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySpan<byte> src, int srcOffset, int destOffset, int length) {
			this.PoolEntry.TestPoolRetreived();
			src.Slice(srcOffset, length).CopyTo(this.Span.Slice(destOffset, length));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySpan<byte> src, int srcOffset, int length) {
			this.CopyFrom(src, srcOffset, 0, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySpan<byte> src, int destOffset) {
			this.CopyFrom(src, 0, destOffset, src.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySpan<byte> src) {
			this.CopyFrom(src, 0, 0, src.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ref byte[] src, int srcOffset, int destOffset, int length) {
			this.CopyFrom((ReadOnlySpan<byte>) src, srcOffset, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ref byte[] src, int srcOffset, int length) {
			this.CopyFrom(ref src, srcOffset, 0, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ref byte[] src, int destOffset) {
			this.CopyFrom(ref src, 0, destOffset, src.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ref byte[] src) {
			this.CopyFrom(ref src, 0, 0, src.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ByteArray src, int srcOffset, int destOffset, int length) {
			this.CopyFrom(src.Span, srcOffset, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ByteArray src, int srcOffset, int length) {

			this.CopyFrom(src.Span, srcOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ByteArray src, int destOffset) {

			this.CopyFrom(src.Span, destOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ByteArray src) {

			this.CopyFrom(src.Span);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySequence<byte> src, int srcOffset, int destOffset, int length) {
			this.PoolEntry.TestPoolRetreived();
			src.Slice(srcOffset, length).CopyTo(this.Span.Slice(destOffset, length));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySequence<byte> src, int srcOffset, int length) {
			this.CopyFrom(src, srcOffset, 0, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySequence<byte> src, int destOffset) {
			this.CopyFrom(src, 0, destOffset, (int) src.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySequence<byte> src) {
			this.CopyFrom(src, 0, 0, (int) src.Length);
		}

		/// <summary>
		///     Clear the memory array
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear() {
			this.PoolEntry.TestPoolRetreived();
			if(this.HasData) {
				this.Span.Clear();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(int offset, int length) {
			this.PoolEntry.TestPoolRetreived();
			if(this.HasData) {
				this.Span.Slice(offset, length).Clear();
			}
		}

		public void FillSafeRandom() {

			GlobalRandom.GetNextBytes(this.Bytes, this.Length);
		}

		public int GetArrayHash() {
			this.PoolEntry.TestPoolRetreived();
			xxHasher32 hasher = new xxHasher32();
			return hasher.Hash(this.ToExactByteArray());
		}

		public string ToBase30() {
			this.PoolEntry.TestPoolRetreived();
			return new Base30().Encode(this);
		}

		public string ToBase32() {
			this.PoolEntry.TestPoolRetreived();
			return new Base32().Encode(this);
		}

		public string ToBase35() {
			this.PoolEntry.TestPoolRetreived();
			return new Base35().Encode(this);
		}

		public int CompareTo(byte[] other) {
			this.PoolEntry.TestPoolRetreived();
			return this.Span.SequenceCompareTo(other);
		}

		public bool Equals(byte[] other) {
			return this.Span.SequenceEqual(other);
		}

		public static SafeArrayHandle FromBase30(string value) {
			return new Base30().Decode(value);
		}

		public static SafeArrayHandle FromBase32(string value) {
			return new Base32().Decode(value);
		}

		public static SafeArrayHandle FromBase35(string value) {
			return new Base35().Decode(value);
		}

		public static SafeArrayHandle FromBase58(string value) {
			return new Base58().Decode(value);
		}

		public static SafeArrayHandle FromBase64(string value) {
			return Convert.FromBase64String(value);
		}

		public static SafeArrayHandle FromBase85(string value) {
			return new Base85().Decode(value);
		}

		public static SafeArrayHandle FromBase94(string value) {
			return new Base94().Decode(value);
		}

		public static implicit operator ByteArray(byte[] data) => Create(data);
	#if DEBUG

#endif
		
	#region Disposable

		private bool ownershipGiven = false;
		public void GiveOwnership() {
			lock(this.disposeLocker) {
				// the suppression ownership is taken by a parent
				if(!this.ownershipGiven) {
					GC.SuppressFinalize(this);
					this.ownershipGiven = true;
				}
			}
		}
		
		public void TakeOwnership() {

			lock(this.disposeLocker) {
				// the suppression ownership is taken by a parent
				if(this.ownershipGiven) {
					GC.ReRegisterForFinalize(this);
					this.ownershipGiven = false;
				}
			}
		}
		
		public Action<ByteArray> Disposed { get; set; }

		public void Return() {
			this.Dispose();
		}
		
		public void Dispose() {
			this.Dispose(true);
		}

		private readonly object disposeLocker = new object();
		
		private void Dispose(bool disposing) {

			int preLockEntry = this.PoolEntry.StoreCounter;
			
			lock(this.disposeLocker) {
				// check if this has already been called.
				if(this.PoolEntry.Stored || preLockEntry != this.PoolEntry.StoreCounter) {
					return;
				}

				if(!disposing) {
					// this must be the last operation, as once disposed, it will go on for it's next life...
					this.SafeHandledEntry.Reset();
				}
				this.SafeHandledEntry.Dispose(disposing);

				this.PoolEntry.IncrementStoreCounter();
			}
		}

		private void PreDisposeSafeHandle(bool disposing) {

			int preLockEntry = this.PoolEntry.StoreCounter;
			
			lock(this.disposeLocker) {
				if(this.PoolEntry.Stored || preLockEntry != this.PoolEntry.StoreCounter) {
					return;
				}

				this.Disposed?.Invoke(this);
				this.Disposed = null;

				this.DisposeSafeHandle(disposing);

				// we just disposed, we are alone again. we take back our ownership
				this.ownershipGiven = false;
			}
		}

		protected void UpdateGCRegistration(bool disposing) {
			if(!disposing || (this.ownershipGiven)) {
				this.TakeOwnership();
			}
		}

		protected abstract void DisposeSafeHandle(bool disposing);

		~ByteArray() {
			this.Dispose(false);
		}

	#endregion


		public PoolEntry PoolEntry { get; } = new PoolEntry();
		public SafeHandledEntry SafeHandledEntry { get; }
	}
}