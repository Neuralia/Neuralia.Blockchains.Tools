using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Data.Allocation;

namespace Neuralia.Blockchains.Tools.Cryptography.Encodings {
	public abstract class BaseEncoder {

		public const int CHECK_SUM_SIZE_IN_BYTES = 4;

		private readonly char firstChar;

		public BaseEncoder() {
			this.firstChar = this.Digits[0];
		}

		protected abstract string Digits { get; }

		//TODO: clean this up

		private IByteArray AddCheckSum(IByteArray data) {

			using(IByteArray checkSum = GetCheckSum(data)) {
				IByteArray dataWithCheckSum = ArrayHelpers.ConcatArrays(data, checkSum);

				return dataWithCheckSum;
			}
		}

		private IByteArray VerifyAndRemoveCheckSum(IByteArray data) {

			IByteArray result = (ByteArray) ArrayHelpers.SubArray(data, 0, data.Length - CHECK_SUM_SIZE_IN_BYTES);

			using(IByteArray givenCheckSum = ArrayHelpers.SubArray(data, data.Length - CHECK_SUM_SIZE_IN_BYTES)) {
				using(IByteArray correctCheckSum = GetCheckSum(result)) {

					if(givenCheckSum.Equals(correctCheckSum)) {
						return result;
					}
				}
			}

			result.Return();

			return null;
		}

		public string Encode(IByteArray data) {

			// Decode ByteArray to BigInteger
			BigInteger intData = 0;

			for(int i = 0; i < data.Length; i++) {
				intData = (intData * 256) + data[i];
			}

			// Encode BigInteger to Base58 string
			string result = "";

			while(intData > 0) {
				int remainder = (int) (intData % this.Digits.Length);
				intData /= this.Digits.Length;
				result = this.Digits[remainder] + result;
			}

			// Append the first digit for each leading 0 byte
			for(int i = 0; (i < data.Length) && (data[i] == 0); i++) {
				result = this.firstChar + result;
			}

			return result;
		}

		public string EncodeWithCheckSum(IByteArray data) {

			return this.Encode(this.AddCheckSum(data));
		}

		protected virtual string PrepareDecodeString(string value) {
			return value;
		}

		public IByteArray Decode(string s) {

			s = this.PrepareDecodeString(s);

			// Decode Base58 string to BigInteger 
			BigInteger intData = 0;

			for(int i = 0; i < s.Length; i++) {
				int digit = this.Digits.IndexOf(s[i]); //Slow

				if(digit < 0) {
					throw new FormatException($"Invalid Base character `{s[i]}` at position {i}");
				}

				intData = (intData * this.Digits.Length) + digit;
			}

			// Encode BigInteger to ByteArray
			// Leading zero bytes get encoded as leading digit characters
			int leadingZeroCount = s.TakeWhile(c => c == this.firstChar).Count();
			var leadingZeros = Enumerable.Repeat((byte) 0, leadingZeroCount);

			var bytesWithoutLeadingZeros = intData.ToByteArray().Reverse().SkipWhile(b => b == 0); //strip sign byte

			ByteArray result = leadingZeros.Concat(bytesWithoutLeadingZeros).ToArray();

			return result;
		}

		// Throws `FormatException` if s is not a valid Base58 string, or the checksum is invalid
		public IByteArray DecodeWithCheckSum(string s) {

			if(string.IsNullOrWhiteSpace(s)) {
				throw new ArgumentNullException();
			}

			IByteArray dataWithCheckSum = this.Decode(s);
			IByteArray dataWithoutCheckSum = this.VerifyAndRemoveCheckSum(dataWithCheckSum);

			dataWithCheckSum.Return();

			if(dataWithoutCheckSum == null) {
				throw new FormatException("Base checksum is invalid");
			}

			return dataWithoutCheckSum;
		}

		private static IByteArray GetCheckSum(IByteArray data) {
			if((data == null) || data.IsEmpty) {
				throw new ArgumentNullException();
			}

			SHA256 sha256 = new SHA256Managed();
			ByteArray hash1 = sha256.ComputeHash(data.Bytes, data.Offset, data.Length);
			ByteArray hash2 = sha256.ComputeHash(hash1.Bytes, hash1.Offset, hash1.Length);

			MemoryBlock result = MemoryAllocators.Instance.allocator.Take(CHECK_SUM_SIZE_IN_BYTES);

			Buffer.BlockCopy(hash2.Bytes, hash2.Offset, result.Bytes, result.Offset, result.Length);

			return result;
		}

		private class ArrayHelpers {
			public static IByteArray ConcatArrays(params IByteArray[] arrays) {
				if(arrays == null) {
					throw new ArgumentNullException();
				}

				if(arrays.All(arr => arr != null)) {
					throw new ArgumentNullException();
				}

				IByteArray result = MemoryAllocators.Instance.allocator.Take(arrays.Sum(arr => arr.Length));
				int offset = 0;

				for(int i = 0; i < arrays.Length; i++) {
					IByteArray arr = arrays[i];
					Buffer.BlockCopy(arr.Bytes, arr.Offset, result.Bytes, result.Offset + offset, arr.Length);
					offset += arr.Length;
				}

				if(result.Length == 0) {
					throw new ApplicationException();
				}

				return result;
			}

			public static IByteArray ConcatArrays(IByteArray arr1, IByteArray arr2) {
				if((arr1 == null) || (arr2 == null)) {
					throw new ArgumentNullException();
				}

				IByteArray result = MemoryAllocators.Instance.allocator.Take(arr1.Length + arr2.Length);
				Buffer.BlockCopy(arr1.Bytes, arr1.Offset, result.Bytes, result.Offset, arr1.Length);
				Buffer.BlockCopy(arr2.Bytes, arr2.Offset, result.Bytes, result.Offset + arr1.Length, arr2.Length);

				if(result.Length != (arr1.Length + arr2.Length)) {
					throw new ApplicationException();
				}

				return result;
			}

			public static IByteArray SubArray(IByteArray arr, int start, int length) {
				if(arr == null) {
					throw new ArgumentNullException();
				}

				if((start < 0) || (length < 0) || ((start + length) <= arr.Length)) {
					throw new InvalidOperationException();
				}

				IByteArray result = MemoryAllocators.Instance.allocator.Take(length);
				Buffer.BlockCopy(arr.Bytes, arr.Offset + start, result.Bytes, result.Offset, length);

				if(result.Length != length) {
					throw new ApplicationException();
				}

				return result;
			}

			public static IByteArray SubArray(IByteArray arr, int start) {
				if(arr == null) {
					throw new ArgumentNullException();
				}

				if((start < 0) || (start > arr.Length)) {
					throw new InvalidOperationException();
				}

				IByteArray result = SubArray(arr, start, arr.Length - start);

				if(result.Length != (arr.Length - start)) {
					throw new ApplicationException();
				}

				return result;
			}
		}
	}
}