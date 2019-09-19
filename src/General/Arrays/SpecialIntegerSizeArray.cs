using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Neuralia.Blockchains.Tools.Data;

namespace Neuralia.Blockchains.Tools.General.Arrays {
	/// <summary>
	///     A special array size that allows us to save uneven bitted integers, for example 4, 12, 20, 24, 28 bits numbers.
	/// </summary>
	public class SpecialIntegerSizeArray : IDisposable2 {
		public enum BitSizes {
			B0d5 = 4,
			B1d0 = 8,
			B1d5 = 12,
			B2d0 = 16,
			B2d5 = 20,
			B3d0 = 24,
			B3d5 = 28,
			B4d0 = 32
		}

		private readonly int bitsize;
		private readonly int byteSize;

		private readonly SafeArrayHandle data = SafeArrayHandle.Create();
		private readonly uint mask;

		public SpecialIntegerSizeArray(BitSizes bitsize, int length) : this(bitsize, ByteArray.Create((int) Math.Ceiling((double) GetDataBitOffset(bitsize, length) / 8)), length) {

		}

		public SpecialIntegerSizeArray(BitSizes bitsize, SafeArrayHandle data, int length) {

			this.Length = length;
			this.bitsize = (int) bitsize;
			this.byteSize = (int) Math.Ceiling((double) bitsize / 8);

			this.mask = 0;

			for(int i = 0; i < this.bitsize; i++) {
				this.mask |= (uint) (1 << i);
			}

			this.data = data;
		}

		public int Length { get; }

		public uint this[int i] {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				void ThrowException() {
					throw new ApplicationException("index is bigger than available size");
				}

				if(i >= this.Length) {
					ThrowException();
				}

				int bitoffset = this.GetBitOffset(i);
				bool ishalf = this.IsHalf(bitoffset);
				int byteOffset = this.GetByteOffset(bitoffset);

				Span<byte> array = stackalloc byte[4];

				for(int j = 0; j < this.byteSize; j++) {
					array[j] = this.data[byteOffset + j];
				}

				uint currentValue = (uint) ((array[3] << (8 * 3)) | (array[2] << (8 * 2)) | (array[1] << (8 * 1)) | array[0]);

				if(ishalf) {
					// restore it by shifting the bits to cut the half out
					currentValue >>= 4;
				}

				return currentValue & this.mask;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set {
				void ThrowException() {
					throw new ApplicationException("index is bigger than available size");
				}

				if(i >= this.Length) {
					ThrowException();
				}

				int bitoffset = this.GetBitOffset(i);
				bool ishalf = this.IsHalf(bitoffset);
				int byteOffset = this.GetByteOffset(bitoffset);

				// ensure we clear the value to the max
				uint currentValue = value & this.mask;

				if(ishalf) {
					currentValue <<= 4;

					// we start at the half of the first byte. lets restore the half value
					currentValue |= (uint) (this.data[byteOffset] & 0x0f);
				} else {
					// we start normally, and restore the half byte of the last one
					currentValue |= (uint) ((this.data[byteOffset + (this.byteSize - 1)] & 0xf0) << (8 * (this.byteSize - 1)));
				}

				Span<byte> array = stackalloc byte[4];
				array[0] = (byte) (currentValue & 0xff);
				array[1] = (byte) (currentValue >> (8 * 1));
				array[2] = (byte) (currentValue >> (8 * 2));
				array[3] = (byte) (currentValue >> (8 * 3));

				for(int j = 0; j < this.byteSize; j++) {
					this.data[byteOffset + j] = array[j];
				}
			}
		}

		private int GetBitOffset(int index) {
			return GetDataBitOffset((BitSizes) this.bitsize, index);
		}

		private static int GetDataBitOffset(BitSizes bitsize, int index) {
			return (int) bitsize * index;
		}

		private bool IsHalf(int bitoffset) {
			return (bitoffset % 8) != 0;
		}

		private int GetByteOffset(double bitoffset) {
			return (int) Math.Floor(bitoffset / 8);
		}

		/// <summary>
		///     for debugging
		/// </summary>
		public void PrintBits() {
			var results = new List<string>();

			for(int i = this.data.Length - 1; i != -1; i--) {

				string result = "";

				for(int j = 7; j != -1; j--) {
					result += (this.data[i] & (1 << j)) != 0 ? "1" : "0";

					if(j == 4) {
						result += " ";
					}
				}

				results.Add(result);
			}

			Console.WriteLine(string.Join(" | ", results));
		}

		public byte[] GetData() {
			return this.data.ToExactByteArray();
		}

	#region Disposable

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if(disposing && !this.IsDisposed) {
				try {
					this.data.Dispose();
				} finally {
					this.IsDisposed = true;
				}
			}

		}

		~SpecialIntegerSizeArray() {
			this.Dispose(false);
		}

		public bool IsDisposed { get; private set; }

	#endregion

	}
}