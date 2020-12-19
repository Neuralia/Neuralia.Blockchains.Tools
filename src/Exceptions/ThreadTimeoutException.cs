using System;

namespace Neuralia.Blockchains.Tools.Exceptions {
	public class ThreadTimeoutException : Exception {
		public ThreadTimeoutException(string message) : base(message) {
		}
	}
}