using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.IO;
using Neuralia.Blockchains.Tools.Cryptography;
using Neuralia.Blockchains.Tools.Cryptography.Encodings;
using Neuralia.Blockchains.Tools.Data.Allocation;

namespace Neuralia.Blockchains.Tools.Data {

	/// <summary>
	///     An improved wrapper around byte arIsEmptyrays. Will borrow memory when the size warrants it.
	/// </summary>
	/// <remarks>
	///     BE CAREFUL!!!  borrowed memory can return arrays that are bigger than our expected size. passing the byte
	///     array directly to methods that read it to the end can be very dangerous. always try to use version that allow to
	///     limit the reading with a length parameter.
	/// </remarks>
	[DebuggerDisplay("{HasData?Bytes[Offset].ToString():\"null\"}, {HasData?Bytes[Offset+1].ToString():\"null\"}, {HasData?Bytes[Offset+2].ToString():\"null\"}")]
	public sealed class ByteArray : IByteArray {

		public enum BaseFormat {
			Base64,
			Base58,
			Base32
		}

		public ByteArray() : this(0) {

		}

		public ByteArray(int length) {

			// buig objects are 85000, but benchmarks show that at about 1200 bytes, the speed is the same between the pool and an instanciation
			if(length < 1200) {
				this.Bytes = new byte[length];
			} else {
				this.Bytes = ArrayPool<byte>.Shared.Rent(length);
				this.IsRented = true;
			}

			this.Length = length;
		}

		public ByteArray(byte[] data) : this(data, data.Length) {
		}

		public ByteArray(IByteArray data) : this(data.Length) {
			data.CopyTo(this);
		}

		public ByteArray(byte[] data, int length) : this(data, 0, length) {

		}

		public ByteArray(byte[] data, int offset, int length) {
			this.Bytes = data;
			this.Length = length;
			this.Offset = offset;
		}

		public bool IsRented { get;private set; }

		public bool IsExactSize => this.IsNull || (this.Length == this.Bytes.Length);

		public Memory<byte> Memory => ((Memory<byte>) this.Bytes).Slice(this.Offset, this.Length);

		public Span<byte> Span => ((Span<byte>) this.Bytes).Slice(this.Offset, this.Length);

		public int Offset { get; }

		public void Return() {
			if(this.IsRented && this.Bytes != null) {
				ArrayPool<byte>.Shared.Return(this.Bytes);
			}

			this.IsRented = false;
			this.Bytes = null;
			this.Length = 0;
		}

		public bool IsNull => this.Bytes == null;

		public bool IsEmpty => this.IsNull || (this.Length == 0);
		public bool IsActive => !this.IsDisposed && this.HasData;

		public bool HasData => !this.IsEmpty;

		public int Length { get; private set;}

		public byte[] Bytes { get; private set; }

		public byte this[int i] {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => this.Bytes[i];
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => this.Bytes[i] = value;
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
			return Convert.ToBase64String(this.Bytes, this.Offset, this.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IByteArray Clone() {
			return CreateFrom(this);
		}

		/// <summary>
		///     Make a copy with the exact size of the expected array. no rented data
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte[] ToExactByteArrayCopy() {

			var buffer = new byte[this.Length];

			this.CopyTo(ref buffer);

			return buffer;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte[] ToExactByteArray() {

			if(this.IsExactSize) {
				return this.Bytes;
			}

			return this.ToExactByteArrayCopy();
		}

		public int CompareTo(IByteArray other) {
			return this.Span.SequenceCompareTo(other.Span);
		}

		public IEnumerator<byte> GetEnumerator() {
			return new ByteArrayEnumerator<byte, IByteArray>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj) {

			if(obj == null) {
				return this.IsEmpty;
			}

			if(obj is IByteArray array) {
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

		//Im disabling this for now, its too confusing. Call either Bytes for the full array, ToExactByteArray() to get a compromise and ToExactByteArrayCopy to get a full copy.
		//		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//		public static explicit operator byte[](ByteArray baw) {
		//			// no choice, otherwise we risk returning an array that is bigger than what is expect if it is rented.
		//			return baw.ToExactByteArray();
		//		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> CastedArray<T>()
			where T : struct {
			if((this.Length % Marshal.SizeOf<T>()) != 0) {
				throw new ApplicationException($"Not enough memory to cast to array of type {typeof(T)}");
			}

			return MemoryMarshal.Cast<byte, T>(this.Memory.Span);
		}

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

		public IByteArray Slice(int offset, int length) {
			ByteArray slice = new ByteArray(length);

			this.CopyTo(slice, offset, 0, length);

			return slice;
		}

		public IByteArray Slice(int offset) {
			return this.Slice(offset, this.Length - offset);
		}

		/// <summary>
		///     Slice the contents, but return a reference to the inner data. copies no data
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public IByteArray SliceReference(int offset, int length) {

			return new ByteArray(this.Bytes, this.Offset + offset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(in Span<byte> dest, int srcOffset, int destOffset, int length) {
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
		public void CopyTo(IByteArray dest, int srcOffset, int destOffset, int length) {
			this.CopyTo(dest.Span, srcOffset, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(IByteArray dest, int destOffset) {
			this.CopyTo(dest, 0, destOffset, this.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(IByteArray dest) {
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
		public void CopyFrom(IByteArray src, int srcOffset, int destOffset, int length) {
			this.CopyFrom(src.Span, srcOffset, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(IByteArray src, int srcOffset, int length) {

			this.CopyFrom(src.Span, srcOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(IByteArray src, int destOffset) {

			this.CopyFrom(src.Span, destOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(IByteArray src) {

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
			if(this.Bytes != null) {
				this.Span.Clear();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(int offset, int length) {
			this.Span.Slice(0, length).Clear();
		}

		public void FillSafeRandom() {

			GlobalRandom.GetNextBytes(this.Bytes, this.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(IByteArray other) {
			return this == other;
		}

		public int GetArrayHash() {
			return MemoryAllocators.xxHash32.Hash(this.ToExactByteArray());
		}

		public string ToBase30() {
			return new Base30().Encode(this);
		}

		public string ToBase32() {
			return new Base32().Encode(this);
		}

		public string ToBase35() {
			return new Base35().Encode(this);
		}

		public int CompareTo(byte[] other) {
			return this.Span.SequenceCompareTo(other);
		}

		public bool Equals(byte other) {
			return false;
		}

		public static IByteArray FromBase30(string value) {
			return new Base30().Decode(value);
		}

		public static IByteArray FromBase32(string value) {
			return new Base32().Decode(value);
		}

		public static IByteArray FromBase35(string value) {
			return new Base35().Decode(value);
		}

		public static IByteArray FromBase58(string value) {
			return new Base58().Decode(value);
		}

		public static IByteArray FromBase64(string value) {
			return (ByteArray) Convert.FromBase64String(value);
		}

		public static IByteArray FromBase85(string value) {
			return new Base85().Decode(value);
		}

		public static IByteArray FromBase94(string value) {
			return new Base94().Decode(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(ByteArray dest, int srcOffset, int destOffset, int length) {
			this.CopyTo((IByteArray) dest, srcOffset, destOffset, length);
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
		public void CopyFrom(ByteArray src, int srcOffset, int destOffset, int length) {
			this.CopyFrom(src.Span, srcOffset, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ByteArray src, int srcOffset, int length) {
			this.CopyFrom(src, srcOffset, 0, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ByteArray src, int destOffset) {
			this.CopyFrom(src, 0, destOffset, src.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ByteArray src) {
			this.CopyFrom(src, 0, 0, src.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(ByteArray array1, IByteArray array2) {

			if(ReferenceEquals(array1, null)) {
				return ReferenceEquals(array2, null);
			}

			if(ReferenceEquals(array2, null)) {
				return ReferenceEquals(array1, null) || array1.IsEmpty;
			}

			if(ReferenceEquals(array1, array2)) {
				return true;
			}

			return array1.Span.SequenceEqual(array2.Span);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(ByteArray array1, IByteArray array2) {
			return !(array1 == array2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ByteArray(byte[] other) {
			if(other == null) {
				return null;
			}

			return new ByteArray(other);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Expand(ByteArray src, int expandBy) {

			ByteArray dest = new ByteArray(src.Length + expandBy);

			dest.CopyFrom(src);

			return dest;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray CreateFrom(in ReadOnlySpan<byte> src) {

			ByteArray dest = new ByteArray(src.Length);

			dest.CopyFrom(src);

			return dest;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray CreateFrom(in RecyclableMemoryStream stream) {

			var byffer = stream.GetBuffer();

			return CreateFrom(ref byffer, (int) stream.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray CreateFrom(ref byte[] src, int length) {

			ReadOnlySpan<byte> temp = src;

			return CreateFrom(temp.Slice(0, length));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray CreateFrom(ref byte[] src) {

			return CreateFrom((ReadOnlySpan<byte>) src);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray CreateFrom(IByteArray src) {

			return CreateFrom(src.Span);
		}

	#region Disposable

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			if(!this.IsDisposed) {
				if(disposing) {
					
				}
				// return it no matter what
				try {
					this.Return();

				} catch {
						
				}
			}
			
			this.IsDisposed = true;
		}

		~ByteArray() {
			this.Dispose(false);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return this.GetEnumerator();
		}

		public bool IsDisposed { get; private set; }

	#endregion

	}
}