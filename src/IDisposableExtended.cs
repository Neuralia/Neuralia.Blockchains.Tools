using System;

namespace Neuralia.Blockchains.Tools {
	public interface IDisposableExtended : IDisposable {
		bool IsDisposed { get; }
	}
}