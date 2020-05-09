using System;
using Neuralia.Blockchains.Tools.Data;

namespace Neuralia.Blockchains.Core.Cryptography.xxHash {
	public interface IxxHash {
		void Hash(in Span<byte> data, in Span<byte> hash);
		SafeArrayHandle Hash(in Span<byte> data);
		int HashSize { get; }
		
	}
}