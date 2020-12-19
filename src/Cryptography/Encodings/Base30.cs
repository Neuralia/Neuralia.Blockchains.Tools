using System.Data;

namespace Neuralia.Blockchains.Tools.Cryptography.Encodings {
	public class Base30 : BaseEncoder {

		//excludes 0, 1, L, V, I, O
		public const string Base30_Tokens = "YBNDRFG8EJKMCPQXTVWSZA345H7692";
		
		protected override string Digits => Base30_Tokens;

		protected override string PrepareDecodeString(string value) {
			return value.Trim().ToUpper();
		}
	}
}