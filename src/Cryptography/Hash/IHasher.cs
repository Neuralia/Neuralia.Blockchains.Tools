using Neuralia.Blockchains.Tools.Data;

namespace Neuralia.Blockchains.Tools.Cryptography.Hash {
	public interface IHasher<out T> : IDisposableExtended {
		T Hash(SafeArrayHandle wrapper);
		T Hash(byte[] message);
		T HashTwo(SafeArrayHandle message1, SafeArrayHandle message2);
		T HashTwo(SafeArrayHandle message1, short message2);
		T HashTwo(SafeArrayHandle message1, int message2);
		T HashTwo(SafeArrayHandle message1, long message2);
		T HashTwo(short message1, short message2);
		T HashTwo(ushort message1, ushort message2);
		T HashTwo(ushort message1, long message2);
		T HashTwo(int message1, int message2);
		T HashTwo(uint message1, uint message2);
		T HashTwo(long message1, long message2);
		T HashTwo(ulong message1, ulong message2);
	}
}