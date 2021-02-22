using System;

namespace Neuralia.Blockchains.Tools.Extensions {
	public static class ArrayExtensions {

		/// <summary>
		///     remove trailing zeros of a span
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static Span<byte> TrimEnd(this Span<byte> input) {

			int length = 0;

			for(int i = input.Length - 1; i != 0; i--) {
				if(input[i] != 0) {
					break;
				}

				length++;
			}

			return input.Slice(0, input.Length - length);
		}
		
		public static Span<char> TrimEnd(this Span<char> input, char character) {

			int length = 0;

			for(int i = input.Length - 1; i != 0; i--) {
				if(input[i] != character) {
					break;
				}

				length++;
			}

			return input.Slice(0, input.Length - length);
		}
	}
}