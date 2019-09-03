using System;
using System.Runtime.CompilerServices;

namespace Neuralia.Blockchains.Tools.Data.Allocation {
	public class MemoryBlockDoubleArray : MemoryBlock<IByteArray, MemoryBlockDoubleArray, MemoryBlockDoubleArray> {

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public new void Return() {
			if(this.IsActive) {
				// empty our contents
				for(int i = 0; i < this.Length; i++) {
					IByteArray entry = this[i];

					entry?.Return();
				}

				base.Return();
			}
		}

		/// <summary>
		///     Here we return the double array, but not the inner arrays. to use with CloneShallow
		/// </summary>
		public void ReturnShallow() {
			if(this.IsActive) {
				// we do not return our contents
				base.Return();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override Span<T> CastedArray<T>() {
			throw new NotImplementedException();
		}

		public override MemoryBlockDoubleArray Clone() {
			MemoryBlockDoubleArray other = this.allocator?.Take(this.Length);
			other?.CopyFrom(this);

			for(int i = 0; i < this.Length; i++) {
				other[i] = this[i].Clone();
			}

			return other;
		}

		public MemoryBlockDoubleArray CloneShallow() {
			MemoryBlockDoubleArray other = this.allocator?.Take(this.Length);
			other?.CopyFrom(this);

			return other;
		}

		protected override MemoryBlockDoubleArray GetAllocatedEntry(int length) {
			return MemoryAllocators.Instance.doubleArrayCryptoAllocator.Take(length);
		}

		public override MemoryBlockDoubleArray SliceReference(int offset, int length) {
			throw new NotImplementedException();
		}

		public override byte[] ToExactByteArrayCopy() {
			throw new NotImplementedException();
		}

		public override byte[] ToExactByteArray() {
			throw new NotImplementedException();
		}

		public override void FillSafeRandom() {
			throw new NotImplementedException();
		}

		public override string ToBase85() {
			throw new NotImplementedException();
		}

		public override string ToBase94() {
			throw new NotImplementedException();
		}

		public override string ToBase64() {
			throw new NotImplementedException();
		}

		public override string ToBase58() {
			throw new NotImplementedException();
		}

		public override string ToBase30() {
			throw new NotImplementedException();
		}

		public override string ToBase32() {
			throw new NotImplementedException();
		}

		public override string ToBase35() {
			throw new NotImplementedException();
		}

		/// <summary>
		///     in this version, we dont clear the content. ,most probably it was used
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReturnNoClear() {
			if(this.IsActive) {

				base.Return();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(IByteArray dest, int srcOffset, int destOffset, int length) {
			throw new NotImplementedException();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(IByteArray dest, int destOffset) {
			throw new NotImplementedException();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(IByteArray dest) {
			throw new NotImplementedException();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(IByteArray src, int srcOffset, int destOffset, int length) {
			throw new NotImplementedException();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(IByteArray src, int srcOffset, int length) {
			throw new NotImplementedException();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(IByteArray src, int destOffset) {
			throw new NotImplementedException();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(IByteArray src) {
			throw new NotImplementedException();
		}

		public static bool operator ==(MemoryBlockDoubleArray a, MemoryBlockDoubleArray b) {
			if(ReferenceEquals(null, a)) {
				return ReferenceEquals(null, b);
			}

			return a.Equals(b);
		}

		public static bool operator !=(MemoryBlockDoubleArray a, MemoryBlockDoubleArray b) {
			return !(a == b);
		}
	}
}