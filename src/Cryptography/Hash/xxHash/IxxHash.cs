using System;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Data.Arrays;

namespace Neuralia.Blockchains.Core.Cryptography.xxHash {
	public interface IxxHash {
		void Hash(in Span<byte> data, in Span<byte> hash);
		ByteArray Hash(in Span<byte> data);
		int HashSize { get; }
		
	}
}