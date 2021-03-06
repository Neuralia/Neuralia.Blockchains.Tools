using System;
using Neuralia.Blockchains.Tools.Data.Arrays;

namespace Neuralia.Blockchains.Tools.Data {
	public static class DoubleArrayHelper {
		public static void Return(ByteArray[] array) {
			if(array == null) {
				return;
			}

			// empty our contents
			for(int i = 0; i < array.Length; i++) {
				array[i]?.Return();
				array[i] = null;
			}
		}

		public static void Dispose(ByteArray[] array) {
			// empty our contents
			Return(array);
		}

		public static ByteArray[] Clone(ByteArray[] array) {
			ByteArray[] clone = new ByteArray[array.Length];

			for(int i = 0; i < clone.Length; i++) {
				clone[i] = array[i].Clone();
			}

			return clone;
		}

		public static ByteArray[] CloneShallow(ByteArray[] array) {

			ByteArray[] clone = new ByteArray[array.Length];

			Array.Copy(array, clone, clone.Length);

			return clone;
		}
	}
}