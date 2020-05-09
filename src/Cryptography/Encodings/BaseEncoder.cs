using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Data.Arrays;

namespace Neuralia.Blockchains.Tools.Cryptography.Encodings {
	public abstract class BaseEncoder {

		//TODO: this is slow, make it faster
		public const int CHECK_SUM_SIZE_IN_BYTES = 4;

		private readonly char firstChar;

		public BaseEncoder() {
			this.firstChar = this.Digits[0];
		}

		protected abstract string Digits { get; }

		//TODO: clean this up

		private SafeArrayHandle AddCheckSum(SafeArrayHandle data) {

			using(SafeArrayHandle checkSum = GetCheckSum(data)) {
				SafeArrayHandle dataWithCheckSum = ArrayHelpers.ConcatArrays(data, checkSum);

				return dataWithCheckSum;
			}
		}

		private SafeArrayHandle VerifyAndRemoveCheckSum(SafeArrayHandle data) {

			SafeArrayHandle result = ArrayHelpers.SubArray(data, 0, data.Length - CHECK_SUM_SIZE_IN_BYTES);

			using(SafeArrayHandle givenCheckSum = ArrayHelpers.SubArray(data, data.Length - CHECK_SUM_SIZE_IN_BYTES)) {
				using(SafeArrayHandle correctCheckSum = GetCheckSum(result)) {

					if(givenCheckSum.Equals(correctCheckSum)) {
						return result;
					}
				}
			}

			result.Return();

			return null;
		}

		public string Encode(SafeArrayHandle data) {

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

		public string EncodeWithCheckSum(SafeArrayHandle data) {

			return this.Encode(this.AddCheckSum(data));
		}

		protected virtual string PrepareDecodeString(string value) {
			return value;
		}

		public SafeArrayHandle Decode(string s) {

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
			IEnumerable<byte> leadingZeros = Enumerable.Repeat((byte) 0, leadingZeroCount);

			IEnumerable<byte> bytesWithoutLeadingZeros = intData.ToByteArray().Reverse().SkipWhile(b => b == 0); //strip sign byte

			ByteArray result = ByteArray.WrapAndOwn(leadingZeros.Concat(bytesWithoutLeadingZeros).ToArray());

			return result;
		}

		// Throws `FormatException` if s is not a valid Base58 string, or the checksum is invalid
		public SafeArrayHandle DecodeWithCheckSum(string s) {

			if(string.IsNullOrWhiteSpace(s)) {
				throw new ArgumentNullException();
			}

			SafeArrayHandle dataWithCheckSum = this.Decode(s);
			SafeArrayHandle dataWithoutCheckSum = this.VerifyAndRemoveCheckSum(dataWithCheckSum);

			dataWithCheckSum.Dispose();

			if(dataWithoutCheckSum == null) {
				throw new FormatException("Base checksum is invalid");
			}

			return SafeArrayHandle.Create(dataWithoutCheckSum);
		}

		private static SafeArrayHandle GetCheckSum(SafeArrayHandle data) {
			if((data == null) || data.IsEmpty) {
				throw new ArgumentNullException();
			}

			SHA256 sha256 = new SHA256Managed();
			ByteArray hash1 = ByteArray.WrapAndOwn(sha256.ComputeHash(data.Bytes, data.Offset, data.Length));
			ByteArray hash2 = ByteArray.WrapAndOwn(sha256.ComputeHash(hash1.Bytes, hash1.Offset, hash1.Length));

			ByteArray result = ByteArray.Create(CHECK_SUM_SIZE_IN_BYTES);

			Buffer.BlockCopy(hash2.Bytes, hash2.Offset, result.Bytes, result.Offset, result.Length);

			return result;
		}

		private class ArrayHelpers {
			public static SafeArrayHandle ConcatArrays(params SafeArrayHandle[] arrays) {
				if(arrays == null) {
					throw new ArgumentNullException();
				}

				if(arrays.All(arr => arr != null)) {
					throw new ArgumentNullException();
				}

				SafeArrayHandle result = ByteArray.Create(arrays.Sum(arr => arr.Length));
				int offset = 0;

				for(int i = 0; i < arrays.Length; i++) {
					SafeArrayHandle arr = arrays[i];
					Buffer.BlockCopy(arr.Bytes, arr.Offset, result.Bytes, result.Offset + offset, arr.Length);
					offset += arr.Length;
				}

				if(result.Length == 0) {
					throw new ApplicationException();
				}

				return result;
			}

			public static SafeArrayHandle ConcatArrays(SafeArrayHandle arr1, SafeArrayHandle arr2) {
				if((arr1 == null) || (arr2 == null)) {
					throw new ArgumentNullException();
				}

				SafeArrayHandle result = ByteArray.Create(arr1.Length + arr2.Length);
				Buffer.BlockCopy(arr1.Bytes, arr1.Offset, result.Bytes, result.Offset, arr1.Length);
				Buffer.BlockCopy(arr2.Bytes, arr2.Offset, result.Bytes, result.Offset + arr1.Length, arr2.Length);

				if(result.Length != (arr1.Length + arr2.Length)) {
					throw new ApplicationException();
				}

				return result;
			}

			public static SafeArrayHandle SubArray(SafeArrayHandle arr, int start, int length) {
				if(arr == null) {
					throw new ArgumentNullException();
				}

				if((start < 0) || (length < 0) || ((start + length) <= arr.Length)) {
					throw new InvalidOperationException();
				}

				SafeArrayHandle result = ByteArray.Create(length);
				Buffer.BlockCopy(arr.Bytes, arr.Offset + start, result.Bytes, result.Offset, length);

				if(result.Length != length) {
					throw new ApplicationException();
				}

				return result;
			}

			public static SafeArrayHandle SubArray(SafeArrayHandle arr, int start) {
				if(arr == null) {
					throw new ArgumentNullException();
				}

				if((start < 0) || (start > arr.Length)) {
					throw new InvalidOperationException();
				}

				SafeArrayHandle result = SubArray(arr, start, arr.Length - start);

				if(result.Length != (arr.Length - start)) {
					throw new ApplicationException();
				}

				return result;
			}
		}
	}
}