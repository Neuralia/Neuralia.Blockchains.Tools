using Microsoft.IO;

namespace Neuralia.Blockchains.Tools.Data {
	public class MemoryUtils {

		public readonly RecyclableMemoryStreamManager recyclableMemoryStreamManager;

		static MemoryUtils() {

		}

		private MemoryUtils() {

			this.recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
			this.recyclableMemoryStreamManager.AggressiveBufferReturn = true;

		}

		public static MemoryUtils Instance { get; } = new MemoryUtils();
	}
}