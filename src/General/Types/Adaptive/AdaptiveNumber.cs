using System;
using Neuralia.Blockchains.Tools.Serialization;

namespace Neuralia.Blockchains.Tools.General.Types.Dynamic {
	public abstract class SerializationAdaptiveNumber<T> : IBinarySerializable, IEquatable<SerializationAdaptiveNumber<T>>, IComparable<T>, IComparable<SerializationAdaptiveNumber<T>>
		where T : struct, IComparable, IConvertible, IFormattable, IComparable<T>, IEquatable<T> {

		private T size;

		public SerializationAdaptiveNumber() {

		}

		public SerializationAdaptiveNumber(T size) {

			this.Size = size;
		}

		public SerializationAdaptiveNumber(SerializationAdaptiveNumber<T> other) {
			this.Size = other.Size;
		}

		public abstract T MaxValue { get; }

		protected abstract int Offset { get; }

		protected abstract int MinimumByteCount { get; }

		protected abstract int MaximumByteCount { get; }

		protected int LowerMask => byte.MaxValue >> (8 - this.Offset);

		protected int HigherMask => byte.MaxValue >> this.Offset;

		/// <summary>
		///     Number of seconds since chain inception
		/// </summary>
		public T Size {
			get => this.size;
			set {
				this.TestMaxSize(value);
				this.size = value;
			}
		}

		public virtual void Dehydrate(IDataDehydrator dehydrator) {

			byte[] data = this.GetShrunkBytes();
			dehydrator.WriteRawArray(data);
		}

		public virtual void Rehydrate(IDataRehydrator rehydrator) {

			this.ReadData(rehydrator.ReadByte, (in Span<byte> longbytes, int start, int length) => rehydrator.ReadBytes(longbytes, start, length));
		}

		public int CompareTo(SerializationAdaptiveNumber<T> other) {
			if(ReferenceEquals(this, other)) {
				return 0;
			}

			if(ReferenceEquals(null, other)) {
				return 1;
			}

			return this.size.CompareTo(other.size);
		}

		public int CompareTo(T other) {
			return this.size.CompareTo(other);
		}

		public bool Equals(SerializationAdaptiveNumber<T> other) {
			if(ReferenceEquals(null, other)) {
				return false;
			}

			if(ReferenceEquals(this, other)) {
				return true;
			}

			return this.size.Equals(other.size);
		}

		protected void TestMaxSize(T value) {
			if(value.CompareTo(this.MaxValue) > 0) {
				throw new ApplicationException("Invalid value value. bit size is too big!");
			}
		}

		protected byte[] BuildShrunkBytes(T value) {
			ulong convertedValue = this.ConvertTypeFrom(value);

			int bitSize = BitUtilities.GetValueBitSize(convertedValue) + this.Offset;
			int serializationByteSize = 0;

			for(int i = this.MinimumByteCount; i <= this.MaximumByteCount; i++) {
				if(bitSize <= (8 * i)) {
					serializationByteSize = i;

					break;
				}
			}

			(int serializationByteSize, int adjustedSerializationByteExtraSize, int bitValues) adjusted = this.AdjustSerializationByteSize(serializationByteSize);

			// ensure the important type bits are set too

			byte[] shrunkBytes = new byte[adjusted.serializationByteSize];

			// serialize the first byte, combination of 4 bits for the serialization type, and the firs 4 bits of our value
			shrunkBytes[0] = (byte) ((byte) adjusted.bitValues & this.LowerMask);
			byte tempId = (byte) ((byte) convertedValue & this.HigherMask);
			shrunkBytes[0] |= (byte) (tempId << this.Offset);

			// now offset the rest of the ulong value
			ulong temp = convertedValue >> (8 - this.Offset);

			TypeSerializer.SerializeBytes(((Span<byte>) shrunkBytes).Slice(1, shrunkBytes.Length - 1), temp);

			return shrunkBytes;
		}

		public virtual byte[] GetShrunkBytes() {
			// determine the size it will take when serialized
			return this.BuildShrunkBytes(this.Size);

		}

		private int ReadData(Func<byte> readFirstByte, CopyDataDelegate copyBytes) {
			byte firstByte = readFirstByte();

			(int serializationByteSize, int adjustedSerializationByteExtraSize, int bitValues) specs = this.ReadByteSpecs(firstByte);

			Span<byte> longbytes = stackalloc byte[8];

			int readLength = specs.serializationByteSize - 1;

			copyBytes(longbytes, 0, readLength);

			TypeSerializer.DeserializeBytes(longbytes, out ulong buffer);

			buffer <<= 8 - this.Offset;
			buffer |= (byte) (firstByte >> this.Offset);

			buffer = this.prepareBuffer(buffer, firstByte);

			this.Size = this.ConvertTypeTo(buffer);

			return readLength + 1;
		}

		protected abstract T ConvertTypeTo(ulong buffer);
		protected abstract ulong ConvertTypeFrom(T value);

		protected virtual ulong prepareBuffer(ulong buffer, byte firstByte) {
			return buffer;
		}

		public virtual (int serializationByteSize, int adjustedSerializationByteExtraSize, int bitValues) ReadByteSpecs(byte firstByte) {
			// set the buffer, so we can read the serialization type
			return this.AdjustSerializationByteSize((firstByte & this.LowerMask) + this.MinimumByteCount);
		}

		public int ReadByteSize(byte firstByte) {
			// set the buffer, so we can read the serialization type
			return this.ReadByteSpecs(firstByte).serializationByteSize;
		}

		protected virtual (int serializationByteSize, int adjustedSerializationByteExtraSize, int bitValues) AdjustSerializationByteSize(int value) {
			return (value, value - this.MinimumByteCount, value - this.MinimumByteCount);
		}

		public override bool Equals(object obj) {
			if(obj is SerializationAdaptiveNumber<T> adaptive) {
				return this.Equals(adaptive);
			}

			return base.Equals(obj);
		}

		public static bool operator ==(SerializationAdaptiveNumber<T> left, SerializationAdaptiveNumber<T> right) {
			if(ReferenceEquals(null, left)) {
				return ReferenceEquals(null, right);
			}

			return left.Equals(right);
		}

		public static bool operator !=(SerializationAdaptiveNumber<T> left, SerializationAdaptiveNumber<T> right) {
			return !Equals(left, right);
		}

		public override int GetHashCode() {
			return this.size.GetHashCode();
		}

		public override string ToString() {
			return this.Size.ToString();
		}

		private delegate void CopyDataDelegate(in Span<byte> longbytes, int start, int length);
	}
}