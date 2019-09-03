using System.Collections.Generic;

namespace Neuralia.Blockchains.Tools.Cryptography.Encodings {
	public class Base85 : BaseEncoder {
		public static readonly string Digits85;

		static Base85() {
			var printableChars = new List<char>();

			for(char i = char.MinValue; i <= '\u007F'; i++) {

				if(!char.IsControl(i) && !char.IsSymbol(i) && (i != (char) 32)) {
					printableChars.Add(i);
				}
			}

			Digits85 = string.Join("", printableChars);
		}

		protected override string Digits => Digits85;
	}
}