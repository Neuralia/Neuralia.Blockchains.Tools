using System;
using Neuralia.Blockchains.Tools.Data;

namespace Neuralia.Blockchains.Tools.General {

	/// <summary>
	///     Various utilities to deal with bits and bytes arithmetic
	/// </summary>
	public static class BitUtilities {

		private const ulong multiplicator = 0x6c04f118e9966f6bUL;

		private static readonly int[] bitPatternToLog2 = new int[128] {
			0, // change to 1 if you want bitSize(0) = 1
			48, -1, -1, 31, -1, 15, 51, -1, 63, 5, -1, -1, -1, 19, -1, 23, 28, -1, -1, -1, 40, 36, 46, -1, 13, -1, -1, -1, 34, -1, 58, -1, 60, 2, 43, 55, -1, -1, -1, 50, 62, 4, -1, 18, 27, -1, 39, 45, -1, -1, 33, 57, -1, 1, 54, -1, 49, -1, 17, -1, -1, 32, -1, 53, -1, 16, -1, -1, 52, -1, -1, -1, 64, 6, 7, 8, -1, 9, -1, -1, -1, 20, 10, -1, -1, 24, -1, 29, -1, -1, 21, -1, 11, -1, -1, 41, -1, 25, 37, -1, 47, -1, 30, 14, -1, -1, -1, -1, 22, -1, -1, 35, 12, -1, -1, -1, 59, 42, -1, -1, 61, 3, 26, 38, 44, -1, 56
		};

		/// <summary>
		///     Get the mminimum amount of bits required to store this number
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static int GetValueBitSize(ulong v) {

			v |= v >> 1;
			v |= v >> 2;
			v |= v >> 4;
			v |= v >> 8;
			v |= v >> 16;
			v |= v >> 32;

			return bitPatternToLog2[(v * multiplicator) >> 57];
		}

		/// <summary>
		///     Get the number of bytes required to store this number of bits
		/// </summary>
		/// <param name="bitCount"></param>
		/// <returns></returns>
		/// <remarks>the bit count is 1 based. the first bit = 1 and the firsrt byte stores bits 1 to 8 inclusively</remarks>
		public static int GetBytesRequiredToStoreBits(int bitCount) {

			return (int) Math.Ceiling((double) bitCount / 8);
		}

		/// <summary>
		///     Set the bit at the right index in the array
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="bit"></param>
		/// <remarks>The index is 0 based. the first byte stores index 0 to 7 inclusively.</remarks>
		public static void SetBit(SafeArrayHandle buffer, int index, bool bit) {

			int byteIndex = GetBytesRequiredToStoreBits(index + 1) - 1;

			if(byteIndex > buffer.Length) {
				throw new ApplicationException("Buffer is too small");
			}

			int bitIndex = index - (byteIndex * 8);
			buffer[byteIndex] |= (byte) ((bit ? 1 : 0) << bitIndex);
		}
	}
}