using System;
using Neuralia.Blockchains.Core.Cryptography.xxHash;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Data.Arrays;
using Neuralia.Blockchains.Tools.Serialization;

namespace Neuralia.Blockchains.Tools.Cryptography.Hash {
	public abstract class xxHasher<T, H> : IHasher<T> 
	where H : class, IxxHash, new(){
		
		private readonly H hasher;

		public xxHasher() {

			this.hasher = new H();
		}

		public abstract T Hash(SafeArrayHandle wrapper);
		public abstract T Hash(byte[] message);

		public T HashTwo(SafeArrayHandle message1, SafeArrayHandle message2) {
			
			return this.HashTwo(message1.Span, message2.Span);
		}
		
		public T HashTwo(SafeArrayHandle message1, in Span<byte> message2) {
			
			return this.HashTwo(message1.Span, message2);
		}
		
		/// <summary>
		/// A version that does not use stack alloc, usually for larger arrays
		/// </summary>
		/// <param name="message1"></param>
		/// <param name="message2"></param>
		/// <returns></returns>
		public T HashTwo(in Span<byte> message1, in Span<byte> message2) {
			int len1 = 0;

			if(message1 != null) {
				len1 = message1.Length;
			}

			int len2 = 0;

			if(message2 != null) {
				len2 = message2.Length;
			}

			SafeArrayHandle buffer = ByteArray.Create(len1 + len2);

			if(message1 != null) {
				message1.CopyTo(buffer.Span.Slice(len1));
			}

			if(message2 != null) {
				message2.CopyTo(buffer.Span.Slice(len1, len2));
			}

			// do the hash
			return this.Hash(buffer);
		}
		
		/// <summary>
		/// a faster version that uses stack allock. reserved for smaller arrays
		/// </summary>
		/// <param name="message1"></param>
		/// <param name="message2"></param>
		/// <returns></returns>
		public T HashTwoFast(in Span<byte> message1, in Span<byte> message2) {
			int len1 = 0;

			if(message1 != null) {
				len1 = message1.Length;
			}

			int len2 = 0;

			if(message2 != null) {
				len2 = message2.Length;
			}

			Span<byte> buffer = stackalloc byte[len1 + len2];

			if(message1 != null) {
				message1.CopyTo(buffer.Slice(0,len1));
			}

			if(message2 != null) {
				message2.CopyTo(buffer.Slice(len1, len2));
			}

			// do the hash
			return this.Hash(buffer);
		}
		
		public T HashTwoFast(SafeArrayHandle message1, SafeArrayHandle message2) {
			return this.HashTwoFast(message1.Span, message2.Span);
		}

		public T HashTwo(in Span<byte> message1, short message2) {
			
			Span<byte> buffer = stackalloc byte[sizeof(short)];
			TypeSerializer.Serialize(message2, buffer);
			
			return this.HashTwo(message1, buffer);
		}
		
		public T HashTwo(SafeArrayHandle message1, short message2) {
			return this.HashTwo(message1, TypeSerializer.Serialize(message2));
		}
		
		public T HashTwo(in Span<byte> message1, int message2) {
			
			Span<byte> buffer = stackalloc byte[sizeof(int)];
			TypeSerializer.Serialize(message2, buffer);
			
			return this.HashTwo(message1, buffer);
		}

		public T HashTwo(SafeArrayHandle message1, int message2) {
			return this.HashTwo(message1, TypeSerializer.Serialize(message2));
		}
		
		public T HashTwo(in Span<byte> message1, long message2) {
			
			Span<byte> buffer = stackalloc byte[sizeof(long)];
			TypeSerializer.Serialize(message2, buffer);
			
			return this.HashTwo(message1, buffer);
		}

		public T HashTwo(SafeArrayHandle message1, long message2) {
			return this.HashTwo(message1, TypeSerializer.Serialize(message2));
		}

		public T HashTwo(short message1, short message2) {
			
			Span<byte> part1 = stackalloc byte[sizeof(short)];
			TypeSerializer.Serialize(message2, part1);
			
			Span<byte> part2 = stackalloc byte[sizeof(short)];
			TypeSerializer.Serialize(message2, part2);

			return this.HashTwoFast(part1, part2);
		}

		public T HashTwo(ushort message1, ushort message2) {
			Span<byte> part1 = stackalloc byte[sizeof(ushort)];
			TypeSerializer.Serialize(message2, part1);
			
			Span<byte> part2 = stackalloc byte[sizeof(ushort)];
			TypeSerializer.Serialize(message2, part2);

			return this.HashTwoFast(part1, part2);
		}

		public T HashTwo(ushort message1, long message2) {
			Span<byte> part1 = stackalloc byte[sizeof(ushort)];
			TypeSerializer.Serialize(message2, part1);
			
			Span<byte> part2 = stackalloc byte[sizeof(long)];
			TypeSerializer.Serialize(message2, part2);

			return this.HashTwoFast(part1, part2);
		}

		public T HashTwo(int message1, int message2) {
			Span<byte> part1 = stackalloc byte[sizeof(int)];
			TypeSerializer.Serialize(message2, part1);
			
			Span<byte> part2 = stackalloc byte[sizeof(int)];
			TypeSerializer.Serialize(message2, part2);

			return this.HashTwoFast(part1, part2);
		}

		public T HashTwo(uint message1, uint message2) {
			Span<byte> part1 = stackalloc byte[sizeof(uint)];
			TypeSerializer.Serialize(message2, part1);
			
			Span<byte> part2 = stackalloc byte[sizeof(uint)];
			TypeSerializer.Serialize(message2, part2);

			return this.HashTwoFast(part1, part2);
		}

		public T HashTwo(long message1, long message2) {
			Span<byte> part1 = stackalloc byte[sizeof(long)];
			TypeSerializer.Serialize(message2, part1);
			
			Span<byte> part2 = stackalloc byte[sizeof(long)];
			TypeSerializer.Serialize(message2, part2);

			return this.HashTwoFast(part1, part2);
		}

		public T HashTwo(ulong message1, ulong message2) {
			Span<byte> part1 = stackalloc byte[sizeof(ulong)];
			TypeSerializer.Serialize(message2, part1);
			
			Span<byte> part2 = stackalloc byte[sizeof(ulong)];
			TypeSerializer.Serialize(message2, part2);

			return this.HashTwoFast(part1, part2);
		}
		
		public T HashTwo(SafeArrayHandle message1, ulong message2) {
			
			return this.HashTwo(message1.Span, message2);
		}

		public T HashTwo(in Span<byte> message1, ulong message2) {
			
			Span<byte> part1 = stackalloc byte[sizeof(ulong)];
			TypeSerializer.Serialize(message2, part1);
			
			return this.HashTwo(message1, part1);
		}

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public bool IsDisposed { get; private set; }
		
		public abstract T Hash(in Span<byte> message);
		
		protected SafeArrayHandle HashToBytes(SafeArrayHandle buffer) {

			return this.hasher.Hash(buffer.Span);
		}

		protected SafeArrayHandle HashToBytes(in Span<byte> message) {

			SafeArrayHandle hash = ByteArray.Create(this.hasher.HashSize);
			this.HashToBytes(message, hash.Span);
			return hash;
		}
		
		protected void HashToBytes(in Span<byte> message, in Span<byte> hash) {

			this.hasher.Hash(message, hash);
		}

		protected SafeArrayHandle HashToBytes(byte[] message) {
			return this.hasher.Hash(message);
		}
		
		protected void HashToBytes(byte[] message, in Span<byte> hash) {
			this.hasher.Hash(message, hash);
		}

		protected SafeArrayHandle HashToBytes(byte[] message, int offset, int length) {

			SafeArrayHandle hash = ByteArray.Create(this.hasher.HashSize);
			this.HashToBytes(message.AsSpan(offset, length), hash.Span);
			return hash;
		}
		
		protected void HashToBytes(byte[] message, int offset, int length, in Span<byte> hash) {

			this.hasher.Hash(message.AsSpan(offset, length), hash);
		}

		protected virtual void Dispose(bool disposing) {
			if(disposing && !this.IsDisposed) {

			}

			this.IsDisposed = true;
		}
	}
}