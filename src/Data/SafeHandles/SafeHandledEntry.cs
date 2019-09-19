using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Neuralia.Blockchains.Tools.Data {
	
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>Since this object is par of recycled objects on the finalizer, it will be finalized on their finalizers if it ever implements IDisposable. be careful!</remarks>
	public class SafeHandledEntry {
		private int referenceCounter = 0;
		private readonly object disposeLocker;
		private readonly Action<bool> disposeCallback;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposeCallback"></param>
		/// <param name="disposeLocker">share the same locker reference as the parent</param>
		public SafeHandledEntry(Action<bool> disposeCallback, object disposeLocker) {
			this.disposeCallback = disposeCallback;
			this.disposeLocker = disposeLocker;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Increment() {
			lock(this.disposeLocker) {
				++this.referenceCounter;
			}
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Decrement() {
			lock(this.disposeLocker) {
				this.DecrementNoClear();
				
				this.Dispose(true);
			}
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DecrementNoClear() {
			lock(this.disposeLocker) {
				if(this.referenceCounter != 0) {
					--this.referenceCounter;
				}
			}
		}


		public void Dispose(bool disposing) {
			
			lock(this.disposeLocker) {
				if(disposing && this.referenceCounter != 0) {
					return;
				}
				
				this.disposeCallback(disposing);
			}
		}

		public void Reset() {
			lock(this.disposeLocker) {
				this.referenceCounter = 0;
			}
		}

		public bool Singular => this.referenceCounter == 1;
	}
}