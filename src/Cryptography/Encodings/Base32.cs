namespace Neuralia.Blockchains.Tools.Cryptography.Encodings {

	public class Base32 : BaseEncoder {

		protected override string Digits => "234567ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		protected override string PrepareDecodeString(string value) {
			return value.ToUpper();
		}
	}

}