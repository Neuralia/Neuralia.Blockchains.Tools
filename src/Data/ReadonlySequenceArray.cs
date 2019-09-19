using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Neuralia.Blockchains.Tools.Data {

	/// <summary>
	///     A special class to handle sequences sent by network pipelines
	/// </summary>
	public class ReadonlySequenceArray : ByteArray, IEquatable<ReadonlySequenceArray>, IComparable<ReadOnlySequence<byte>> {

		public ReadonlySequenceArray(in ReadOnlySequence<byte> buffer) {
			this.Sequence = buffer;
		}

		public ReadOnlySequence<byte> Sequence { get; }
		

		public override ByteArray SliceReference(int offset, int length) {
			throw new NotImplementedException();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void CopyTo(in Span<byte> dest, int srcOffset, int destOffset, int length) {

			this.Sequence.Slice(srcOffset, length).CopyTo(dest.Slice(destOffset, length));
		}

		protected override void DisposeSafeHandle(bool disposing) {
		}
		
		public bool Equals(ReadonlySequenceArray other) {
			throw new NotImplementedException();
		}

		public int CompareTo(ReadOnlySequence<byte> other) {
			throw new NotImplementedException();
		}
	}
}