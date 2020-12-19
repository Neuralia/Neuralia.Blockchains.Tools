using System;
using Neuralia.Blockchains.Core.Cryptography.xxHash;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Data.Arrays;
using Neuralia.Blockchains.Tools.Serialization;

namespace Neuralia.Blockchains.Tools.Cryptography.Hash {
	public class xxHasher32 : xxHasher<int, xxHash32> {
		
		public override int Hash(in Span<byte> message) {

			return this.HashInt(message);
		}

		public override int Hash(SafeArrayHandle wrapper) {
			return this.HashInt(wrapper.Span);
		}

		public override int Hash(byte[] message) {

			return this.Hash(message.AsSpan());
		}

		public int HashInt(SafeArrayHandle message) {
			return this.HashInt(message.Span);
		}

		public int HashInt(byte[] message) {
			return this.HashInt(message.AsSpan());
		}

		public int HashInt(in Span<byte> message) {
			Span<byte> hash = stackalloc byte[sizeof(int)];
			this.HashToBytes(message, hash);
			TypeSerializer.Deserialize(hash, out int result);

			return result;
		}

		public uint HashUInt(SafeArrayHandle message) {
			return this.HashUInt(message.Span);
		}

		public uint HashUInt(byte[] message) {
			return this.HashUInt(message.AsSpan());
		}

		public uint HashUInt(in Span<byte> message) {
			Span<byte> hash = stackalloc byte[sizeof(int)];
			this.HashToBytes(message, hash);
			TypeSerializer.Deserialize(hash, out uint result);

			return result;
		}
	}
}