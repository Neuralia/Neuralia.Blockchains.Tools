using System;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Serialization;
using Neuralia.Data.HashFunction.xxHash;

namespace Neuralia.Blockchains.Tools.Cryptography.Hash {
	public abstract class xxHasher<T> : IHasher<T> {
		private readonly IxxHash hasher;

		public xxHasher() {
			xxHashConfig XxHashConfig = this.CreatexxHashConfig();

			this.hasher = xxHashFactory.Instance.Create(XxHashConfig);
		}

		public abstract T Hash(SafeArrayHandle wrapper);
		public abstract T Hash(byte[] message);

		public T HashTwo(SafeArrayHandle message1, SafeArrayHandle message2) {
			int len1 = 0;

			if(message1 != null) {
				len1 = message1.Length;
			}

			int len2 = 0;

			if(message2 != null) {
				len2 = message2.Length;
			}

			SafeArrayHandle buffer = ByteArray.Create(len1 + len2);

			if(message1 != null) {
				Buffer.BlockCopy(message1.Bytes, message1.Offset, buffer.Bytes, buffer.Offset, len1);
			}

			if(message2 != null) {
				Buffer.BlockCopy(message2.Bytes, message2.Offset, buffer.Bytes, buffer.Offset + len1, len2);
			}

			// do the hash
			return this.Hash(buffer);
		}

		public T HashTwo(SafeArrayHandle message1, short message2) {
			return this.HashTwo(message1, TypeSerializer.Serialize(message2));
		}

		public T HashTwo(SafeArrayHandle message1, int message2) {
			return this.HashTwo(message1, TypeSerializer.Serialize(message2));
		}

		public T HashTwo(SafeArrayHandle message1, long message2) {
			return this.HashTwo(message1, TypeSerializer.Serialize(message2));
		}

		public T HashTwo(short message1, short message2) {
			return this.HashTwo(TypeSerializer.Serialize(message1), TypeSerializer.Serialize(message2));
		}

		public T HashTwo(ushort message1, ushort message2) {
			return this.HashTwo(TypeSerializer.Serialize(message1), TypeSerializer.Serialize(message2));
		}

		public T HashTwo(ushort message1, long message2) {
			return this.HashTwo(TypeSerializer.Serialize(message1), TypeSerializer.Serialize(message2));
		}

		public T HashTwo(int message1, int message2) {
			return this.HashTwo(TypeSerializer.Serialize(message1), TypeSerializer.Serialize(message2));
		}

		public T HashTwo(uint message1, uint message2) {
			return this.HashTwo(TypeSerializer.Serialize(message1), TypeSerializer.Serialize(message2));
		}

		public T HashTwo(long message1, long message2) {
			return this.HashTwo(TypeSerializer.Serialize(message1), TypeSerializer.Serialize(message2));
		}

		public T HashTwo(ulong message1, ulong message2) {
			return this.HashTwo(TypeSerializer.Serialize(message1), TypeSerializer.Serialize(message2));
		}

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public bool IsDisposed { get; private set; }

		public T HashTwo(SafeArrayHandle message1, ulong message2) {
			return this.HashTwo(message1, TypeSerializer.Serialize(message2));
		}

		public abstract T Hash(in Span<byte> message);

		protected abstract xxHashConfig CreatexxHashConfig();

		protected SafeArrayHandle HashToBytes(SafeArrayHandle buffer) {
			// if we get here, our actual data buffer is larger than the length we use. our data is rented. we have no choice but to copy.
			return this.hasher.ComputeHash(buffer.Bytes, buffer.Offset, buffer.Length).Hash;
		}

		protected SafeArrayHandle HashToBytes(in Span<byte> message) {

			return this.hasher.ComputeHash(message.ToArray()).Hash;
		}

		protected SafeArrayHandle HashToBytes(byte[] message) {

			return this.hasher.ComputeHash(message).Hash;
		}

		protected SafeArrayHandle HashToBytes(byte[] message, int offset, int length) {

			return this.hasher.ComputeHash(message, offset, length).Hash;
		}

		protected virtual void Dispose(bool disposing) {
			if(disposing && !this.IsDisposed) {

			}

			this.IsDisposed = true;
		}
	}
}