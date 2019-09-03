using System.Collections;
using System.Collections.Generic;

namespace Neuralia.Blockchains.Tools.Data {
	public class ByteArrayEnumerator<T, U> : IEnumerator<T>
		where U : IByteArray<T, U>{

		private readonly U buffer;
		private int index = -1;

		public ByteArrayEnumerator(U buffer) {
			this.buffer = buffer;
			this.Reset();
		}

		public bool MoveNext() {
			if(++this.index >= this.buffer.Length) {
				return false;
			}

			this.Current = this.buffer[this.index];

			return true;
		}

		public void Reset() {
			this.index = -1;
		}

		public T Current { get; private set; }

		object IEnumerator.Current => this.Current;

		public void Dispose() {

		}
	}
}