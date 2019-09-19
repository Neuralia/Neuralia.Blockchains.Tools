using System;

using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Serialization;
using Neuralia.Data.HashFunction.xxHash;

namespace Neuralia.Blockchains.Tools.Cryptography.Hash {
	public class xxHasher64 : xxHasher<long> {

		protected override xxHashConfig CreatexxHashConfig() {
			return new xxHashConfig {HashSizeInBits = 64, Seed = 4745261967123280399UL};
		}

		public override long Hash(SafeArrayHandle message) {
			SafeArrayHandle hash = this.HashToBytes(message);
			
			TypeSerializer.Deserialize(hash.Bytes, hash.Offset, out long result);
			hash.Return();

			return result;
		}

		public override long Hash(in Span<byte> message) {
			SafeArrayHandle hash = this.HashToBytes(message);
			TypeSerializer.Deserialize(hash.Bytes, hash.Offset, out long result);
			hash.Return();

			return result;
		}

		public override long Hash(byte[] message) {
			SafeArrayHandle hash = this.HashToBytes(message);
			TypeSerializer.Deserialize(hash.Bytes, hash.Offset, out long result);
			hash.Return();

			return result;
		}

		public long HashLong(SafeArrayHandle message) {
			SafeArrayHandle hash = this.HashToBytes(message);
			TypeSerializer.Deserialize(hash.Bytes, hash.Offset, out long result);
			hash.Return();

			return result;
		}

		public long HashLong(byte[] message) {
			SafeArrayHandle hash = this.HashToBytes(message);
			TypeSerializer.Deserialize(hash.Bytes, hash.Offset, out long result);
			hash.Return();

			return result;
		}

		public long HashLong(in Span<byte> message) {
			SafeArrayHandle hash = this.HashToBytes(message);
			TypeSerializer.Deserialize(hash.Bytes, hash.Offset, out long result);
			hash.Return();

			return result;
		}

		public ulong HashULong(SafeArrayHandle message) {
			SafeArrayHandle hash = this.HashToBytes(message);
			TypeSerializer.Deserialize(hash.Bytes, hash.Offset, out ulong result);
			hash.Return();

			return result;
		}

		public ulong HashULong(byte[] message) {
			SafeArrayHandle hash = this.HashToBytes(message);
			TypeSerializer.Deserialize(hash.Bytes, hash.Offset, out ulong result);
			hash.Return();

			return result;
		}

		public ulong HashULong(in Span<byte> message) {
			SafeArrayHandle hash = this.HashToBytes(message);
			TypeSerializer.Deserialize(hash.Bytes, hash.Offset, out ulong result);
			hash.Return();

			return result;
		}
	}
}