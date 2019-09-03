using System;

namespace Neuralia.Blockchains.Tools.Serialization {
	public class InsufficientDataException : Exception {
		public InsufficientDataException() {

		}

		public InsufficientDataException(string message) : base(message) {

		}
	}
}