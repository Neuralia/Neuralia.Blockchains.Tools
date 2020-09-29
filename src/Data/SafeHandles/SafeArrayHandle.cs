using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.IO;
using Neuralia.Blockchains.Tools.Data.Arrays;
using Neuralia.Blockchains.Tools.Data.Pools;

namespace Neuralia.Blockchains.Tools.Data {
	[DebuggerDisplay("{HasData?Bytes[Offset].ToString():\"null\"}, {HasData?Bytes[Offset+1].ToString():\"null\"}, {HasData?Bytes[Offset+2].ToString():\"null\"}")]
	public class SafeArrayHandle : SafeHandle<ByteArray, SafeArrayHandle>, IPoolEntry, IComparable<SafeArrayHandle>,  IComparable<byte[]>,  IComparable<ByteArray> {

		static SafeArrayHandle() {
			creators.Add(typeof(SafeArrayHandle), SafeArrayHandle.Create);
		}
		internal static readonly SecureObjectPool<SafeArrayHandle> HandlePool = new SecureObjectPool<SafeArrayHandle>(CreatePooled);
		public byte this[int i] {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {

				lock(this.locker) {
					return this.Entry[i];
				}
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set {

				lock(this.locker) {
					this.Entry[i] = value;
				}
			}
		}

		public int Length {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => this.Entry?.Length ?? 0;
		}

		public Memory<byte> Memory => this.Entry?.Memory ?? Array.Empty<byte>();
		public Span<byte> Span => this.Entry.Span;
		public byte[] Bytes => this.Entry?.Bytes;
		public int Offset => this.Entry?.Offset ?? 0;
		public bool HasData => this.Entry?.HasData ?? false;
		public bool IsEmpty => this.Entry?.IsEmpty ?? true;
		public bool IsZero => this.Entry?.IsCleared ?? true;
		public bool IsNull => this.Entry == null;
		public bool IsNotNull => this.Entry != null;
		public bool IsCleared => this.Entry?.IsCleared ?? true;

		public byte[] ToExactByteArray() {

			return this.Entry?.ToExactByteArray();
		}

		public byte[] ToExactByteArrayCopy() {

			return this.Entry?.ToExactByteArrayCopy();
		}

		internal static SafeArrayHandle CreatePooled() {
			return new SafeArrayHandle();
		}
		
		public static new SafeArrayHandle Create() {
			return HandlePool.GetObject();
		}
		
		public static SafeArrayHandle Create(byte[] array) {
			if(array == null) {
				return null;
			}
			return (SafeArrayHandle) ByteArray.Create(array);
		}
		
		public static SafeArrayHandle Create(int size) {
			return Create().SetSize(size);
		}

		public static explicit operator SafeArrayHandle(ByteArray data) {
			if(data == null) {
				return null;
			}
			return (SafeArrayHandle) Create().SetData(data);
		}
		
		public static explicit operator SafeArrayHandle(byte[] data) {
			if(data == null) {
				return null;
			}
			return (SafeArrayHandle)ByteArray.Wrap(data);
		}

		public static SafeArrayHandle Create(byte[] array, int length) {
			if(array == null) {
				return null;
			}
			return (SafeArrayHandle) ByteArray.Create(array, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafeArrayHandle Create(in RecyclableMemoryStream stream) {
			return (SafeArrayHandle) ByteArray.Create(stream);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafeArrayHandle Create(in SafeArrayHandle safeArrayHandle) {
			return (SafeArrayHandle) ByteArray.Create(safeArrayHandle);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafeArrayHandle Empty() {
			return (SafeArrayHandle) ByteArray.Empty();
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafeArrayHandle CreateClone(ref byte[] array) {
			return (SafeArrayHandle) ByteArray.CreateClone(array);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafeArrayHandle CreateClone(byte[] array) {
			return (SafeArrayHandle) ByteArray.CreateClone(array);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static SafeArrayHandle CreateClone(byte[] array, int length) {

			return (SafeArrayHandle) ByteArray.CreateClone(array, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static SafeArrayHandle CreateClone(byte[] array, int offset, int length) {
			return (SafeArrayHandle) ByteArray.CreateClone(array, offset, length);
		}

		public static SafeArrayHandle Wrap(byte[] data) {

			return Wrap(ByteArray.Wrap(data));
		}
		
		public static SafeArrayHandle Wrap(ByteArray data) {
			var entry = Create();
			// make sure we do not own this data so we do not clear it
			entry.SetEntry(data, false);
			return entry;
		}
		
		public static SafeArrayHandle WrapAndOwn(byte[] data) {
			return (SafeArrayHandle) ByteArray.WrapAndOwn(data);
		}
		
		public static SafeArrayHandle WrapAndOwn(ByteArray data) {
			return (SafeArrayHandle) data;
		}
		
		public SafeArrayHandle SetSize(int size) {

			this.Entry = ByteArray.Create(size);

			return this;
		}
		
		public void FillSafeRandom() {

			this.Entry.FillSafeRandom();
		}
		
		public void FillIncremental() {

			this.Entry.FillIncremental();
		}
		
		public static SafeArrayHandle FromBase94(string data) {
			return (SafeArrayHandle) ByteArray.FromBase94(data);
		}
		
		public static SafeArrayHandle FromBase64(string data) {
			return (SafeArrayHandle) ByteArray.FromBase64(data);
		}
		
		public static SafeArrayHandle FromBase58(string data) {
			return (SafeArrayHandle) ByteArray.FromBase58(data);
		}
		
		public static SafeArrayHandle FromBase32(string data) {
			return (SafeArrayHandle) ByteArray.FromBase32(data);
		}
		
		public static SafeArrayHandle FromBase30(string data) {
			return (SafeArrayHandle) ByteArray.FromBase30(data);
		}

		public string ToBase30() {
			return this.Entry.ToBase30();
		}
		
		public string ToBase32() {
			return this.Entry.ToBase32();
		}
		
		public string ToBase58() {
			return this.Entry.ToBase58();
		}
		
		public string ToBase64() {
			return this.Entry.ToBase64();
		}
		public string ToBase94() {
			return this.Entry.ToBase94();
		}

	
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual void CopyTo(in Span<byte> dest, int srcOffset, int destOffset, int length) {

			this.Entry.CopyTo( dest, srcOffset, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(in Span<byte> dest, int destOffset) {
			this.Entry.CopyTo( dest, destOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(in Span<byte> dest) {
			this.Entry.CopyTo( dest);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(ref byte[] dest, int srcOffset, int destOffset, int length) {
			this.Entry.CopyTo( dest, srcOffset, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(ref byte[] dest, int destOffset) {
			this.Entry.CopyTo( dest, destOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(ref byte[] dest) {
			this.Entry.CopyTo( dest);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(ByteArray dest, int srcOffset, int destOffset, int length) {
			this.Entry.CopyTo( dest, srcOffset, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(ByteArray dest, int destOffset) {
			this.Entry.CopyTo( dest, destOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(ByteArray dest) {
			this.Entry.CopyTo( dest);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(SafeArrayHandle dest, int srcOffset, int destOffset, int length) {
			this.Entry.CopyTo( dest.Entry, srcOffset, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(SafeArrayHandle dest, int destOffset) {
			this.Entry.CopyTo( dest.Entry, destOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(SafeArrayHandle dest) {
			this.Entry.CopyTo( dest.Entry);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySpan<byte> src, int srcOffset, int destOffset, int length) {

			this.Entry.CopyFrom( src, srcOffset, destOffset, length);;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySpan<byte> src, int srcOffset, int length) {
			this.Entry.CopyFrom( src, srcOffset, srcOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySpan<byte> src, int destOffset) {
			this.Entry.CopyFrom( src, destOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySpan<byte> src) {
			this.Entry.CopyFrom( src);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ref byte[] src, int srcOffset, int destOffset, int length) {
			this.Entry.CopyFrom( src, srcOffset, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ref byte[] src, int srcOffset, int length) {
			this.Entry.CopyFrom( src, srcOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ref byte[] src, int destOffset) {
			this.Entry.CopyFrom( src, destOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ref byte[] src) {
			this.Entry.CopyFrom( src);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ByteArray src, int srcOffset, int destOffset, int length) {
			this.Entry.CopyFrom( src, srcOffset, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ByteArray src, int srcOffset, int length) {

			this.Entry.CopyFrom( src, srcOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ByteArray src, int destOffset) {

			this.Entry.CopyFrom( src, destOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ByteArray src) {

			this.Entry.CopyFrom( src);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(SafeArrayHandle src, int srcOffset, int destOffset, int length) {
			this.Entry.CopyFrom( src.Entry, srcOffset, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(SafeArrayHandle src, int srcOffset, int length) {

			this.Entry.CopyFrom( src.Entry, srcOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(SafeArrayHandle src, int destOffset) {

			this.Entry.CopyFrom( src.Entry, destOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(SafeArrayHandle src) {

			this.Entry.CopyFrom( src.Entry);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySequence<byte> src, int srcOffset, int destOffset, int length) {

			this.Entry.CopyFrom( src, srcOffset, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySequence<byte> src, int srcOffset, int length) {
			this.Entry.CopyFrom( src, srcOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySequence<byte> src, int destOffset) {
			this.Entry.CopyFrom( src, destOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySequence<byte> src) {
			this.Entry.CopyFrom( src);
		}

		public void Clear() {
			this.Entry?.Clear();
		}
		
		public void SafeDispose() {
			if(!this.IsDisposed && !this.IsNull) {
				this.Entry.Clear();
				this.Dispose();
			}
		}
		public override void Dispose() {
			// lets return the entry to the pool
			if(this.PoolEntry.Stored) {
				return;
			}
			this.Entry = null;
			HandlePool.PutObject(this);
		}

		public PoolEntry PoolEntry { get; } = new PoolEntry();
		
		
		public int CompareTo(byte[] other) {

			//TODO: check for null
			return this.Span.SequenceCompareTo(other);
		}

		public int CompareTo(SafeArrayHandle other) {
			return this.Entry.CompareTo(other.Entry);
		}

		public int CompareTo(ByteArray other) {
			return this.Entry.CompareTo(other);
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
				return this.Entry == array;
			}
			
			if(obj is SafeArrayHandle safeArray) {
				return this == safeArray;
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
		public static bool operator ==(SafeArrayHandle array1, SafeArrayHandle array2) {

			if(ReferenceEquals(array1, null)) {
				return ReferenceEquals(array2, null);
			}

			if(ReferenceEquals(array2, null)) {
				return array1.IsNull;
			}

			if(ReferenceEquals(array1, array2)) {
				return true;
			}

			return array1.Entry == array2.Entry;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SafeArrayHandle array1, SafeArrayHandle array2) {
			return !(array1 == array2);
		}
	}
}