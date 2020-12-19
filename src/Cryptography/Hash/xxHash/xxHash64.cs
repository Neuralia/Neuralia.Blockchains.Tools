using System;

namespace Neuralia.Blockchains.Core.Cryptography.xxHash {
	public class xxHash64 : xxHash<ulong> {

		private static readonly ulong[] primes64 = {11400714785074694791UL, 14029467366897019727UL, 1609587929392839161UL, 9650029242287828579UL, 2870177450012600261UL};

		public xxHash64() : this(4745261967123280399UL) {

		}
		
		public xxHash64(ulong seed) : base(seed) {

		}
		
		public override int HashSize => sizeof(ulong);

		public override void Hash(in Span<byte> data, in Span<byte> hash) {
			const int sliceSize = 32;
			ulong temp = this.seed + primes64[4];

			ulong[] strides = {this.seed + primes64[0] + primes64[1], this.seed + primes64[1], this.seed, this.seed - primes64[0]};

			long dataCount = 0;
			Span<byte> remainder = stackalloc byte[0];

			int remainderLength = data.Length % sliceSize;

			int mainLength = data.Length - remainderLength;

			if(mainLength > 0) {
				for(var x = 0; x < mainLength; x += sliceSize) {
					for(var y = 0; y < 4; ++y) {
						strides[y] += xxHashUtils.Deserialize64(data, x + (y * 8)) * primes64[1];
						strides[y] = xxHashUtils.RotateLeft(strides[y], 31);
						strides[y] *= primes64[0];
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

				for(var x = 0; x < strides.Length; ++x) {
					strides[x] *= primes64[1];
					strides[x] = xxHashUtils.RotateLeft(strides[x], 31);
					strides[x] *= primes64[0];

					temp ^= strides[x];
					temp = (temp * primes64[0]) + primes64[3];
				}
			}

			temp += (ulong) dataCount;

			if(remainder != null) {
				// In 8-byte chunks, process all full chunks
				for(var x = 0; x < (remainder.Length / 8); ++x) {
					temp ^= xxHashUtils.RotateLeft(xxHashUtils.Deserialize64(remainder, x * 8) * primes64[1], 31) * primes64[0];
					temp = (xxHashUtils.RotateLeft(temp, 27) * primes64[0]) + primes64[3];
				}

				// Process a 4-byte chunk if it exists
				if((remainder.Length % 8) >= 4) {
					temp ^= xxHashUtils.Deserialize32(remainder, remainder.Length - (remainder.Length % 8)) * primes64[0];
					temp = (xxHashUtils.RotateLeft(temp, 23) * primes64[1]) + primes64[2];
				}

				// Process last 4 bytes in 1-byte chunks (only runs if data.Length % 4 != 0)
				for(int x = remainder.Length - (remainder.Length % 4); x < remainder.Length; ++x) {
					temp ^= remainder[x] * primes64[4];
					temp = xxHashUtils.RotateLeft(temp, 11) * primes64[0];
				}
			}

			temp ^= temp >> 33;
			temp *= primes64[1];
			temp ^= temp >> 29;
			temp *= primes64[2];
			temp ^= temp >> 32;

			// now extract our hash
			xxHashUtils.Serialize(temp, hash);
		}

		public void Hash64(in Span<byte> data, in Span<byte> hash, ulong seed) {

		}
	}
}