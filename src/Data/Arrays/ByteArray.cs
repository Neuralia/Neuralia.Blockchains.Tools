using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.IO;
using Neuralia.Blockchains.Tools.Cryptography;
using Neuralia.Blockchains.Tools.Cryptography.Encodings;
using Neuralia.Blockchains.Tools.Cryptography.Hash;
using Neuralia.Blockchains.Tools.Serialization;

namespace Neuralia.Blockchains.Tools.Data.Arrays {

	/// <summary>
	/// </summary>
	/// <remarks>
	///     Since this object is recycled on the finalizer, any disposable objects inside will be automatically disposed
	///     also. be careful!
	/// </remarks>
	[DebuggerDisplay("{HasData?Bytes[Offset].ToString():\"null\"}, {HasData?Bytes[Offset+1].ToString():\"null\"}, {HasData?Bytes[Offset+2].ToString():\"null\"}")]
	public abstract class ByteArray : IComparable<byte[]>, IEquatable<byte[]>, IEnumerable<byte>, ISafeHandled<ByteArray> {

		public static bool RENT_LARGE_BUFFERS = true;

		private readonly object locker = new object();

		private int offsetIncrement;

		protected ByteArray() {
			this.SafeHandledEntry = new SafeHandledEntry(this.PreDisposeSafeHandle, this.disposeLocker);
		}

		public bool IsExactSize {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => this.IsNull || (this.Length == this.Bytes.Length);
		}

		public Memory<byte> Memory {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ((Memory<byte>) this.Bytes).Slice(this.Offset, this.Length);
		}

		public Span<byte> Span {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ((Span<byte>) this.Bytes).Slice(this.Offset, this.Length);
		}

		public int Offset {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => this.offset + this.offsetIncrement;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected set {
				this.offset = value;
				this.ResetOffsetIncrement();
			}
		}

		public bool IsNull {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => this.Bytes == null;
		}

		public bool IsEmpty {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => this.IsNull || (this.Length == 0);
		}

		public bool HasData {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => !this.IsEmpty;
		}

		/// <summary>
		///     tells us if the entire array is all zeros (is cleared)
		/// </summary>
		public bool IsCleared {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				if(this.IsEmpty) {
					return true;
				}

				int longSize = this.Length >> 0x3;
				int remainder = this.Length & 0x7;

				int expandedLongSize = longSize * sizeof(long);

				if(longSize != 0) {

					unsafe {
						fixed(byte* longArrayB = this.Span.Slice(0, expandedLongSize)) {
							long* longArray = (long*) longArrayB;
							int length64 = longSize >> 0x3;
							int remainder64 = longSize & 0x7;

							int index = 0;

							if(length64 != 0) {

								// unroll groups of 64 bytes
								for(int i = 0; i < length64; i++) {

									index = i * 0x8;

									long flag = longArray[index];
									flag |= longArray[index + 1];
									flag |= longArray[index + 2];
									flag |= longArray[index + 3];
									flag |= longArray[index + 4];
									flag |= longArray[index + 5];
									flag |= longArray[index + 6];
									flag |= longArray[index + 7];

									if(flag != 0) {
										return false;
									}
								}

								index += 0x8;
							}

							if(remainder64 != 0) {

								int remainder32 = remainder64 & 0x4;
								int finalRemainder = remainder64 & 0x3;

								long flag = 0;

								if(remainder32 != 0) {
									flag = longArray[index];
									flag |= longArray[index + 1];
									flag |= longArray[index + 2];
									flag |= longArray[index + 3];

									if(flag != 0) {
										return false;
									}

									index += 0x4;
								}

								for(int i = finalRemainder; i != 0; i--) {
									flag |= longArray[(index + i) - 1];
								}

								if(flag != 0) {
									return false;
								}
							}
						}
					}
				}

				if(remainder == 0) {
					return true;
				}

				Span<byte> remainderSpan = stackalloc byte[sizeof(long)];
				this.Span.Slice(expandedLongSize, remainder).CopyTo(remainderSpan);

				TypeSerializer.Deserialize(remainderSpan, out long remainderLong);

				return remainderLong == 0;
			}
		}

		public int Length {
			get => this.length - this.offsetIncrement;
			protected set => this.length = value;
		}

		public byte[] Bytes {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				lock(this.locker) {
					return this.bytes;
				}
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected set {
				lock(this.locker) {
					this.bytes = value;
				}
			}
		}

		public byte this[int i] {
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

		public int CompareTo(byte[] other) {

			return this.Span.SequenceCompareTo(other);
		}

		public IEnumerator<byte> GetEnumerator() {

			return new ByteArrayEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return this.GetEnumerator();
		}

		public bool Equals(byte[] other) {
			return this.Span.SequenceEqual(other);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteArray Clone() {

			ByteArray slice = Create(this.Length);

			this.CopyTo(slice);

			return slice;
		}

		public SafeHandledEntry SafeHandledEntry { get; }
		public bool IsDisposed { get; protected set; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void ResetOffset() {
			this.offsetIncrement = 0;
			this.Offset = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ResetOffsetIncrement() {
			this.offsetIncrement = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IncreaseOffset(int amount) {
			this.offsetIncrement += amount;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Create(int size = 0) {

			if((size != 0) && (size < FixedAllocator.SMALL_SIZE)) {
				return CreateMappedArrayArray(size);
			}

			return CreateSimpleArray(size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Empty() {
			return Create();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray CreateSimpleArray(int size = 0) {

			SimpleByteArray entry = GetPooledSimpleArrayEntry();

			entry.SetSize(size);

			return entry;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static SimpleByteArray GetPooledSimpleArrayEntry() {

			//return SimpleByteArray.SimpleByteArrayPool.GetObject();
			return SimpleByteArray.CreatePooled();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ByteArray CreateMappedArrayArray(int size = 0) {

			return MappedByteArray.ALLOCATOR.Take(size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static WrapByteArray GetPooledWrapArrayEntry() {

			//return WrapArrayEntry.WrapArrayEntryPool.GetObject();
			return WrapByteArray.CreatePooled();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray CreateLargeBuffer(int size) {

			SimpleByteArray entry = GetPooledSimpleArrayEntry();

			entry.SetSize(size, true);

			return entry;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Create(ByteArray array) {
			ByteArray copy = Create(array?.Length ?? 0);

			copy.CopyFrom(array);

			return copy;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Create(byte[] array) {
			return Create(array, array?.Length ?? 0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Create(byte[] array, int length) {

			return Create(array, 0, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static ByteArray Create(byte[] array, int offset, int length) {
			SimpleByteArray entry = GetPooledSimpleArrayEntry();

			entry.SetArray(array, offset, length);

			return entry;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Wrap(ByteArray array) {
			WrapByteArray entry = GetPooledWrapArrayEntry();

			entry.SetArray(array, false);

			return entry;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray WrapAndOwn(byte[] array) {
			return Wrap(array, true);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Wrap(byte[] array, bool takeOwnership = false) {
			return Wrap(array, 0, array?.Length ?? 0, takeOwnership);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Wrap(byte[] array, int length, bool takeOwnership) {
			return Wrap(array, 0, length, takeOwnership);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Wrap(byte[] array, int offset, int length, bool takeOwnership) {
			WrapByteArray entry = GetPooledWrapArrayEntry();

			entry.SetArray(array, offset, length, takeOwnership);

			return entry;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray CreateClone(ref byte[] array) {
			return CreateClone(array, array?.Length ?? 0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray CreateClone(byte[] array) {
			return CreateClone(array, array?.Length ?? 0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ByteArray CreateClone(byte[] array, int length) {

			return CreateClone(array, 0, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ByteArray CreateClone(byte[] array, int offset, int length) {

			return Create(array, offset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Create(in RecyclableMemoryStream stream) {

			byte[] byffer = stream.GetBuffer();

			ByteArray newEntry = Create((int) stream.Length);

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

		public string ToBase58() {

			return new Base58().Encode(this);
		}

		public string ToBase85() {

			return new Base85().Encode(this);
		}

		public string ToBase94() {

			return new Base94().Encode(this);
		}

		public string ToBase64() {

			return Convert.ToBase64String(this.Span);
		}

		/// <summary>
		///     Make a copy with the exact size of the expected array. no rented data
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte[] ToExactByteArrayCopy() {

			byte[] buffer = new byte[this.Length];

			this.CopyTo(ref buffer);

			return buffer;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte[] ToExactByteArray() {

			if(this.IsExactSize && (this.Offset == 0)) {
				return this.Bytes;
			}

			return this.ToExactByteArrayCopy();
		}

		/// <summary>
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		/// <remarks>we dont take ownership by default</remarks>

		//public static implicit operator ByteArray(byte[] data) => Wrap(data);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(ByteArray other) {

			return this.Span.SequenceCompareTo(other.Span);
		}

		public override int GetHashCode() {

			return this.Bytes != null ? this.Bytes.GetHashCode() : 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj) {

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Expand(ByteArray src, int expandBy) {

			ByteArray dest = Create(src.Length + expandBy);

			dest.CopyFrom(src);

			return dest;
		}

		//Im disabling this for now, its too confusing. Call either Bytes for the full array, ToExactByteArray() to get a compromise and ToExactByteArrayCopy to get a full copy.
		//		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//		public static explicit operator byte[](ByteArrayBase baw) {
		//			// no choice, otherwise we risk returning an array that is bigger than what is expect if it is rented.
		//			return baw.ToExactByteArray();
		//		}

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <remarks>This method is VERy slow, do not use in critical path</remarks>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> CastedArray<T>()
			where T : struct {

			return CastArray<T>(this.Span);
		}

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <remarks>This method is VERy slow, do not use in critical path</remarks>
		/// <returns></returns>
		public static Span<T> CastArray<T>(Span<byte> source)
			where T : struct {
			if((source.Length % Marshal.SizeOf<T>()) != 0) {
				throw new ApplicationException($"Not enough memory to cast to array of type {typeof(T)}");
			}

			return MemoryMarshal.Cast<byte, T>(source);
		}

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <remarks>This method is VERy slow, do not use in critical path</remarks>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T ReadCasted<T>(int index)
			where T : struct {

			return this.CastedArray<T>()[index];
		}

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <remarks>This method is VERy slow, do not use in critical path</remarks>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteCasted<T>(int index, T value)
			where T : struct {

			this.CastedArray<T>()[index] = value;
		}

		public ByteArray Slice(int offset, int length) {

			ByteArray slice = Create(length);

			this.CopyTo(slice, offset, 0, length);

			return slice;
		}

		public ByteArray Slice(int offset) {

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

			if(this.HasData) {
				this.Span.Clear();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(int offset, int length) {

			if(this.HasData) {
				this.Span.Slice(offset, length).Clear();
			}
		}

		public void FillSafeRandom() {

			GlobalRandom.GetNextBytes(this.Bytes, this.Offset, this.Length);
		}

		public void Save(string filename) {
			File.WriteAllBytes(filename, this.ToExactByteArray());
		}

		public int GetArrayHash() {

			xxHasher32 hasher = new xxHasher32();

			return hasher.Hash(this.ToExactByteArray());
		}

		public string ToBase30() {

			return new Base30().Encode(this);
		}

		public string ToBase32() {

			return Base32.Encode(this);
		}

		public string ToBase35() {

			return new Base35().Encode(this);
		}

		public static SafeArrayHandle FromBase30(string value) {
			return new Base30().Decode(value);
		}

		public static SafeArrayHandle FromBase32(string value) {
			return Base32.Decode(value);
		}

		public static SafeArrayHandle FromBase35(string value) {
			return new Base35().Decode(value);
		}

		public static SafeArrayHandle FromBase58(string value) {
			return new Base58().Decode(value);
		}

		public static SafeArrayHandle FromBase64(string value) {
			return WrapAndOwn(Convert.FromBase64String(value));
		}

		public static SafeArrayHandle FromBase85(string value) {
			return new Base85().Decode(value);
		}

		public static SafeArrayHandle FromBase94(string value) {
			return new Base94().Decode(value);
		}

#if DEBUG

#endif

	#region Disposable

		public Action<ByteArray> Disposed { get; set; }

		public void Return() {
			this.Dispose();
		}

		public void Dispose() {
			this.Dispose(true);
		}

		private readonly object disposeLocker = new object();
		private byte[] bytes;
		private int offset;
		private int length;

		private void Dispose(bool disposing) {

			lock(this.disposeLocker) {
				if(!this.IsDisposed) {
					if(!disposing) {
						// this must be the last operation, as once disposed, it will go on for it's next life...
						this.SafeHandledEntry.Reset();
					}

					this.SafeHandledEntry.Dispose(disposing);

					if(!disposing && (this.Bytes != null)) {
						// we force it!, this is a finalizer
						this.DisposeSafeHandle(false);
					}
				} else if(this.Bytes != null) {
					throw new ApplicationException("Byte array was not disposed!");
				}

			}
		}

		protected virtual void DisposeClear() {
			this.Clear();
		}

		private void PreDisposeSafeHandle(bool disposing) {

			lock(this.disposeLocker) {

				if(!this.IsDisposed) {

					this.DisposeClear();

					if(this.Disposed != null) {
						this.Disposed(this);
					}

					this.Disposed = null;

					this.DisposeSafeHandle(disposing);

					this.SuppressFinalize(disposing);
				}

				this.IsDisposed = true;
			}
		}

		protected virtual void SuppressFinalize(bool disposing) {
			if(disposing) {
				GC.SuppressFinalize(this);
			}
		}

		protected abstract void DisposeSafeHandle(bool disposing);

		~ByteArray() {
			this.Dispose(false);
		}

	#endregion

	}
}