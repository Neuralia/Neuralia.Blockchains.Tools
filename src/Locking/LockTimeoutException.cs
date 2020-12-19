using System;

namespace Neuralia.Blockchains.Tools.Locking {
	public class LockTimeoutException : ApplicationException {

		public LockTimeoutException() {
		}

		public LockTimeoutException(string message) : base(message) {
		}

		public LockTimeoutException(string message, Exception innerException) : base(message, innerException) {
		}
	}
}