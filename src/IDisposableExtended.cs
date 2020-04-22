using System;

namespace Neuralia.Blockchains.Tools {
	public interface IDisposableExtended : IDisposable {
		public bool IsDisposed { get; }
	}
}