using System;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Data.Arrays;

namespace Neuralia.Blockchains.Core.Cryptography.xxHash {
	public abstract class xxHash<T> : IxxHash {
		protected readonly T seed;
		
		public xxHash(T seed) {
			this.seed = seed;
		}
		
		public abstract void Hash(in Span<byte> data, in Span<byte> hash);

		/// <summary>
		/// careful! its faster to use the span hash version
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public ByteArray Hash(in Span<byte> data) {
			ByteArray hash = ByteArray.Create(this.HashSize);
			this.Hash(data, hash.Span);

			return hash;
		}
		
		/// <summary>
		/// the byte size of the current hash
		/// </summary>
		public abstract int HashSize { get; }
	}
}