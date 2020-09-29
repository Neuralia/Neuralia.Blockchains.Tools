using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Data.Arrays;

namespace Neuralia.Blockchains.Tools.Serialization {
	//TODO: this class can be further optimized for speed
	public static unsafe class TypeSerializer {

		

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(byte value, in Span<byte> array) {
			array[0] = Serialize(value);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(byte value, in Memory<byte> array) {
			var span = array.Span;
			Serialize(value, in span);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(byte value, in byte[] array) {
			array[0] = Serialize(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte Serialize(byte value) {
			return (byte) (value & 0xff);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, out byte result) {
			result = array[0];
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Memory<byte> array, out byte result) {
			result = array.Span[0];
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
		public static void Serialize(bool value, in Span<byte> array) {
			array[0] = Serialize(value);
		}
		
		public static void Serialize(bool value, in byte[] array) {
			array[0] = Serialize(value);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(bool value, in Memory<byte> array) {
			var span = array.Span;
			Serialize(value, in span);
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
		public static void Serialize(short value, byte* array) {
			
			array[0] = (byte) (value & 0xff);
			array[1] = (byte) (value >> (8 * 1));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(short value, in Span<byte> array) {
			
			array[0] = (byte) (value & 0xff);
			array[1] = (byte) (value >> (8 * 1));
		}
		
		public static void Serialize(short value, in byte[] array) {
			
			array[0] = (byte) (value & 0xff);
			array[1] = (byte) (value >> (8 * 1));
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(short value, in Memory<byte> array) {
			var span = array.Span;
			Serialize(value, in span);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafeArrayHandle Serialize(short value) {
			byte[] bytes = new byte[sizeof(short)];
			Serialize(value, bytes.AsSpan());

			return SafeArrayHandle.WrapAndOwn(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(byte* array, out short result) {
			
			result = (short) ((array[1] << (8 * 1)) | array[0]);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ref byte[] array, out short result) {
			result = (short) ((array[1] << (8 * 1)) | array[0]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, out short result) {
			
			result = (short) ((array[1] << (8 * 1)) | array[0]);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, int offset, out short result) {
			
			Deserialize(array.Slice(offset, sizeof(short)), out result);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Memory<byte> array, out short result) {
			var span = array.Span;
			Deserialize(in span, out result);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ref byte[] array, int offset, out short result) {
			
			result = (short) ((array[offset+1] << (8 * 1)) | array[offset+0]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, out short result) {

			Deserialize(array, 0, out result);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, int offset, out short result) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset+ offset, out result);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, out short result) {

			Deserialize(array, 0, out result);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, int offset, out short result) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset+ offset, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static short DeserializeShort(in Span<byte> array) {
			Deserialize(array, out short result);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(ushort value, byte* array) {
			array[0] = (byte) (value & 0xff);
			array[1] = (byte) (value >> (8 * 1));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(ushort value, in Span<byte> array) {

			array[0] = (byte) (value & 0xff);
			array[1] = (byte) (value >> (8 * 1));
		}
		
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(ushort value, in byte[] array) {

			array[0] = (byte) (value & 0xff);
			array[1] = (byte) (value >> (8 * 1));
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(ushort value, in Memory<byte> array) {
			var span = array.Span;
			Serialize(value, in span);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafeArrayHandle Serialize(ushort value) {
			byte[] bytes = new byte[sizeof(ushort)];
			Serialize(value, bytes.AsSpan());

			return SafeArrayHandle.WrapAndOwn(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(byte* array, out ushort result) {

			result = (ushort) ((array[1] << (8 * 1)) | array[0]);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ref byte[] array, out ushort result) {

			result = (ushort) ((array[1] << (8 * 1)) | array[0]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, out ushort result) {

			result = (ushort) ((array[1] << (8 * 1)) | array[0]);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, int offset, out ushort result) {
			
			Deserialize(array.Slice(offset, sizeof(ushort)), out result);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Memory<byte> array, out ushort result) {
			var span = array.Span;
			Deserialize(in span, out result);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ref byte[] array, int offset, out ushort result) {

			result = (ushort) ((array[offset+1] << (8 * 1)) | array[offset+0]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, out ushort result) {
			Deserialize(array, 0, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, int offset, out ushort result) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset+ offset, out result);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, out ushort result) {
			Deserialize(array, 0, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, int offset, out ushort result) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset+ offset, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort DeserializeUShort(in Span<byte> array) {
			Deserialize(array, out ushort result);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(int value, byte* array) {
			
			array[0] = (byte) (value & 0xff);
			array[1] = (byte) (value >> (8 * 1));
			array[2] = (byte) (value >> (8 * 2));
			array[3] = (byte) (value >> (8 * 3));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(int value, in Span<byte> array) {
			
			array[0] = (byte) (value & 0xff);
			array[1] = (byte) (value >> (8 * 1));
			array[2] = (byte) (value >> (8 * 2));
			array[3] = (byte) (value >> (8 * 3));
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(int value, in byte[] array) {
			
			array[0] = (byte) (value & 0xff);
			array[1] = (byte) (value >> (8 * 1));
			array[2] = (byte) (value >> (8 * 2));
			array[3] = (byte) (value >> (8 * 3));
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(int value, in Memory<byte> array) {
			var span = array.Span;
			Serialize(value, in span);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafeArrayHandle Serialize(int value) {
			byte[] bytes = new byte[sizeof(int)];
			Serialize(value, bytes.AsSpan());

			return SafeArrayHandle.WrapAndOwn(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(byte* array, out int result) {

			result = (array[3] << (8 * 3)) | (array[2] << (8 * 2)) | (array[1] << (8 * 1)) | array[0];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ref byte[] array, out int result) {

			result = (array[3] << (8 * 3)) | (array[2] << (8 * 2)) | (array[1] << (8 * 1)) | array[0];
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, out int result) {

			result = (array[3] << (8 * 3)) | (array[2] << (8 * 2)) | (array[1] << (8 * 1)) | array[0];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, int offset, out int result) {
			
			Deserialize(array.Slice(offset, sizeof(int)), out result);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Memory<byte> array, out int result) {
			var span = array.Span;
			Deserialize(in span, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ref byte[] array, int offset, out int result) {

			result = (array[offset+3] << (8 * 3)) | (array[offset+2] << (8 * 2)) | (array[offset+1] << (8 * 1)) | array[offset+0];
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, out int result) {
			Deserialize(array, 0, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, int offset, out int result) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset+ offset, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, out int result) {
			Deserialize(array, 0, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, int offset, out int result) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset+ offset, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int DeserializeInt(in Span<byte> array) {
			Deserialize(array, out int result);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(uint value, byte* array) {
			
			array[0] = (byte) (value & 0xff);
			array[1] = (byte) (value >> (8 * 1));
			array[2] = (byte) (value >> (8 * 2));
			array[3] = (byte) (value >> (8 * 3));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(uint value, in Span<byte> array) {
			
			array[0] = (byte) (value & 0xff);
			array[1] = (byte) (value >> (8 * 1));
			array[2] = (byte) (value >> (8 * 2));
			array[3] = (byte) (value >> (8 * 3));
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(uint value, in byte[] array) {
			
			array[0] = (byte) (value & 0xff);
			array[1] = (byte) (value >> (8 * 1));
			array[2] = (byte) (value >> (8 * 2));
			array[3] = (byte) (value >> (8 * 3));
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(uint value, in Memory<byte> array) {
			var span = array.Span;
			Serialize(value, in span);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafeArrayHandle Serialize(uint value) {
			byte[] bytes = new byte[sizeof(uint)];
			Serialize(value, bytes.AsSpan());

			return SafeArrayHandle.WrapAndOwn(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(byte* array, out uint result) {

			result = (uint) ((array[3] << (8 * 3)) | (array[2] << (8 * 2)) | (array[1] << (8 * 1)) | array[0]);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ref byte[] array, out uint result) {

			result = (uint) ((array[3] << (8 * 3)) | (array[2] << (8 * 2)) | (array[1] << (8 * 1)) | array[0]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array,out uint result) {

			result = (uint) ((array[3] << (8 * 3)) | (array[2] << (8 * 2)) | (array[1] << (8 * 1)) | array[0]);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, int offset, out uint result) {
			
			Deserialize(array.Slice(offset, sizeof(uint)), out result);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Memory<byte> array, out uint result) {
			var span = array.Span;
			Deserialize(in span, out result);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ref byte[] array, int offset, out uint result) {

			result = (uint) ((array[offset+ 3] << (8 * 3)) | (array[offset+ 2] << (8 * 2)) | (array[offset+ 1] << (8 * 1)) | array[offset+ 0]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, out uint result) {

			Deserialize(array, 0, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, int offset, out uint result) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset + offset, out result);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, out uint result) {

			Deserialize(array, 0, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, int offset, out uint result) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset + offset, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint DeserializeUInt(in Span<byte> array) {
			Deserialize(array, out uint result);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(long value, byte* array) {

			array[0] = (byte) (value & 0xff);
			array[1] = (byte) (value >> (8 * 1));
			array[2] = (byte) (value >> (8 * 2));
			array[3] = (byte) (value >> (8 * 3));
			array[4] = (byte) (value >> (8 * 4));
			array[5] = (byte) (value >> (8 * 5));
			array[6] = (byte) (value >> (8 * 6));
			array[7] = (byte) (value >> (8 * 7));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(long value, in Span<byte> array) {

			array[0] = (byte) (value & 0xff);
			array[1] = (byte) (value >> (8 * 1));
			array[2] = (byte) (value >> (8 * 2));
			array[3] = (byte) (value >> (8 * 3));
			array[4] = (byte) (value >> (8 * 4));
			array[5] = (byte) (value >> (8 * 5));
			array[6] = (byte) (value >> (8 * 6));
			array[7] = (byte) (value >> (8 * 7));
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(long value, in byte[] array) {

			array[0] = (byte) (value & 0xff);
			array[1] = (byte) (value >> (8 * 1));
			array[2] = (byte) (value >> (8 * 2));
			array[3] = (byte) (value >> (8 * 3));
			array[4] = (byte) (value >> (8 * 4));
			array[5] = (byte) (value >> (8 * 5));
			array[6] = (byte) (value >> (8 * 6));
			array[7] = (byte) (value >> (8 * 7));
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(long value, in Memory<byte> array) {
			var span = array.Span;
			Serialize(value, in span);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafeArrayHandle Serialize(long value) {
			byte[] bytes = new byte[sizeof(long)];
			Serialize(value, bytes.AsSpan());

			return SafeArrayHandle.WrapAndOwn(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(byte* array, out long result) {

			result = ((long) array[7] << (8 * 7)) | ((long) array[6] << (8 * 6)) | ((long) array[5] << (8 * 5)) | ((long) array[4] << (8 * 4)) | ((long) array[3] << (8 * 3)) | ((long) array[2] << (8 * 2)) | ((long) array[1] << (8 * 1)) | array[0];
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ref byte[] array, out long result) {

			result = ((long) array[7] << (8 * 7)) | ((long) array[6] << (8 * 6)) | ((long) array[5] << (8 * 5)) | ((long) array[4] << (8 * 4)) | ((long) array[3] << (8 * 3)) | ((long) array[2] << (8 * 2)) | ((long) array[1] << (8 * 1)) | array[0];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, out long result) {

			result = ((long) array[7] << (8 * 7)) | ((long) array[6] << (8 * 6)) | ((long) array[5] << (8 * 5)) | ((long) array[4] << (8 * 4)) | ((long) array[3] << (8 * 3)) | ((long) array[2] << (8 * 2)) | ((long) array[1] << (8 * 1)) | array[0];
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, int offset, out long result) {
			
			Deserialize(array.Slice(offset, sizeof(long)), out result);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Memory<byte> array, out long result) {
			var span = array.Span;
			Deserialize(in span, out result);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ref byte[] array, int offset, out long result) {
			result = ((long) array[offset+7] << (8 * 7)) | ((long) array[offset+6] << (8 * 6)) | ((long) array[offset+5] << (8 * 5)) | ((long) array[offset+4] << (8 * 4)) | ((long) array[offset+3] << (8 * 3)) | ((long) array[offset+2] << (8 * 2)) | ((long) array[offset+1] << (8 * 1)) | array[offset+0];
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, int offset, out long result) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset+ offset, out result);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, out long result) {
			Deserialize(array, 0, out result);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, int offset, out long result) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset+ offset, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long DeserializeLong(in Span<byte> array) {
			Deserialize(array, out long result);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(ulong value, byte* array) {
			array[0] = (byte) (value & 0xff);
			array[1] = (byte) (value >> (8 * 1));
			array[2] = (byte) (value >> (8 * 2));
			array[3] = (byte) (value >> (8 * 3));
			array[4] = (byte) (value >> (8 * 4));
			array[5] = (byte) (value >> (8 * 5));
			array[6] = (byte) (value >> (8 * 6));
			array[7] = (byte) (value >> (8 * 7));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(ulong value, in Span<byte> array) {
			array[0] = (byte) (value & 0xff);
			array[1] = (byte) (value >> (8 * 1));
			array[2] = (byte) (value >> (8 * 2));
			array[3] = (byte) (value >> (8 * 3));
			array[4] = (byte) (value >> (8 * 4));
			array[5] = (byte) (value >> (8 * 5));
			array[6] = (byte) (value >> (8 * 6));
			array[7] = (byte) (value >> (8 * 7));
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(ulong value, in byte[] array) {
			array[0] = (byte) (value & 0xff);
			array[1] = (byte) (value >> (8 * 1));
			array[2] = (byte) (value >> (8 * 2));
			array[3] = (byte) (value >> (8 * 3));
			array[4] = (byte) (value >> (8 * 4));
			array[5] = (byte) (value >> (8 * 5));
			array[6] = (byte) (value >> (8 * 6));
			array[7] = (byte) (value >> (8 * 7));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(ulong value, in Memory<byte> array) {
			var span = array.Span;
			Serialize(value, in span);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafeArrayHandle Serialize(ulong value) {
			byte[] bytes = new byte[sizeof(ulong)];
			Serialize(value, bytes.AsSpan());

			return SafeArrayHandle.WrapAndOwn(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(byte* array, out ulong result) {
			result = ((ulong) array[7] << (8 * 7)) | ((ulong) array[6] << (8 * 6)) | ((ulong) array[5] << (8 * 5)) | ((ulong) array[4] << (8 * 4)) | ((ulong) array[3] << (8 * 3)) | ((ulong) array[2] << (8 * 2)) | ((ulong) array[1] << (8 * 1)) | array[0];
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ref byte[] array, out ulong result) {
			result = ((ulong) array[7] << (8 * 7)) | ((ulong) array[6] << (8 * 6)) | ((ulong) array[5] << (8 * 5)) | ((ulong) array[4] << (8 * 4)) | ((ulong) array[3] << (8 * 3)) | ((ulong) array[2] << (8 * 2)) | ((ulong) array[1] << (8 * 1)) | array[0];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, out ulong result) {
			result = ((ulong) array[7] << (8 * 7)) | ((ulong) array[6] << (8 * 6)) | ((ulong) array[5] << (8 * 5)) | ((ulong) array[4] << (8 * 4)) | ((ulong) array[3] << (8 * 3)) | ((ulong) array[2] << (8 * 2)) | ((ulong) array[1] << (8 * 1)) | array[0];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, int offset, out ulong result) {
			
			Deserialize(array.Slice(offset, sizeof(ulong)), out result);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Memory<byte> array, out ulong result) {
			var span = array.Span;
			Deserialize(in span, out result);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ref byte[] array, int offset, out ulong result) {
			result = ((ulong) array[offset+7] << (8 * 7)) | ((ulong) array[offset+6] << (8 * 6)) | ((ulong) array[offset+5] << (8 * 5)) | ((ulong) array[offset+4] << (8 * 4)) | ((ulong) array[offset+3] << (8 * 3)) | ((ulong) array[offset+2] << (8 * 2)) | ((ulong) array[offset+1] << (8 * 1)) | array[offset+0];
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, out ulong result) {
			Deserialize(array, 0, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(SafeArrayHandle array, int offset, out ulong result) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset+offset, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(ByteArray array, int offset, out ulong result) {

			var bytes = array.Bytes;
			Deserialize(ref bytes, array.Offset+offset, out result);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong DeserializeULong(in Span<byte> array) {
			Deserialize(array, out ulong result);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(float value, in Span<byte> array) {

			BitConverter.TryWriteBytes(array, value);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(float value, in byte[] array) {

			BitConverter.TryWriteBytes(array, value);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(float value, in Memory<byte> array) {
			var span = array.Span;
			Serialize(value, in span);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafeArrayHandle Serialize(float value) {
			byte[] bytes = new byte[sizeof(float)];
			Serialize(value, bytes.AsSpan());

			return SafeArrayHandle.WrapAndOwn(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, out float result) {

			result = BitConverter.ToSingle(array);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, int offset, out float result) {
			
			Deserialize(array.Slice(offset, sizeof(float)), out result);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Memory<byte> array, out float result) {
			var span = array.Span;
			Deserialize(in span, out result);
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
		public static void Serialize(double value, in byte[] array) {

			BitConverter.TryWriteBytes(array, value);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(double value, in Memory<byte> array) {
			var span = array.Span;
			Serialize(value, in span);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafeArrayHandle Serialize(double value) {
			byte[] bytes = new byte[sizeof(double)];
			Serialize(value, bytes.AsSpan());

			return SafeArrayHandle.WrapAndOwn(bytes);
		}

		public static void Deserialize(in Span<byte> array, out double result) {

			result = BitConverter.ToDouble(array);

		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, int offset, out double result) {
			
			Deserialize(array.Slice(offset, sizeof(double)), out result);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Memory<byte> array, out double result) {
			var span = array.Span;
			Deserialize(in span, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double DeserializeDouble(in Span<byte> array) {
			Deserialize(array, out double result);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafeArrayHandle Serialize(decimal value) {

			SafeArrayHandle[] bytesSets = decimal.GetBits(value).Select(e => Serialize(e)).ToArray();
			int fullSize = bytesSets.Sum(b => b.Length);

			byte[] bytes = new byte[fullSize];

			int offset = 0;

			foreach(SafeArrayHandle byteset in bytesSets) {
				Buffer.BlockCopy(byteset.Bytes, byteset.Offset, bytes, offset, byteset.Length);
				offset += byteset.Length;
			}

			return SafeArrayHandle.WrapAndOwn(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(Guid value, in Span<byte> array) {

			value.TryWriteBytes(array);

		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(Guid value, in byte[] array) {

			value.TryWriteBytes(array);

		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(Guid value, in Memory<byte> array) {
			var span = array.Span;
			Serialize(value, in span);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafeArrayHandle Serialize(Guid value) {
			return SafeArrayHandle.WrapAndOwn(value.ToByteArray());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, out Guid result) {
			result = new Guid(array);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Span<byte> array, int offset, out Guid result) {
			
			Deserialize(array.Slice(offset, sizeof(Guid)), out result);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deserialize(in Memory<byte> array, out Guid result) {
			var span = array.Span;
			Deserialize(in span, out result);
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