using Neuralia.Blockchains.Tools.Data;

namespace Neuralia.Blockchains.Tools.Cryptography.Hash {
	public interface IHasher<out T> : IDisposable2 {
		T Hash(IByteArray wrapper);
		T Hash(byte[] message);
		T HashTwo(IByteArray message1, IByteArray message2);
		T HashTwo(IByteArray message1, short message2);
		T HashTwo(IByteArray message1, int message2);
		T HashTwo(IByteArray message1, long message2);
		T HashTwo(short message1, short message2);
		T HashTwo(ushort message1, ushort message2);
		T HashTwo(ushort message1, long message2);
		T HashTwo(int message1, int message2);
		T HashTwo(uint message1, uint message2);
		T HashTwo(long message1, long message2);
		T HashTwo(ulong message1, ulong message2);
	}
}