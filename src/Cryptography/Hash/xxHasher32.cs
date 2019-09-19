using System;

using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Serialization;
using Neuralia.Data.HashFunction.xxHash;

namespace Neuralia.Blockchains.Tools.Cryptography.Hash {
	public class xxHasher32 : xxHasher<int> {

		protected override xxHashConfig CreatexxHashConfig() {
			return new xxHashConfig {HashSizeInBits = 32, Seed = 4745261967123280399UL};
		}

		public override int Hash(SafeArrayHandle message) {
			SafeArrayHandle hash = this.HashToBytes(message);
			TypeSerializer.Deserialize(hash.Bytes, hash.Offset, out int result);
			hash.Return();

			return result;
		}

		public override int Hash(byte[] message) {
			SafeArrayHandle hash = this.HashToBytes(message);
			TypeSerializer.Deserialize(hash.Bytes, hash.Offset, out int result);
			hash.Return();

			return result;
		}

		public override int Hash(in Span<byte> message) {
			SafeArrayHandle hash = this.HashToBytes(message);
			TypeSerializer.Deserialize(hash.Bytes, hash.Offset, out int result);
			hash.Return();

			return result;
		}

		public uint HashUInt(SafeArrayHandle message) {
			SafeArrayHandle hash = this.HashToBytes(message);
			TypeSerializer.Deserialize(hash.Bytes, hash.Offset, out uint result);
			hash.Return();

			return result;
		}

		public uint HashUInt(byte[] message) {
			SafeArrayHandle hash = this.HashToBytes(message);
			TypeSerializer.Deserialize(hash.Bytes, hash.Offset, out uint result);
			hash.Return();

			return result;
		}

		public uint HashUInt(in Span<byte> message) {
			SafeArrayHandle hash = this.HashToBytes(message);
			TypeSerializer.Deserialize(hash.Bytes, hash.Offset, out uint result);
			hash.Return();

			return result;
		}
	}
}