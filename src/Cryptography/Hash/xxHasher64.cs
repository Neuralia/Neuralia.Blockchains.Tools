using System;

using Neuralia.Blockchains.Tools.Data;
using Neuralia.Data.HashFunction.xxHash;

namespace Neuralia.Blockchains.Tools.Cryptography.Hash {
	public class xxHasher64 : xxHasher<long> {

		protected override xxHashConfig CreatexxHashConfig() {
			return new xxHashConfig {HashSizeInBits = 64, Seed = 4745261967123280399UL};
		}

		public override long Hash(IByteArray message) {
			IByteArray hash = this.HashToBytes(message);
			long result = BitConverter.ToInt64(hash.Bytes, hash.Offset);
			hash.Return();

			return result;
		}

		public override long Hash(in Span<byte> message) {
			IByteArray hash = this.HashToBytes(message);
			long result = BitConverter.ToInt64(hash.Bytes, hash.Offset);
			hash.Return();

			return result;
		}

		public override long Hash(byte[] message) {
			IByteArray hash = this.HashToBytes(message);
			long result = BitConverter.ToInt64(hash.Bytes, hash.Offset);
			hash.Return();

			return result;
		}

		public long HashLong(IByteArray message) {
			IByteArray hash = this.HashToBytes(message);
			long result = BitConverter.ToInt64(hash.Bytes, hash.Offset);
			hash.Return();

			return result;
		}

		public long HashLong(byte[] message) {
			IByteArray hash = this.HashToBytes(message);
			long result = BitConverter.ToInt64(hash.Bytes, hash.Offset);
			hash.Return();

			return result;
		}

		public long HashLong(in Span<byte> message) {
			IByteArray hash = this.HashToBytes(message);
			long result = BitConverter.ToInt64(hash.Bytes, hash.Offset);
			hash.Return();

			return result;
		}

		public ulong HashULong(IByteArray message) {
			IByteArray hash = this.HashToBytes(message);
			ulong result = BitConverter.ToUInt64(hash.Bytes, hash.Offset);
			hash.Return();

			return result;
		}

		public ulong HashULong(byte[] message) {
			IByteArray hash = this.HashToBytes(message);
			ulong result = BitConverter.ToUInt64(hash.Bytes, hash.Offset);
			hash.Return();

			return result;
		}

		public ulong HashULong(in Span<byte> message) {
			IByteArray hash = this.HashToBytes(message);
			ulong result = BitConverter.ToUInt64(hash.Bytes, hash.Offset);
			hash.Return();

			return result;
		}
	}
}