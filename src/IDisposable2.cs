using System;

namespace Neuralia.Blockchains.Tools {
	public interface IDisposable2 : IDisposable {
		bool IsDisposed { get; }
	}
}