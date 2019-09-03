namespace Neuralia.Blockchains.Tools.Cryptography.Encodings {
	public class Base58 : BaseEncoder {

		protected override string Digits => "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
	}
}