using System;

namespace Neuralia.Blockchains.Core.Cryptography.xxHash {
	public class xxHash32 : xxHash<uint> {

		private static readonly uint[] primes32 = {2654435761U, 2246822519U, 3266489917U, 668265263U, 374761393U};

		public xxHash32() : this(2744273497U) {

		}
		
		public xxHash32(uint seed) : base(seed) {

		}

		public override void Hash(in Span<byte> data, in Span<byte> hash) {

			const int sliceSize = 16;
			uint temp = this.seed + primes32[4];

			uint[] strides = {this.seed + primes32[0] + primes32[1], this.seed + primes32[1], this.seed, this.seed - primes32[0]};

			long dataCount = 0;
			Span<byte> remainder = stackalloc byte[0];

			int remainderLength = data.Length % sliceSize;

			int mainLength = data.Length - remainderLength;

			if(mainLength > 0) {
				for(var x = 0; x < mainLength; x += sliceSize) {
					for(var y = 0; y < 4; ++y) {
						strides[y] += xxHashUtils.Deserialize32(data, x + (y * 4)) * primes32[1];
						strides[y] = xxHashUtils.RotateLeft(strides[y], 13);
						strides[y] *= primes32[0];
					}
				}

				dataCount += mainLength;
			}

			if(remainderLength > 0) {
				remainder = stackalloc byte[remainderLength];

				data.Slice(mainLength, remainderLength).CopyTo(remainder);

				dataCount += remainderLength;
			}

			// now post process
			if(dataCount >= sliceSize) {
				temp = xxHashUtils.RotateLeft(strides[0], 1) + xxHashUtils.RotateLeft(strides[1], 7) + xxHashUtils.RotateLeft(strides[2], 12) + xxHashUtils.RotateLeft(strides[3], 18);
			}

			temp += (uint) dataCount;

			if(remainder.Length != 0) {
				// In 4-byte chunks, transform all full chunks
				for(var x = 0; x < (remainder.Length / 4); ++x) {
					temp += xxHashUtils.Deserialize32(remainder, x * 4) * primes32[2];
					temp = xxHashUtils.RotateLeft(temp, 17) * primes32[3];
				}

				// Transform remainder
				for(int x = remainder.Length - (remainder.Length % 4); x < remainder.Length; ++x) {
					temp += remainder[x] * primes32[4];
					temp = xxHashUtils.RotateLeft(temp, 11) * primes32[0];
				}
			}

			temp ^= temp >> 15;
			temp *= primes32[1];
			temp ^= temp >> 13;
			temp *= primes32[2];
			temp ^= temp >> 16;

			// now extract our hash
			xxHashUtils.Serialize(temp, hash);
		}

		public override int HashSize => sizeof(uint);
	}
}