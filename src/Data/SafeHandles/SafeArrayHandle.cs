using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Neuralia.Blockchains.Tools.Data {
	[DebuggerDisplay("{HasData?Bytes[Offset].ToString():\"null\"}, {HasData?Bytes[Offset+1].ToString():\"null\"}, {HasData?Bytes[Offset+2].ToString():\"null\"}")]

	public class SafeArrayHandle : SafeHandle<ByteArray,SafeArrayHandle> {
		
		
		public byte this[int i] {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				

				lock(this.locker) {
					return this.Entry[i];
				}
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set {
				

				lock(this.locker) {
					this.Entry[i] = value;
				}
			}
		}
		
		public int Length {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				
				
				return this.Entry?.Length ?? 0;
			}
		}

		public Memory<byte> Memory => this.Entry.Memory;
		public Span<byte> Span => this.Entry.Span;
		public byte[] Bytes => this.Entry.Bytes;
		public int Offset => this.Entry.Offset;
		public bool HasData => this.Entry?.HasData ?? false;
		public bool IsEmpty => this.Entry?.IsEmpty ?? true;
		public bool IsZero => this.Entry?.IsCleared ?? true;
		public bool IsNull => this.Entry == null;
		
		
		public byte[] ToExactByteArray() {
			
			return this.Entry?.ToExactByteArray();
		}

		public byte[] ToExactByteArrayCopy() {
			
			return this.Entry?.ToExactByteArrayCopy();
		}
		
		public static SafeArrayHandle Create(int size) {
			return Create().SetSize(size);
		}
		
		public static implicit operator SafeArrayHandle(byte[] data) {
			return (SafeArrayHandle)Create().SetData(data);
		}

		public static implicit operator SafeArrayHandle(ByteArray data) {
			return (SafeArrayHandle)Create().SetData(data);
		}

		public SafeArrayHandle SetSize(int size) {
			
		
			this.Entry = ByteArray.Create(size);

			return this;
		}

		
	}
}