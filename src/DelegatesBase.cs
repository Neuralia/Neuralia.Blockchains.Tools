using System;

namespace Neuralia.Blockchains.Tools {
	public class DelegatesBase {

		public delegate void SimpleDelegate();

		public delegate void SimpleExceptionDelegate(Exception ex);

		public delegate void SimpleTypedDelegate<T>(T value);
	}
}