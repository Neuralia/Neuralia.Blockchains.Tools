using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Neuralia.Blockchains.Tools.Data.Arrays.Large {
	
	/// <summary>
	/// A byte array that can bypass the 2GB limit
	/// </summary>
	public class VeryLargeByteArray : IVeryLargeByteArray {

		private const int GIGABYTE = 1 << 30;
		public readonly ByteArray[] pieces;
		public readonly uint MAX_SIZE;

		public long Length { get; } = 0;

		public VeryLargeByteArray(long length, uint maxSize = GIGABYTE) {
			this.MAX_SIZE = maxSize;
			this.Length = length;

			long parts = (long)Math.Ceiling((decimal)length / this.MAX_SIZE);

			long remaining = length;
			this.pieces = new ByteArray[parts];
			for(int i = 0; i < parts; i++) {
				this.pieces[i] = ByteArray.Create((int)Math.Min(remaining, this.MAX_SIZE));
				remaining -= this.MAX_SIZE;
			}
		}

		public byte this[long i] {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {

				void ThrowException() {
					throw new IndexOutOfRangeException();
				}

				if(i >= this.Length) {
					ThrowException();
				}

				int startIndex = (int)(i / this.MAX_SIZE);
				return this.pieces[startIndex][(int)(i - (startIndex* this.MAX_SIZE))];
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set {

				void ThrowException() {
					throw new IndexOutOfRangeException();
				}

				if(i >= this.Length) {
					ThrowException();
				}

				int startIndex = (int)(i / this.MAX_SIZE);
				this.pieces[startIndex][(int) (i - (startIndex * this.MAX_SIZE))] = value;
			}
		}

		public Task Initialize() {
			return Task.CompletedTask;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task Clear() {
			foreach(var entry in this.pieces) {
				entry.Clear();
			}

			return Task.CompletedTask;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task CopyTo(SafeArrayHandle dest, long srcOffset, int destOffset, int length) {
			return this.CopyTo(dest.Entry, srcOffset, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task CopyTo(ByteArray dest, long srcOffset, int destOffset, int length) {
			int startIndex = (int)((srcOffset-1) / this.MAX_SIZE);
			int endIndex = (int)(((long)srcOffset+length-1) / this.MAX_SIZE);

			if(startIndex == endIndex) {
				this.pieces[startIndex].CopyTo(dest, (int)(srcOffset-(startIndex* this.MAX_SIZE)), destOffset, length);
			} else {
				int startOffset = (int) (srcOffset - (startIndex * this.MAX_SIZE));
				int endOffset = (int) (((long)srcOffset + length) - (endIndex * this.MAX_SIZE));

				int startLength = this.pieces[startIndex].Length - startOffset;
				this.pieces[startIndex].CopyTo(dest, startOffset, destOffset, startLength);
				
				this.pieces[endIndex].CopyTo(dest, 0, startLength, endOffset);
			}

			return Task.CompletedTask;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task CopyFrom(SafeArrayHandle src, int srcOffset, long destOffset, int length) {
			return this.CopyFrom(src.Entry, srcOffset, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task CopyFrom(ByteArray src, int srcOffset, long destOffset, int length) {
			int startIndex = (int)((destOffset-1) / this.MAX_SIZE);
			int endIndex = (int)(((long)destOffset+length-1) / this.MAX_SIZE);
			
			if(startIndex == endIndex) {
				
				this.pieces[startIndex].CopyFrom(src, srcOffset, (int)(destOffset-(startIndex* this.MAX_SIZE)), length);
			} else {
				int startOffset = (int) (destOffset - (startIndex * this.MAX_SIZE));
				int endOffset = (int) (((long)destOffset + length) - (endIndex * this.MAX_SIZE));

				int startLength = this.pieces[startIndex].Length - startOffset;
				this.pieces[startIndex].CopyFrom(src, srcOffset, startOffset, startLength);
				
				this.pieces[endIndex].CopyFrom(src, startLength, 0, endOffset);
			}
			
			return Task.CompletedTask;
		}
		
	#region Dispose

		public bool IsDisposed { get; private set; }

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {

			if(disposing && !this.IsDisposed) {

				foreach(var entry in this.pieces) {
					try {
						entry.Dispose();
					} catch(Exception ex) {
					}
				}
			}

			this.IsDisposed = true;
		}

		~VeryLargeByteArray() {
			this.Dispose(false);
		}

	#endregion
	}
}