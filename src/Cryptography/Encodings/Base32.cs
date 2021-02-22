using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuralia.Blockchains.Tools.Data.Arrays;
using Neuralia.Blockchains.Tools.Extensions;
using Neuralia.Blockchains.Tools.Serialization;

namespace Neuralia.Blockchains.Tools.Cryptography.Encodings {

	public class Base32 {
		
		// customized z-base32. excludes 0, 1, U, I. 0 => O, 1,I => L, U => V
		public const string Base32_Tokens = "YBNDRFG8EJKMCPQXOTLVWSZA345H7692";
		private static readonly Dictionary<char, int> TokenIndices = Base32_Tokens.Select((c, i) => (c, i)).ToDictionary(c => c.c, c => c.i);

		private const int SLIDE = 5;
		private const int MAX_BYTES = 5;
		private const int MAX_CHARACTERS = 8;
		private const byte MASK = 0x1F;

		public static string Encode(ByteArray bytes) {
			Span<byte> buffer = stackalloc byte[8];
			
			int length = bytes.Length;

			Span<char> characters = new char[(int)Math.Ceiling((double)(length*8)/5)];
			int index = 0;

			for(var offset = 0; offset < length; offset += MAX_BYTES) {

				int stride = Math.Min(MAX_BYTES, length - offset);

				buffer.Clear();
				bytes.Span.Slice(offset, stride).CopyTo(buffer);
				TypeSerializer.Deserialize(buffer, out ulong workspace);

				var take = (int) Math.Ceiling(((double) stride * 8) / 5);

				for(var j = 0; j < take; j++) {
					characters[index++] = Base32_Tokens[(byte) (workspace & MASK)];
					workspace >>= SLIDE;
				}
			}
			
			// make sure to trim 0s at the end of the string
			return characters.TrimEnd(Base32_Tokens[0]).ToString();
		}

		public static ByteArray Decode(string base32) {

			if(string.IsNullOrWhiteSpace(base32)) {
				return null;
			}
			string base32fixed = Prepare(base32);
			Span<byte> buffer = stackalloc byte[8];
			int length = base32fixed.Length;
			var bytes = ByteArray.Create((int) Math.Ceiling(((double) length * SLIDE) / 8));

			var byteOffset = 0;

			for(var offset = 0; offset < length; offset += MAX_CHARACTERS) {
				int stride = Math.Min(MAX_CHARACTERS, length - offset);
				ulong workspace = 0;

				for(int j = stride; j != 0; j--) {
					workspace <<= SLIDE;
					workspace |= (ulong) (TokenIndices[base32fixed[(offset + j) - 1]] & MASK);
				}

				buffer.Clear();
				TypeSerializer.Serialize(workspace, buffer);

				int byteLength = Math.Min(bytes.Length - byteOffset, (int) Math.Ceiling(((double) stride * 5) / 8));
				buffer.Slice(0, byteLength).CopyTo(bytes.Span.Slice(byteOffset, byteLength));
				byteOffset += byteLength;
			}

			return bytes;
		}

		public static string Prepare(string value) {
			return value.Trim().ToUpper().Replace("0", "O").Replace("1", "L").Replace("I", "L").Replace("U", "V");
		}
	}

}