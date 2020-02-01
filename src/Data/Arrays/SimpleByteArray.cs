using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Neuralia.Blockchains.Tools.Data.Arrays {

	/// <summary>
	///     An improved wrapper around byte arIsEmptyrays. Will borrow memory when the size warrants it.
	/// </summary>
	/// <remarks>
	///     BE CAREFUL!!!  borrowed memory can return arrays that are bigger than our expected size. passing the byte
	///     array directly to methods that read it to the end can be very dangerous. always try to use version that allow to
	///     limit the reading with a length parameter.
	/// </remarks>
	public class SimpleByteArray : ByteArray {

		//internal static readonly SecureObjectPool<SimpleByteArray> SimpleByteArrayPool = new SecureObjectPool<SimpleByteArray>(CreatePooled);

		internal static SimpleByteArray CreatePooled() {
			return new SimpleByteArray();
		}

		private bool IsRented { get; set; }
		
		private SimpleByteArray() : this(0) {

		}

		private SimpleByteArray(int length) {

			this.SetSize(length);
		}

		public SimpleByteArray(byte[] data) : this(data, data.Length) {
		}

		public SimpleByteArray(ByteArray data) : this(data.Length) {
			data.CopyTo(this);
		}

		public SimpleByteArray(byte[] data, int length) : this(data, 0, length) {

		}

		public SimpleByteArray(byte[] data, int offset, int length) {

			this.SetArray(data, offset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetArray(byte[] data) {
			this.SetArray(data, data?.Length??0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetArray(byte[] data, int length) {
			this.SetArray(data, 0, data?.Length??0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetArray(byte[] data, int offset, int length) {

			this.SetSize(length);
			this.CopyFrom(data.AsSpan(), offset, length);
			
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetSize(int length, bool forceLargeBuffer = false) {

			if(this.IsDisposed) {
				throw new ApplicationException();
			}

			this.Reset();
			
			if(this.Bytes != null && this.HasData) {
				throw new ApplicationException("Array is already set.");
			}

			this.IsRented = false;

			if(length == 0) {
				this.Bytes = new byte[length];
			} else if(length > FixedAllocator.SMALL_SIZE && (ByteArray.RENT_LARGE_BUFFERS || forceLargeBuffer)) {
				this.Bytes = ArrayPool<byte>.Shared.Rent(length);
				this.IsRented = true;
			} else {
				this.Bytes = new byte[length];
			}

			this.Length = length;
			this.Offset = 0;

			this.Clear();
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
		public bool Equals(SimpleByteArray other) {

			return this == other;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(SimpleByteArray array1, ByteArray array2) {

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SimpleByteArray array1, ByteArray array2) {
			return !(array1 == array2);
		}

		private void Reset() {
			this.ResetOffsetIncrement();
			
			if(this.IsRented && this.Bytes != null) {
				ArrayPool<byte>.Shared.Return(this.Bytes);
			}

			this.IsRented = false;
			this.Bytes = null;
			this.Length = 0;
			this.ResetOffset();
		}

		protected override void DisposeSafeHandle(bool disposing) {

			this.Reset();

			// if(disposing) {
			// 	// if explicit disposing, go back to the pool. if its a destructor, nothing to do, let it die
			// 	SimpleByteArrayPool.PutObject(this);
			// }
		}
	}
}