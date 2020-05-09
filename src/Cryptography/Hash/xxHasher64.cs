using System;
using Neuralia.Blockchains.Core.Cryptography.xxHash;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Serialization;

namespace Neuralia.Blockchains.Tools.Cryptography.Hash {
	public class xxHasher64 : xxHasher<long, xxHash64> {

		public override long Hash(SafeArrayHandle message) {
			SafeArrayHandle hash = this.HashToBytes(message);

			TypeSerializer.Deserialize(hash.Bytes, hash.Offset, out long result);
			hash.Return();

			return result;
		}

		public override long Hash(in Span<byte> message) {

			return this.HashLong(message);
		}

		public override long Hash(byte[] message) {

			return this.Hash(message.AsSpan());
		}

		public long HashLong(SafeArrayHandle message) {
			return this.HashLong(message.Span);
		}

		public long HashLong(byte[] message) {
			return this.HashLong(message.AsSpan());
		}

		public long HashLong(in Span<byte> message) {
			Span<byte> hash = stackalloc byte[sizeof(long)];
			this.HashToBytes(message, hash);
			TypeSerializer.Deserialize(hash, out long result);

			return result;
		}

		public ulong HashULong(SafeArrayHandle message) {
			return this.HashULong(message.Span);
		}

		public ulong HashULong(byte[] message) {
			return this.HashULong(message.AsSpan());
		}

		public ulong HashULong(in Span<byte> message) {
			Span<byte> hash = stackalloc byte[sizeof(long)];
			this.HashToBytes(message, hash);
			TypeSerializer.Deserialize(hash, out ulong result);

			return result;
		}
	}
}