namespace Neuralia.Blockchains.Tools.Cryptography.Encodings {
	public class Base30 : BaseEncoder {
		protected override string Digits => "23456789ABCDEFGHJKMNPRSTUVWXYZ";

		protected override string PrepareDecodeString(string value) {
			return value.ToUpper();
		}
	}
}