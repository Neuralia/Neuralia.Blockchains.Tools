using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Neuralia.Blockchains.Tools.Data {

	public interface IByteArray<T, U> : IDisposable2, IComparable<U>, IEquatable<U>, IEnumerable<T>
		where U : IByteArray<T, U> {

		bool IsNull { get; }
		bool IsEmpty { get; }
		bool IsActive { get; }
		bool HasData { get; }
		bool IsExactSize { get; }
		int Length { get; }
		int Offset { get; }
		T[] Bytes { get; }

		Span<T> Span { get; }
		Memory<T> Memory { get; }

		T this[int i] {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set;
		}

		void Return();

		int GetArrayHash();

		Span<TX> CastedArray<TX>()
			where TX : struct;

		TX ReadCasted<TX>(int index)
			where TX : struct;

		void WriteCasted<TX>(int index, TX value)
			where TX : struct;

		void CopyTo(in Span<T> dest, int srcOffset, int destOffset, int length);
		void CopyTo(in Span<T> dest, int destOffset);
		void CopyTo(in Span<T> dest);
		void CopyTo(ref T[] dest, int srcOffset, int destOffset, int length);
		void CopyTo(ref T[] dest, int destOffset);
		void CopyTo(ref T[] dest);

		void CopyTo(U dest, int srcOffset, int destOffset, int length);
		void CopyTo(U dest, int destOffset);
		void CopyTo(U dest);

		void CopyFrom(ReadOnlySpan<T> src, int srcOffset, int destOffset, int length);
		void CopyFrom(ReadOnlySpan<T> src, int srcOffset, int length);
		void CopyFrom(ReadOnlySpan<T> src, int destOffset);
		void CopyFrom(ReadOnlySpan<T> src);
		void CopyFrom(ref T[] src, int srcOffset, int destOffset, int length);
		void CopyFrom(ref T[] src, int srcOffset, int length);
		void CopyFrom(ref T[] src, int destOffset);
		void CopyFrom(ref T[] src);

		void CopyFrom(ReadOnlySequence<T> src, int srcOffset, int destOffset, int length);
		void CopyFrom(ReadOnlySequence<T> src, int srcOffset, int length);
		void CopyFrom(ReadOnlySequence<T> src, int destOffset);
		void CopyFrom(ReadOnlySequence<T> src);

		void CopyFrom(U src, int srcOffset, int destOffset, int length);
		void CopyFrom(U src, int srcOffset, int length);
		void CopyFrom(U src, int destOffset);
		void CopyFrom(U src);

		U Clone();

		U Slice(int offset, int length);
		U Slice(int offset);
		U SliceReference(int offset, int length);

		byte[] ToExactByteArrayCopy();
		byte[] ToExactByteArray();

		bool Equals(object obj);
		int GetHashCode();

		string ToBase94();
		string ToBase85();
		string ToBase64();
		string ToBase58();
		string ToBase30();
		string ToBase32();
		string ToBase35();

		void Clear();

		void Clear(int offset, int length);

		void FillSafeRandom();
	}

	/// <summary>
	///     covers 99% of our use cases
	/// </summary>
	public interface IByteArray : IByteArray<byte, IByteArray> {
	}

}