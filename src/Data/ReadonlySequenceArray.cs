using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Neuralia.Blockchains.Tools.Data.Allocation;

namespace Neuralia.Blockchains.Tools.Data {

	/// <summary>
	///     A special class to handle sequences sent by network pipelines
	/// </summary>
	public class ReadonlySequenceArray : IByteArray, IEquatable<ReadonlySequenceArray>, IComparable<ReadOnlySequence<byte>> {

		public ReadonlySequenceArray(in ReadOnlySequence<byte> buffer) {
			this.Sequence = buffer;
		}

		public ReadOnlySequence<byte> Sequence { get; }

		public bool IsNull => false;
		public bool IsEmpty => this.Sequence.IsEmpty;
		public bool IsActive => true;
		public bool HasData => !this.IsEmpty;
		public bool IsExactSize => true;
		public int Length => (int) this.Sequence.Length;
		public byte[] Bytes => this.Sequence.ToArray();
		public int Offset => 0;

		public Span<byte> Span => throw new NotImplementedException();

		public Memory<byte> Memory => throw new NotImplementedException();
		public bool IsDisposed { get; }

		public byte this[int i] {
			get => this.Sequence.Slice(this.Sequence.GetPosition(i, this.Sequence.Start)).First.Span[0];
			set {
				// do nothing, read only
			}
		}

		public string ToBase94() {
			throw new NotImplementedException();
		}

		public string ToBase85() {
			throw new NotImplementedException();
		}

		public string ToBase64() {
			throw new NotImplementedException();
		}

		public string ToBase58() {
			throw new NotImplementedException();
		}

		public string ToBase30() {
			throw new NotImplementedException();
		}

		public string ToBase32() {
			throw new NotImplementedException();
		}

		public string ToBase35() {
			throw new NotImplementedException();
		}

		public IByteArray Clone() {
			throw new NotImplementedException();
		}

		public IByteArray Slice(int offset, int length) {
			throw new NotImplementedException();
		}

		public byte[] ToExactByteArrayCopy() {
			return this.Sequence.ToArray();
		}

		public void Return() {
			// do nothing there
		}

		public byte[] ToExactByteArray() {
			return this.ToExactByteArrayCopy();
		}

		public Span<T> CastedArray<T>()
			where T : struct {
			throw new NotImplementedException();
		}

		public TX ReadCasted<TX>(int index)
			where TX : struct {
			throw new NotImplementedException();
		}

		public void WriteCasted<TX>(int index, TX value)
			where TX : struct {
			throw new NotImplementedException();
		}

		public IByteArray Slice(int offset) {
			throw new NotImplementedException();
		}

		public IByteArray SliceReference(int offset, int length) {
			throw new NotImplementedException();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(in Span<byte> dest, int srcOffset, int destOffset, int length) {

			this.Sequence.Slice(srcOffset, length).CopyTo(dest.Slice(destOffset, length));
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
		public void CopyTo(IByteArray dest, int destOffset) {
			this.CopyTo(dest, 0, destOffset, this.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(IByteArray dest) {
			this.CopyTo(dest, 0, 0, this.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(IByteArray dest, int srcOffset, int destOffset, int length) {
			dest.CopyFrom(this, destOffset, srcOffset, length);
		}

		public void CopyFrom(ReadOnlySpan<byte> src, int srcOffset, int destOffset, int length) {
			throw new NotImplementedException();
		}

		public void CopyFrom(ReadOnlySpan<byte> src, int srcOffset, int length) {
			throw new NotImplementedException();
		}

		public void CopyFrom(ReadOnlySpan<byte> src, int destOffset) {
			throw new NotImplementedException();
		}

		public void CopyFrom(ReadOnlySpan<byte> src) {
			throw new NotImplementedException();
		}

		public void CopyFrom(ref byte[] src, int srcOffset, int destOffset, int length) {
			throw new NotImplementedException();
		}

		public void CopyFrom(ref byte[] src, int srcOffset, int length) {
			throw new NotImplementedException();
		}

		public void CopyFrom(ref byte[] src, int destOffset) {
			throw new NotImplementedException();
		}

		public void CopyFrom(ref byte[] src) {
			throw new NotImplementedException();
		}

		public void CopyFrom(IByteArray src, int srcOffset, int destOffset, int length) {
			throw new NotImplementedException();
		}

		public void CopyFrom(IByteArray src, int srcOffset, int length) {
			throw new NotImplementedException();
		}

		public void CopyFrom(IByteArray src, int destOffset) {
			throw new NotImplementedException();
		}

		public void CopyFrom(IByteArray src) {
			throw new NotImplementedException();
		}

		public void CopyFrom(ReadOnlySequence<byte> src, int srcOffset, int destOffset, int length) {
			throw new NotImplementedException();
		}

		public void CopyFrom(ReadOnlySequence<byte> src, int srcOffset, int length) {
			throw new NotImplementedException();
		}

		public void CopyFrom(ReadOnlySequence<byte> src, int destOffset) {
			throw new NotImplementedException();
		}

		public void CopyFrom(ReadOnlySequence<byte> src) {
			throw new NotImplementedException();
		}

		public void Clear() {
			throw new NotImplementedException();
		}

		public void Clear(int offset, int length) {
			throw new NotImplementedException();
		}

		public void FillSafeRandom() {
			throw new NotImplementedException();
		}

		public void Dispose() {

		}

		public int CompareTo(IByteArray other) {
			throw new NotImplementedException();
		}

		public int GetArrayHash() {
			return MemoryAllocators.xxHash32.Hash(this.ToExactByteArray());
		}

		public bool Equals(IByteArray other) {
			throw new NotImplementedException();
		}

		public IEnumerator<byte> GetEnumerator() {
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return this.GetEnumerator();
		}

		public int CompareTo(ReadOnlySequence<byte> other) {
			throw new NotImplementedException();
		}

		public bool Equals(ReadonlySequenceArray other) {
			throw new NotImplementedException();
		}

		public int CompareTo(byte[] other) {
			throw new NotImplementedException();
		}

		public bool Equals(byte other) {
			throw new NotImplementedException();
		}

		public IByteArray FromBase30(string value) {
			throw new NotImplementedException();
		}

		public IByteArray FromBase32(string value) {
			throw new NotImplementedException();
		}

		public IByteArray FromBase35(string value) {
			throw new NotImplementedException();
		}

		public IByteArray FromBase58(string value) {
			throw new NotImplementedException();
		}

		public IByteArray FromBase64(string value) {
			throw new NotImplementedException();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(ReadonlySequenceArray dest, int srcOffset, int destOffset, int length) {

			dest.CopyFrom(this, destOffset, srcOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(ReadonlySequenceArray dest, int destOffset) {
			this.CopyTo(dest, 0, destOffset, this.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(ReadonlySequenceArray dest) {
			this.CopyTo(dest, 0, 0, this.Length);
		}

		public void CopyFrom(ReadonlySequenceArray src, int srcOffset, int destOffset, int length) {
			throw new NotImplementedException();
		}

		public void CopyFrom(ReadonlySequenceArray src, int srcOffset, int length) {
			throw new NotImplementedException();
		}

		public void CopyFrom(ReadonlySequenceArray src, int destOffset) {
			throw new NotImplementedException();
		}

		public void CopyFrom(ReadonlySequenceArray src) {
			throw new NotImplementedException();
		}
	}
}