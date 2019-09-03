using System;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.System.Data.HashFunction.xxHash;

namespace Neuralia.Blockchains.Tools.Cryptography.Hash {
	public class xxHasher32 : xxHasher<int> {

		protected override xxHashConfig CreatexxHashConfig() {
			return new xxHashConfig {HashSizeInBits = 32, Seed = 4745261967123280399UL};
		}

		public override int Hash(IByteArray message) {
			IByteArray hash = this.HashToBytes(message);
			int result = BitConverter.ToInt32(hash.Bytes, hash.Offset);
			hash.Return();

			return result;
		}

		public override int Hash(byte[] message) {
			IByteArray hash = this.HashToBytes(message);
			int result = BitConverter.ToInt32(hash.Bytes, hash.Offset);
			hash.Return();

			return result;
		}

		public override int Hash(in Span<byte> message) {
			IByteArray hash = this.HashToBytes(message);
			int result = BitConverter.ToInt32(hash.Bytes, hash.Offset);
			hash.Return();

			return result;
		}

		public uint HashUInt(IByteArray message) {
			IByteArray hash = this.HashToBytes(message);
			uint result = BitConverter.ToUInt32(hash.Bytes, hash.Offset);
			hash.Return();

			return result;
		}

		public uint HashUInt(byte[] message) {
			IByteArray hash = this.HashToBytes(message);
			uint result = BitConverter.ToUInt32(hash.Bytes, hash.Offset);
			hash.Return();

			return result;
		}

		public uint HashUInt(in Span<byte> message) {
			IByteArray hash = this.HashToBytes(message);
			uint result = BitConverter.ToUInt32(hash.Bytes, hash.Offset);
			hash.Return();

			return result;
		}
	}
}