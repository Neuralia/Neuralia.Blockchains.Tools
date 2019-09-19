using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Neuralia.Blockchains.Tools.Data;

namespace Neuralia.Blockchains.Tools.Serialization {

	/// <summary>
	///     A special extendable array which uses segments to avoid to copy over memory while growing
	/// </summary>
	/// <remarks>
	///     very quick testing shows that this class performs pretty well with large objects. perhaps it will warrant some
	///     development in the future.
	/// </remarks>
	/// <remarks>
	///     The copying of data is expensive, because of segment logic. This class should be used for simply buffer grow
	///     over time logic only
	/// </remarks>
	public class SequenceBuffer : IDisposable2 {

		private readonly List<byte[]> rentedBuffers = new List<byte[]>();
		private readonly SequenceSegment<byte> startSegment;

		private long position;
		private ReadOnlySequence<byte> sequence;

		public SequenceBuffer() : this(512) {

		}

		public SequenceBuffer(int length) {
			var bytes = this.GetBuffer(length);

			this.startSegment = new SequenceSegment<byte>(bytes);
			this.sequence = new ReadOnlySequence<byte>(this.startSegment, 0, this.startSegment, this.startSegment.Memory.Length);
		}

		public long Position {
			get => this.position;
			set {
				this.position = value;

				if(this.position > this.Length) {
					this.Length = (int) this.position;
				}
			}
		}

		public bool IsNull => false;
		public bool IsEmpty => this.sequence.IsEmpty;
		public int Length { get; private set; }

		public byte this[int i] {
			get {
				var temp = this.sequence.Slice(i, 1).First.Span;

				return temp[0];
			}
			set {
				var slice = this.sequence.Slice(i, 1);
				var temp = ((SequenceSegment<byte>) slice.Start.GetObject()).ReadableMemory.Span;
				temp[slice.Start.GetInteger()] = value;
			}
		}

		/// <summary>
		///     Cause the sequence to grow by X bytes.
		/// </summary>
		/// <param name="length"></param>
		public void Extend(int length) {
			var bytes = this.GetBuffer(length);

			var newSegment = ((SequenceSegment<byte>) this.sequence.End.GetObject()).Add(bytes);
			this.sequence = new ReadOnlySequence<byte>(this.startSegment, 0, newSegment, newSegment.Memory.Length);
		}

		private byte[] GetBuffer(int length) {
			byte[] bytes = null;

			if(length < 85000) {
				bytes = new byte[length];
			} else {
				bytes = ArrayPool<byte>.Shared.Rent(length);
				this.rentedBuffers.Add(bytes);
			}

			return bytes;
		}

		private void EnsureLength(int length) {
			if(this.sequence.Length < (this.Position + length)) {

				//TODO: ensure this logic is sound
				int extend = length * 2;

				if(length < (this.Length / 2)) {
					length = this.Length / 2;
				}

				this.Extend(length);
			}
		}

		public void Write(in ReadOnlySpan<byte> bytes) {

			this.Write(bytes, 0, bytes.Length);
		}

		public void Write(in ReadOnlySequence<byte> bytes) {

			this.Write(bytes, 0, (int) bytes.Length);
		}

		public void Write(in ReadOnlySpan<byte> bytes, int offset, int length) {

			this.EnsureLength(length);

			var copySpan = bytes.Slice(offset, length);

			this.CopyFrom(copySpan, 0, (int) this.Position, copySpan.Length);

			this.Position += copySpan.Length;
		}

		public void Write(in ReadOnlySequence<byte> bytes, int offset, int length) {

			this.EnsureLength(length);

			var copySpan = bytes.Slice(offset, length);

			this.CopyFrom(copySpan, 0, (int) this.Position, (int) copySpan.Length);

			this.Position += copySpan.Length;
		}

		public void Write(ref byte[] bytes) {

			this.Write(bytes, 0, bytes.Length);
		}

		public void Write(ref byte[] bytes, int offset, int length) {

			ReadOnlySpan<byte> span = bytes;
			this.Write(span, offset, length);
		}

		public void Write(ByteArray simpleBytes) {

			this.Write(simpleBytes.Span);
		}

		public void Write(ByteArray simpleBytes, int offset, int length) {

			this.Write(simpleBytes.Span, offset, length);
		}

		public void WriteByte(byte value) {
			this.EnsureLength(1);

			this[(int) this.Position++] = value;
		}

		public string ToBase64() {
			throw new NotImplementedException();
		}

		public byte[] ToExactByteArrayCopy() {
			return this.sequence.Slice(0, this.Length).ToArray();
		}

		public byte[] ToExactByteArray() {
			return this.ToExactByteArrayCopy();
		}

		public void CopyTo(in Span<byte> dest, int srcOffset, int destOffset, int length) {
			this.sequence.Slice(srcOffset, length).CopyTo(dest.Slice(destOffset, length));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(Span<byte> dest, int destOffset) {
			this.CopyTo(dest, 0, destOffset, this.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(Span<byte> dest) {
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
		public void CopyTo(SequenceBuffer dest, int srcOffset, int destOffset, int length) {

			dest.CopyFrom(this, destOffset, srcOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(SequenceBuffer dest, int destOffset) {
			this.CopyTo(dest, 0, destOffset, this.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(SequenceBuffer dest) {
			this.CopyTo(dest, 0, 0, this.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySpan<byte> src, int srcOffset, int destOffset, int length) {

			// this is more complicated. we need to cover the segments and since its a readonly class, we need to do a bit of magic to make it work
			var copySequence = this.sequence.Slice(destOffset, length);

			var currentSegment = (SequenceSegment<byte>) copySequence.Start.GetObject();
			var endSegment = (SequenceSegment<byte>) copySequence.End.GetObject();

			int offset = srcOffset;
			int startIndex = copySequence.Start.GetInteger();

			do {

				var temp = currentSegment.ReadableMemory.Span;

				int copyLength = temp.Length;

				if(currentSegment == endSegment) {
					copyLength = copySequence.End.GetInteger();
				}

				var copySpan = temp.Slice(startIndex, copyLength - startIndex);

				src.Slice(offset, copySpan.Length).CopyTo(copySpan);

				offset += copySpan.Length;

				if(currentSegment == endSegment) {
					// its the end
					break;
				}

				currentSegment = (SequenceSegment<byte>) currentSegment.Next;
				startIndex = 0; // we start at the begining of the next segment
			} while(true);
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
			this.CopyFrom(src.AsSpan(), srcOffset, 0, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ref byte[] src, int destOffset) {
			this.CopyFrom(src.AsSpan(), 0, destOffset, src.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ref byte[] src) {
			this.CopyFrom(ref src, 0, 0, src.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ReadOnlySequence<byte> src, int srcOffset, int destOffset, int length) {
			// implement copy here too
			var copySequence = this.sequence.Slice(srcOffset, destOffset);

			var currentSegment = (SequenceSegment<byte>) copySequence.Start.GetObject();
			var endSegment = (SequenceSegment<byte>) copySequence.End.GetObject();

			int offset = 0;
			int startIndex = copySequence.Start.GetInteger();

			do {

				var temp = currentSegment.ReadableMemory.Span;

				int copyLength = temp.Length;

				if(currentSegment == endSegment) {
					copyLength = copySequence.End.GetInteger();
				}

				var copySpan = temp.Slice(startIndex, copyLength - startIndex);

				src.Slice(offset, copySpan.Length).CopyTo(copySpan);

				offset += copySpan.Length;

				if(currentSegment == endSegment) {
					// its the end
					break;
				}

				currentSegment = (SequenceSegment<byte>) currentSegment.Next;
				startIndex = 0; // we start at the begining of the next segment
			} while(true);
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(SequenceBuffer src, int srcOffset, int destOffset, int length) {
			this.CopyFrom(src.sequence, srcOffset, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(SequenceBuffer src, int srcOffset, int length) {
			this.CopyFrom(src, srcOffset, 0, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(SequenceBuffer src, int destOffset) {
			this.CopyFrom(src, 0, destOffset, src.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(SequenceBuffer src) {
			this.CopyFrom(src, 0, 0, src.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ByteArray src, int srcOffset, int destOffset, int length) {
			if(src is ReadonlySequenceArray rsa) {
				//this.CopyFrom(rsa, srcOffset, destOffset, length);
				throw new NotImplementedException();
			} 
			// else if(src is SequenceBuffer sa) {
			// 	this.CopyFrom(sa, srcOffset, destOffset, length);
			// } 
			else {
				this.CopyFrom(src.Span, destOffset, length);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ByteArray src, int srcOffset, int length) {
			if(src is ReadonlySequenceArray rsa) {
				//this.CopyFrom(rsa, srcOffset, length);
				throw new NotImplementedException();
			} 
			// else if(src is SequenceBuffer sa) {
			// 	this.CopyFrom(sa, srcOffset, length);
			// }
			else {
				this.CopyFrom(src.Span, srcOffset, length);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ByteArray src, int destOffset) {
			if(src is ReadonlySequenceArray rsa) {
				//this.CopyFrom(rsa, destOffset);
				throw new NotImplementedException();
			} 
			// else if(src is SequenceBuffer sa) {
			// 	this.CopyFrom(sa, destOffset);
			// } 
			else {
				this.CopyFrom(src.Span, destOffset);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(ByteArray src) {
			if(src is ReadonlySequenceArray rsa) {
				//this.CopyFrom(rsa);
				throw new NotImplementedException();
			}
			// else if(src is SequenceBuffer sa) {
			// 	this.CopyFrom(sa);
			// } 
			else {
				this.CopyFrom(src.Span);
			}
		}

		public void Clear() {

		}

		public SequenceBuffer Clone() {
			throw new NotImplementedException();
		}

		private class SequenceSegment<T> : ReadOnlySequenceSegment<T> {
			public SequenceSegment(Memory<T> memory) {
				this.Memory = memory;
				this.ReadableMemory = memory;
			}

			/// <summary>
			///     keep a read access to the memory
			/// </summary>
			public Memory<T> ReadableMemory { get; }

			public SequenceSegment<T> Add(Memory<T> mem) {
				var segment = new SequenceSegment<T>(mem);
				segment.RunningIndex = this.RunningIndex + this.Memory.Length;
				this.Next = segment;

				return segment;
			}
		}

	#region Disposable

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {

			if(disposing && !this.IsDisposed) {
				try {
					foreach(var rented in this.rentedBuffers) {
						ArrayPool<byte>.Shared.Return(rented);
					}
				} finally {
					this.IsDisposed = true;
				}
			}
		}

		~SequenceBuffer() {
			this.Dispose(false);
		}

		public bool IsDisposed { get; private set; }

	#endregion

	}
}