namespace Neuralia.Blockchains.Tools.Cryptography.Encodings {
	public class Base35 : BaseEncoder {
		protected override string Digits => "123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		protected override string PrepareDecodeString(string value) {
			return value.ToUpper();
		}
	}
}