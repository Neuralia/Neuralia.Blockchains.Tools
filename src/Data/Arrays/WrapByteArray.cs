using System.Runtime.CompilerServices;

namespace Neuralia.Blockchains.Tools.Data.Arrays {
	/// <summary>
	/// a simple class that wraps an existing array without clearing it on dispose
	/// </summary>
	internal class WrapByteArray : ByteArray {

		//internal static readonly SecureObjectPool<WrapByteArray> WrapByteArrayPool = new SecureObjectPool<WrapByteArray>(CreatePooled);

		private bool takeOwnership = false;
		
		internal static WrapByteArray CreatePooled() {
			return new WrapByteArray();
		}
		
		private WrapByteArray() {

		}
		
		public WrapByteArray(ByteArray data, bool takeOwnership)  {
			this.SetArray(data,takeOwnership);
		}
		
		public WrapByteArray(byte[] data, bool takeOwnership) : this(data, data.Length, takeOwnership){

		}
		
		public WrapByteArray(byte[] data, int length, bool takeOwnership)  : this(data, 0, length, takeOwnership){

		}
		public WrapByteArray(byte[] data, int offset, int length, bool takeOwnership) {
			this.SetArray(data, offset, length, takeOwnership);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetArray(ByteArray data, bool takeOwnership) {
			this.SetArray(data?.Bytes, data?.Offset??0, data?.Length??0, takeOwnership);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetArray(byte[] data, bool takeOwnership) {
			this.SetArray(data, data?.Length??0, takeOwnership);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetArray(byte[] data, int length, bool takeOwnership) {
			this.SetArray(data, 0, length, takeOwnership);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetArray(byte[] data, int offset, int length, bool takeOwnership) {
			this.Bytes = data;
			this.Length = length;
			this.Offset = offset;
			this.takeOwnership = takeOwnership;
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
		public bool Equals(WrapByteArray other) {

			return this == other;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(WrapByteArray array1, ByteArray array2) {

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
		public static bool operator !=(WrapByteArray array1, ByteArray array2) {
			return !(array1 == array2);
		}

		protected override void DisposeClear() {
			// do nothing if we dont have ownership, we dont clear the underlying array. otherwise we do
			if(this.takeOwnership) {
				base.Clear();
			}
		}

		protected override void DisposeSafeHandle(bool disposing) {

			this.Bytes = null;
			this.Length = 0;
			this.ResetOffset();
		}
	}
}