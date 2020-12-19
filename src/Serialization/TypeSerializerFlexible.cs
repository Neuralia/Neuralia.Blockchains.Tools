using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Data.Arrays;

namespace Neuralia.Blockchains.Tools.Serialization {
	//TODO: this class can be further optimized for speed
	public static unsafe class TypeSerializerFlexible {

		public enum Direction {
			SmallEndian,
			BigEndian
		}

		private static readonly int[] smallEndianIndices = new int[8];
		private static readonly int[] bigEndianIndices = new int[8];

		static TypeSerializerFlexible() {
			for(int i = 0; i < smallEndianIndices.Length; i++) {
				smallEndianIndices[i] = i;
			}

			for(int i = 0; i < bigEndianIndices.Length; i++) {
				bigEndianIndices[i] = bigEndianIndices.Length - 1 - i;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(byte value, in Span<byte> array) {
			array[0] = Serialize(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte Serialize(byte value) {
			return (byte) (value & 0xff);

			;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, out byte result) {
			result = array[0];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, out byte result) {
			Span<byte> span = array.Span;
			Deserialize(in span, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte DeserializeByte(in Span<byte> array) {
			Deserialize(array, out byte result);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static (int[] indices, int adjuster) GetIndicies(int size, Direction direction) {
			if(direction == Direction.SmallEndian) {

				return (smallEndianIndices, 0);
			}

			return (bigEndianIndices, 8 - size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int GetIndex(int index, int adjuster, in int[] indices) {
			return indices[index] - adjuster;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(bool value, in Span<byte> array) {
			array[0] = Serialize(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte Serialize(bool value) {
			return (byte) (value ? 1 : 0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, out bool result) {
			Span<byte> span = array.Span;
			Deserialize(in span, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, out bool result) {

			result = array[0] != 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool DeserializeBool(in Span<byte> array) {
			Deserialize(array, out bool result);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(short value, byte* array, Direction direction = Direction.SmallEndian) {

			(int[] indices, int adjuster) = GetIndicies(sizeof(short), direction);

			array[GetIndex(0, adjuster, indices)] = (byte) (value & 0xff);
			array[GetIndex(1, adjuster, indices)] = (byte) (value >> (8 * 1));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(short value, in Span<byte> array, Direction direction = Direction.SmallEndian) {

			(int[] indices, int adjuster) = GetIndicies(sizeof(short), direction);

			array[GetIndex(0, adjuster, indices)] = (byte) (value & 0xff);
			array[GetIndex(1, adjuster, indices)] = (byte) (value >> (8 * 1));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Serialize(short value, Direction direction = Direction.SmallEndian) {
			byte[] bytes = new byte[sizeof(short)];
			Serialize(value, bytes.AsSpan(), direction);

			return ByteArray.WrapAndOwn(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(byte* array, out short result, Direction direction = Direction.SmallEndian) {

			(int[] indices, int adjuster) = GetIndicies(sizeof(short), direction);

			result = (short) ((array[GetIndex(1, adjuster, indices)] << (8 * 1)) | array[GetIndex(0, adjuster, indices)]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, out short result, Direction direction = Direction.SmallEndian) {

			(int[] indices, int adjuster) = GetIndicies(sizeof(short), direction);

			result = (short) ((array[GetIndex(1, adjuster, indices)] << (8 * 1)) | array[GetIndex(0, adjuster, indices)]);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ref byte[] array, int offset, out short result, Direction direction = Direction.SmallEndian) {

			(int[] indices, int adjuster) = GetIndicies(sizeof(short), direction);

			result = (short) ((array[offset+GetIndex(1, adjuster, indices)] << (8 * 1)) | array[offset+GetIndex(0, adjuster, indices)]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, out short result, Direction direction = Direction.SmallEndian) {

			Deserialize(array, 0, out result, direction);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, int offset, out short result, Direction direction = Direction.SmallEndian) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset+ offset, out result, direction);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, out short result, Direction direction = Direction.SmallEndian) {

			Deserialize(array, 0, out result, direction);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, int offset, out short result, Direction direction = Direction.SmallEndian) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset+ offset, out result, direction);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> span, int offset, out short result, Direction direction = Direction.SmallEndian) {

			Deserialize(span.Slice(offset, sizeof(short)), out result, direction);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static short DeserializeShort(in Span<byte> array, Direction direction = Direction.SmallEndian) {
			Deserialize(array, out short result, direction);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(ushort value, byte* array, Direction direction = Direction.SmallEndian) {
			(int[] indices, int adjuster) = GetIndicies(sizeof(ushort), direction);

			array[GetIndex(0, adjuster, indices)] = (byte) (value & 0xff);
			array[GetIndex(1, adjuster, indices)] = (byte) (value >> (8 * 1));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(ushort value, in Span<byte> array, Direction direction = Direction.SmallEndian) {
			(int[] indices, int adjuster) = GetIndicies(sizeof(ushort), direction);

			array[GetIndex(0, adjuster, indices)] = (byte) (value & 0xff);
			array[GetIndex(1, adjuster, indices)] = (byte) (value >> (8 * 1));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Serialize(ushort value, Direction direction = Direction.SmallEndian) {
			byte[] bytes = new byte[sizeof(ushort)];
			Serialize(value, bytes.AsSpan(), direction);

			return ByteArray.WrapAndOwn(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(byte* array, out ushort result, Direction direction = Direction.SmallEndian) {
			(int[] indices, int adjuster) = GetIndicies(sizeof(ushort), direction);

			result = (ushort) ((array[GetIndex(1, adjuster, indices)] << (8 * 1)) | array[GetIndex(0, adjuster, indices)]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, out ushort result, Direction direction = Direction.SmallEndian) {
			(int[] indices, int adjuster) = GetIndicies(sizeof(ushort), direction);

			result = (ushort) ((array[GetIndex(1, adjuster, indices)] << (8 * 1)) | array[GetIndex(0, adjuster, indices)]);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ref byte[] array, int offset, out ushort result, Direction direction = Direction.SmallEndian) {
			(int[] indices, int adjuster) = GetIndicies(sizeof(ushort), direction);

			result = (ushort) ((array[offset+GetIndex(1, adjuster, indices)] << (8 * 1)) | array[offset+GetIndex(0, adjuster, indices)]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> span, int offset, out ushort result, Direction direction = Direction.SmallEndian) {

			Deserialize(span.Slice(offset, sizeof(ushort)), out result, direction);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, out ushort result, Direction direction = Direction.SmallEndian) {
			Deserialize(array, 0, out result, direction);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, int offset, out ushort result, Direction direction = Direction.SmallEndian) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset+ offset, out result, direction);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, out ushort result, Direction direction = Direction.SmallEndian) {
			Deserialize(array, 0, out result, direction);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, int offset, out ushort result, Direction direction = Direction.SmallEndian) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset+ offset, out result, direction);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort DeserializeUShort(in Span<byte> array, Direction direction = Direction.SmallEndian) {
			Deserialize(array, out ushort result, direction);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(int value, byte* array, Direction direction = Direction.SmallEndian) {

			(int[] indices, int adjuster) = GetIndicies(sizeof(int), direction);

			array[GetIndex(0, adjuster, indices)] = (byte) (value & 0xff);
			array[GetIndex(1, adjuster, indices)] = (byte) (value >> (8 * 1));
			array[GetIndex(2, adjuster, indices)] = (byte) (value >> (8 * 2));
			array[GetIndex(3, adjuster, indices)] = (byte) (value >> (8 * 3));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(int value, in Span<byte> array, Direction direction = Direction.SmallEndian) {

			(int[] indices, int adjuster) = GetIndicies(sizeof(int), direction);

			array[GetIndex(0, adjuster, indices)] = (byte) (value & 0xff);
			array[GetIndex(1, adjuster, indices)] = (byte) (value >> (8 * 1));
			array[GetIndex(2, adjuster, indices)] = (byte) (value >> (8 * 2));
			array[GetIndex(3, adjuster, indices)] = (byte) (value >> (8 * 3));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Serialize(int value, Direction direction = Direction.SmallEndian) {
			byte[] bytes = new byte[sizeof(int)];
			Serialize(value, bytes.AsSpan(), direction);

			return ByteArray.WrapAndOwn(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(byte* array, out int result, Direction direction = Direction.SmallEndian) {

			(int[] indices, int adjuster) = GetIndicies(sizeof(int), direction);
			result = (array[GetIndex(3, adjuster, indices)] << (8 * 3)) | (array[GetIndex(2, adjuster, indices)] << (8 * 2)) | (array[GetIndex(1, adjuster, indices)] << (8 * 1)) | array[GetIndex(0, adjuster, indices)];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, out int result, Direction direction = Direction.SmallEndian) {

			(int[] indices, int adjuster) = GetIndicies(sizeof(int), direction);
			result = (array[GetIndex(3, adjuster, indices)] << (8 * 3)) | (array[GetIndex(2, adjuster, indices)] << (8 * 2)) | (array[GetIndex(1, adjuster, indices)] << (8 * 1)) | array[GetIndex(0, adjuster, indices)];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ref byte[] array, int offset, out int result, Direction direction = Direction.SmallEndian) {

			(int[] indices, int adjuster) = GetIndicies(sizeof(int), direction);
			result = (array[offset+GetIndex(3, adjuster, indices)] << (8 * 3)) | (array[offset+GetIndex(2, adjuster, indices)] << (8 * 2)) | (array[offset+GetIndex(1, adjuster, indices)] << (8 * 1)) | array[offset+GetIndex(0, adjuster, indices)];
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, out int result, Direction direction = Direction.SmallEndian) {
			Deserialize(array, 0, out result, direction);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, int offset, out int result, Direction direction = Direction.SmallEndian) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset+ offset, out result, direction);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, out int result, Direction direction = Direction.SmallEndian) {
			Deserialize(array, 0, out result, direction);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, int offset, out int result, Direction direction = Direction.SmallEndian) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset+ offset, out result, direction);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int DeserializeInt(in Span<byte> array, Direction direction = Direction.SmallEndian) {
			Deserialize(array, out int result, direction);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(uint value, byte* array, Direction direction = Direction.SmallEndian) {

			(int[] indices, int adjuster) = GetIndicies(sizeof(uint), direction);

			array[GetIndex(0, adjuster, indices)] = (byte) (value & 0xff);
			array[GetIndex(1, adjuster, indices)] = (byte) (value >> (8 * 1));
			array[GetIndex(2, adjuster, indices)] = (byte) (value >> (8 * 2));
			array[GetIndex(3, adjuster, indices)] = (byte) (value >> (8 * 3));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(uint value, in Span<byte> array, Direction direction = Direction.SmallEndian) {

			(int[] indices, int adjuster) = GetIndicies(sizeof(uint), direction);

			array[GetIndex(0, adjuster, indices)] = (byte) (value & 0xff);
			array[GetIndex(1, adjuster, indices)] = (byte) (value >> (8 * 1));
			array[GetIndex(2, adjuster, indices)] = (byte) (value >> (8 * 2));
			array[GetIndex(3, adjuster, indices)] = (byte) (value >> (8 * 3));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Serialize(uint value, Direction direction = Direction.SmallEndian) {
			byte[] bytes = new byte[sizeof(uint)];
			Serialize(value, bytes.AsSpan(), direction);

			return ByteArray.WrapAndOwn(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(byte* array, out uint result, Direction direction = Direction.SmallEndian) {

			(int[] indices, int adjuster) = GetIndicies(sizeof(uint), direction);
			result = (uint) ((array[GetIndex(3, adjuster, indices)] << (8 * 3)) | (array[GetIndex(2, adjuster, indices)] << (8 * 2)) | (array[GetIndex(1, adjuster, indices)] << (8 * 1)) | array[GetIndex(0, adjuster, indices)]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array,out uint result, Direction direction = Direction.SmallEndian) {

			(int[] indices, int adjuster) = GetIndicies(sizeof(uint), direction);
			result = (uint) ((array[GetIndex(3, adjuster, indices)] << (8 * 3)) | (array[GetIndex(2, adjuster, indices)] << (8 * 2)) | (array[GetIndex(1, adjuster, indices)] << (8 * 1)) | array[GetIndex(0, adjuster, indices)]);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ref byte[] array, int offset, out uint result, Direction direction = Direction.SmallEndian) {

			(int[] indices, int adjuster) = GetIndicies(sizeof(uint), direction);
			result = (uint) ((array[offset+ GetIndex(3, adjuster, indices)] << (8 * 3)) | (array[offset+ GetIndex(2, adjuster, indices)] << (8 * 2)) | (array[offset+ GetIndex(1, adjuster, indices)] << (8 * 1)) | array[offset+ GetIndex(0, adjuster, indices)]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> span, int offset, out uint result, Direction direction = Direction.SmallEndian) {

			Deserialize(span.Slice(offset, sizeof(uint)), out result, direction);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, out uint result, Direction direction = Direction.SmallEndian) {

			Deserialize(array, 0, out result, direction);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, int offset, out uint result, Direction direction = Direction.SmallEndian) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset + offset, out result, direction);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, out uint result, Direction direction = Direction.SmallEndian) {

			Deserialize(array, 0, out result, direction);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, int offset, out uint result, Direction direction = Direction.SmallEndian) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset + offset, out result, direction);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint DeserializeUInt(in Span<byte> array, Direction direction = Direction.SmallEndian) {
			Deserialize(array, out uint result, direction);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(long value, byte* array, Direction direction = Direction.SmallEndian) {

			(int[] indices, int adjuster) = GetIndicies(sizeof(long), direction);
			array[GetIndex(0, adjuster, indices)] = (byte) (value & 0xff);
			array[GetIndex(1, adjuster, indices)] = (byte) (value >> (8 * 1));
			array[GetIndex(2, adjuster, indices)] = (byte) (value >> (8 * 2));
			array[GetIndex(3, adjuster, indices)] = (byte) (value >> (8 * 3));
			array[GetIndex(4, adjuster, indices)] = (byte) (value >> (8 * 4));
			array[GetIndex(5, adjuster, indices)] = (byte) (value >> (8 * 5));
			array[GetIndex(6, adjuster, indices)] = (byte) (value >> (8 * 6));
			array[GetIndex(7, adjuster, indices)] = (byte) (value >> (8 * 7));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(long value, in Span<byte> array, Direction direction = Direction.SmallEndian) {

			(int[] indices, int adjuster) = GetIndicies(sizeof(long), direction);
			array[GetIndex(0, adjuster, indices)] = (byte) (value & 0xff);
			array[GetIndex(1, adjuster, indices)] = (byte) (value >> (8 * 1));
			array[GetIndex(2, adjuster, indices)] = (byte) (value >> (8 * 2));
			array[GetIndex(3, adjuster, indices)] = (byte) (value >> (8 * 3));
			array[GetIndex(4, adjuster, indices)] = (byte) (value >> (8 * 4));
			array[GetIndex(5, adjuster, indices)] = (byte) (value >> (8 * 5));
			array[GetIndex(6, adjuster, indices)] = (byte) (value >> (8 * 6));
			array[GetIndex(7, adjuster, indices)] = (byte) (value >> (8 * 7));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Serialize(long value, Direction direction = Direction.SmallEndian) {
			byte[] bytes = new byte[sizeof(long)];
			Serialize(value, bytes.AsSpan(), direction);

			return ByteArray.WrapAndOwn(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(byte* array, out long result, Direction direction = Direction.SmallEndian) {

			(int[] indices, int adjuster) = GetIndicies(sizeof(long), direction);
			result = ((long) array[GetIndex(7, adjuster, indices)] << (8 * 7)) | ((long) array[GetIndex(6, adjuster, indices)] << (8 * 6)) | ((long) array[GetIndex(5, adjuster, indices)] << (8 * 5)) | ((long) array[GetIndex(4, adjuster, indices)] << (8 * 4)) | ((long) array[GetIndex(3, adjuster, indices)] << (8 * 3)) | ((long) array[GetIndex(2, adjuster, indices)] << (8 * 2)) | ((long) array[GetIndex(1, adjuster, indices)] << (8 * 1)) | array[GetIndex(0, adjuster, indices)];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, out long result, Direction direction = Direction.SmallEndian) {

			(int[] indices, int adjuster) = GetIndicies(sizeof(long), direction);
			result = ((long) array[GetIndex(7, adjuster, indices)] << (8 * 7)) | ((long) array[GetIndex(6, adjuster, indices)] << (8 * 6)) | ((long) array[GetIndex(5, adjuster, indices)] << (8 * 5)) | ((long) array[GetIndex(4, adjuster, indices)] << (8 * 4)) | ((long) array[GetIndex(3, adjuster, indices)] << (8 * 3)) | ((long) array[GetIndex(2, adjuster, indices)] << (8 * 2)) | ((long) array[GetIndex(1, adjuster, indices)] << (8 * 1)) | array[GetIndex(0, adjuster, indices)];
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ref byte[] array, int offset, out long result, Direction direction = Direction.SmallEndian) {
			(int[] indices, int adjuster) = GetIndicies(sizeof(long), direction);
			result = ((long) array[offset+GetIndex(7, adjuster, indices)] << (8 * 7)) | ((long) array[offset+GetIndex(6, adjuster, indices)] << (8 * 6)) | ((long) array[offset+GetIndex(5, adjuster, indices)] << (8 * 5)) | ((long) array[offset+GetIndex(4, adjuster, indices)] << (8 * 4)) | ((long) array[offset+GetIndex(3, adjuster, indices)] << (8 * 3)) | ((long) array[offset+GetIndex(2, adjuster, indices)] << (8 * 2)) | ((long) array[offset+GetIndex(1, adjuster, indices)] << (8 * 1)) | array[offset+GetIndex(0, adjuster, indices)];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> span, int offset, out long result, Direction direction = Direction.SmallEndian) {

			Deserialize(span.Slice(offset, sizeof(long)), out result, direction);
		}

		

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, int offset, out long result, Direction direction = Direction.SmallEndian) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset+ offset, out result, direction);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, out long result, Direction direction = Direction.SmallEndian) {
			Deserialize(array, 0, out result, direction);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, int offset, out long result, Direction direction = Direction.SmallEndian) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset+ offset, out result, direction);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long DeserializeLong(in Span<byte> array, Direction direction = Direction.SmallEndian) {
			Deserialize(array, out long result, direction);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(ulong value, byte* array, Direction direction = Direction.SmallEndian) {
			(int[] indices, int adjuster) = GetIndicies(sizeof(ulong), direction);
			array[GetIndex(0, adjuster, indices)] = (byte) (value & 0xff);
			array[GetIndex(1, adjuster, indices)] = (byte) (value >> (8 * 1));
			array[GetIndex(2, adjuster, indices)] = (byte) (value >> (8 * 2));
			array[GetIndex(3, adjuster, indices)] = (byte) (value >> (8 * 3));
			array[GetIndex(4, adjuster, indices)] = (byte) (value >> (8 * 4));
			array[GetIndex(5, adjuster, indices)] = (byte) (value >> (8 * 5));
			array[GetIndex(6, adjuster, indices)] = (byte) (value >> (8 * 6));
			array[GetIndex(7, adjuster, indices)] = (byte) (value >> (8 * 7));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(ulong value, in Span<byte> array, Direction direction = Direction.SmallEndian) {
			(int[] indices, int adjuster) = GetIndicies(sizeof(ulong), direction);
			array[GetIndex(0, adjuster, indices)] = (byte) (value & 0xff);
			array[GetIndex(1, adjuster, indices)] = (byte) (value >> (8 * 1));
			array[GetIndex(2, adjuster, indices)] = (byte) (value >> (8 * 2));
			array[GetIndex(3, adjuster, indices)] = (byte) (value >> (8 * 3));
			array[GetIndex(4, adjuster, indices)] = (byte) (value >> (8 * 4));
			array[GetIndex(5, adjuster, indices)] = (byte) (value >> (8 * 5));
			array[GetIndex(6, adjuster, indices)] = (byte) (value >> (8 * 6));
			array[GetIndex(7, adjuster, indices)] = (byte) (value >> (8 * 7));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Serialize(ulong value, Direction direction = Direction.SmallEndian) {
			byte[] bytes = new byte[sizeof(ulong)];
			Serialize(value, bytes.AsSpan(), direction);

			return ByteArray.WrapAndOwn(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(byte* array, out ulong result, Direction direction = Direction.SmallEndian) {
			(int[] indices, int adjuster) = GetIndicies(sizeof(ulong), direction);
			result = ((ulong) array[GetIndex(7, adjuster, indices)] << (8 * 7)) | ((ulong) array[GetIndex(6, adjuster, indices)] << (8 * 6)) | ((ulong) array[GetIndex(5, adjuster, indices)] << (8 * 5)) | ((ulong) array[GetIndex(4, adjuster, indices)] << (8 * 4)) | ((ulong) array[GetIndex(3, adjuster, indices)] << (8 * 3)) | ((ulong) array[GetIndex(2, adjuster, indices)] << (8 * 2)) | ((ulong) array[GetIndex(1, adjuster, indices)] << (8 * 1)) | array[GetIndex(0, adjuster, indices)];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, out ulong result, Direction direction = Direction.SmallEndian) {
			(int[] indices, int adjuster) = GetIndicies(sizeof(ulong), direction);
			result = ((ulong) array[GetIndex(7, adjuster, indices)] << (8 * 7)) | ((ulong) array[GetIndex(6, adjuster, indices)] << (8 * 6)) | ((ulong) array[GetIndex(5, adjuster, indices)] << (8 * 5)) | ((ulong) array[GetIndex(4, adjuster, indices)] << (8 * 4)) | ((ulong) array[GetIndex(3, adjuster, indices)] << (8 * 3)) | ((ulong) array[GetIndex(2, adjuster, indices)] << (8 * 2)) | ((ulong) array[GetIndex(1, adjuster, indices)] << (8 * 1)) | array[GetIndex(0, adjuster, indices)];
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ref byte[] array, int offset, out ulong result, Direction direction = Direction.SmallEndian) {
			(int[] indices, int adjuster) = GetIndicies(sizeof(ulong), direction);
			result = ((ulong) array[offset+GetIndex(7, adjuster, indices)] << (8 * 7)) | ((ulong) array[offset+GetIndex(6, adjuster, indices)] << (8 * 6)) | ((ulong) array[offset+GetIndex(5, adjuster, indices)] << (8 * 5)) | ((ulong) array[offset+GetIndex(4, adjuster, indices)] << (8 * 4)) | ((ulong) array[offset+GetIndex(3, adjuster, indices)] << (8 * 3)) | ((ulong) array[offset+GetIndex(2, adjuster, indices)] << (8 * 2)) | ((ulong) array[offset+GetIndex(1, adjuster, indices)] << (8 * 1)) | array[offset+GetIndex(0, adjuster, indices)];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> span, int offset, out ulong result, Direction direction = Direction.SmallEndian) {

			Deserialize(span.Slice(offset, sizeof(ulong)), out result, direction);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, out ulong result, Direction direction = Direction.SmallEndian) {
			Deserialize(array, 0, out result, direction);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, int offset, out ulong result, Direction direction = Direction.SmallEndian) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset+offset, out result, direction);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, int offset, out ulong result, Direction direction = Direction.SmallEndian) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset+offset, out result, direction);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong DeserializeULong(in Span<byte> array, Direction direction = Direction.SmallEndian) {
			Deserialize(array, out ulong result, direction);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(float value, in Span<byte> array) {

			BitConverter.TryWriteBytes(array, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Serialize(float value) {
			byte[] bytes = new byte[sizeof(float)];
			Serialize(value, bytes.AsSpan());

			return ByteArray.WrapAndOwn(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, out float result) {

			result = BitConverter.ToSingle(array);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, int offset, out float result) {

			Deserialize(array.Span.Slice(offset, sizeof(float)), out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float DeserializeFloat(in Span<byte> array) {
			Deserialize(array, out float result);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(double value, in Span<byte> array) {

			BitConverter.TryWriteBytes(array, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Serialize(double value) {
			byte[] bytes = new byte[sizeof(double)];
			Serialize(value, bytes.AsSpan());

			return ByteArray.WrapAndOwn(bytes);
		}

		public static void Deserialize(in Span<byte> array, out double result) {

			result = BitConverter.ToDouble(array);

		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double DeserializeDouble(in Span<byte> array) {
			Deserialize(array, out double result);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Serialize(decimal value) {

			ByteArray[] bytesSets = decimal.GetBits(value).Select(e => Serialize(e)).ToArray();
			int fullSize = bytesSets.Sum(b => b.Length);

			byte[] bytes = new byte[fullSize];

			int offset = 0;

			foreach(ByteArray byteset in bytesSets) {
				Buffer.BlockCopy(byteset.Bytes, byteset.Offset, bytes, offset, byteset.Length);
				offset += byteset.Length;
			}

			return ByteArray.WrapAndOwn(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(Guid value, in Span<byte> array) {

			value.TryWriteBytes(array);

		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ByteArray Serialize(Guid value) {
			return ByteArray.WrapAndOwn(value.ToByteArray());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, out Guid result) {

			result = new Guid(array);

		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Guid DeserializeGuid(in Span<byte> array) {
			Deserialize(array, out Guid result);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, int offset, out double result) {

			Deserialize(array.Span.Slice(offset, sizeof(double)), out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SerializeBytes(in Span<byte> array, short value) {

			Span<byte> valueBytes = stackalloc byte[sizeof(short)];

			Serialize(value, valueBytes);
			SerializeBytes(array, valueBytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SerializeBytes(in Span<byte> array, ushort value) {

			Span<byte> valueBytes = stackalloc byte[sizeof(ushort)];

			Serialize(value, valueBytes);
			SerializeBytes(array, valueBytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SerializeBytes(in Span<byte> array, int value) {

			Span<byte> valueBytes = stackalloc byte[sizeof(int)];

			Serialize(value, valueBytes);
			SerializeBytes(array, valueBytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SerializeBytes(in Span<byte> array, uint value) {

			Span<byte> valueBytes = stackalloc byte[sizeof(uint)];

			Serialize(value, valueBytes);
			SerializeBytes(array, valueBytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SerializeBytes(in Span<byte> array, long value) {

			Span<byte> valueBytes = stackalloc byte[sizeof(long)];

			Serialize(value, valueBytes);
			SerializeBytes(array, valueBytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SerializeBytes(in Span<byte> array, ulong value) {

			Span<byte> valueBytes = stackalloc byte[sizeof(ulong)];

			Serialize(value, valueBytes);
			SerializeBytes(array, valueBytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void SerializeBytes(in Span<byte> array, in Span<byte> valueBytes) {

			int length = array.Length;
			int valueSize = valueBytes.Length;

			if(length > valueSize) {
				length = valueSize;
			}

			valueBytes.Slice(0, length).CopyTo(array);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SerializeBytes(in Span<byte> array, Guid value) {

			Span<byte> valueBytes = stackalloc byte[sizeof(ulong)];

			Serialize(value, valueBytes);
			SerializeBytes(array, valueBytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DeserializeBytes(in Span<byte> array, out short value) {

			int valueSize = sizeof(short);

			Span<byte> valueBytes = stackalloc byte[valueSize];

			array.Slice(0, valueSize).CopyTo(valueBytes);

			Deserialize(valueBytes, out value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DeserializeBytes(in Span<byte> array, out ushort value) {

			int valueSize = sizeof(ushort);

			Span<byte> valueBytes = stackalloc byte[valueSize];

			array.Slice(0, valueSize).CopyTo(valueBytes);

			Deserialize(valueBytes, out value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DeserializeBytes(in Span<byte> array, out int value) {

			int valueSize = sizeof(int);

			Span<byte> valueBytes = stackalloc byte[valueSize];

			array.Slice(0, valueSize).CopyTo(valueBytes);

			Deserialize(valueBytes, out value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DeserializeBytes(in Span<byte> array, out uint value) {

			int valueSize = sizeof(uint);

			Span<byte> valueBytes = stackalloc byte[valueSize];

			array.Slice(0, valueSize).CopyTo(valueBytes);

			Deserialize(valueBytes, out value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DeserializeBytes(in Span<byte> array, out long value) {

			int valueSize = sizeof(long);

			Span<byte> valueBytes = stackalloc byte[valueSize];

			array.Slice(0, valueSize).CopyTo(valueBytes);

			Deserialize(valueBytes, out value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DeserializeBytes(in Span<byte> array, out ulong value) {

			int valueSize = sizeof(ulong);

			Span<byte> valueBytes = stackalloc byte[valueSize];

			array.Slice(0, valueSize).CopyTo(valueBytes);

			Deserialize(valueBytes, out value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DeserializeBytes(in Span<byte> array, out Guid value) {

			value = new Guid(array);

		}
	}
}