using System;
using System.Runtime.CompilerServices;

namespace Neuralia.Blockchains.Core.Cryptography.xxHash {
	public static class xxHashUtils {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint RotateLeft(uint operand, int shiftCount) {
			shiftCount &= 0x1f;

			return (operand << shiftCount) | (operand >> (32 - shiftCount));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong RotateLeft(ulong operand, int shiftCount) {
			shiftCount &= 0x3f;

			return (operand << shiftCount) | (operand >> (64 - shiftCount));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize(uint value, in Span<byte> array) {

			array[0] = (byte) (value & 0xff);
			array[1] = (byte) (value >> (8 * 1));
			array[2] = (byte) (value >> (8 * 2));
			array[3] = (byte) (value >> (8 * 3));
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
		public static ulong Deserialize64(in Span<byte> data, int offset) {
			Span<byte> array = data.Slice(offset, sizeof(ulong));

			return ((ulong) array[7] << (8 * 7)) | ((ulong) array[6] << (8 * 6)) | ((ulong) array[5] << (8 * 5)) | ((ulong) array[4] << (8 * 4)) | ((ulong) array[3] << (8 * 3)) | ((ulong) array[2] << (8 * 2)) | ((ulong) array[1] << (8 * 1)) | array[0];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Deserialize32(in Span<byte> data, int offset) {
			Span<byte> array = data.Slice(offset, sizeof(uint));

			return ((uint) array[3] << (8 * 3)) | ((uint) array[2] << (8 * 2)) | ((uint) array[1] << (8 * 1)) | array[0];
		}
	}
}