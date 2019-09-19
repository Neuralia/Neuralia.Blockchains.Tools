namespace Neuralia.Blockchains.Tools.Data {
	public static class DoubleArrayHelper {
		public static void Return(ByteArray[] array) {
			if(array == null) {
				return;
			}
			
			// empty our contents
			for(int i = 0; i < array.Length; i++) {
				array[i]?.Return();
			}
		}
		
		public static void Dispose(ByteArray[] array) {
			// empty our contents
			if(array == null) {
				return;
			}
			for(int i = 0; i < array.Length; i++) {
				array[i]?.Dispose();
			}
		}

		public static ByteArray[] Clone(ByteArray[] array) {
			var clone = new ByteArray[array.Length];
			
			for(int i = 0; i < clone.Length; i++) {
				clone[i] = array[i].Clone();
			}
			
			return clone;
		}
		
		public static ByteArray[] CloneShallow(ByteArray[] array) {

			var clone = new ByteArray[array.Length];
			
			for(int i = 0; i < clone.Length; i++) {
				clone[i] = array[i];
			}
			
			return clone;
		}
	}
}