using System;

namespace Neuralia.Blockchains.Tools.Locking {
	public abstract class RecursiveAsyncLockBase {

		public Guid Uuid { get; } = Guid.NewGuid();
	}
}