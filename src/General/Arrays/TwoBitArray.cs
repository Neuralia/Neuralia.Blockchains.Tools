using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Neuralia.Blockchains.Tools.Data;

namespace Neuralia.Blockchains.Tools.General.Arrays {
	/// <summary>
	///     A special array that can hold 4 values and store in a 2 bit array.
	/// </summary>
	public class TwoBitArray : IDisposable2 {

		private const int BITSIZE = 2;
		private const int MASK = 0x03;

		private int byteSize;
		private IByteArray data;

		public TwoBitArray(int length) {

			this.Length = length;
			this.byteSize = (int) Math.Ceiling((double) this.GetBitOffset(length) / 8);
			this.data = new ByteArray(this.byteSize);
		}

		public TwoBitArray(IByteArray data, int length) {

			this.SetData(data, length);
		}

		public int Length { get; private set; }

		public byte this[int i] {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				void ThrowException() {
					throw new ApplicationException("index is bigger than available size");
				}

				if(i >= this.Length) {
					ThrowException();
				}

				int bitoffset = this.GetBitOffset(i % 4);
				int byteOffset = this.GetByteOffset(this.GetBitOffset(i));

				return (byte) ((this.data[byteOffset] & (byte) (MASK << bitoffset)) >> bitoffset);
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set {
				void ThrowException() {
					throw new ApplicationException("index is bigger than available size");
				}

				if(i >= this.Length) {
					ThrowException();
				}

				int bitoffset = this.GetBitOffset(i % 4);
				int byteOffset = this.GetByteOffset(this.GetBitOffset(i));

				byte byteVal = this.data[byteOffset];

				byteVal &= (byte) ~(MASK << bitoffset);
				byteVal |= (byte) ((value & MASK) << bitoffset);

				this.data[byteOffset] = byteVal;
			}
		}

		public static int GetCorrespondingByteSize(int length) {
			return (int) Math.Ceiling((double) length / 4);
		}

		public void SetData(IByteArray data, int length) {
			this.Length = length;
			this.byteSize = (int) Math.Ceiling((double) this.GetBitOffset(length) / 8);

			if(data.Length != this.byteSize) {
				throw new ApplicationException("Invalid data array length");
			}

			this.data = data;
		}

		private int GetBitOffset(int index) {
			return BITSIZE * index;
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

		public IByteArray GetData() {
			return this.data;
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

		~TwoBitArray() {
			this.Dispose(false);
		}

		public bool IsDisposed { get; private set; }

	#endregion

	}
}