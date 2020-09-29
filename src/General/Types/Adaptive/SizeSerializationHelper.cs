using System;
using System.Collections.Generic;
using Neuralia.Blockchains.Tools.General;
using Neuralia.Blockchains.Tools.General.Types.Dynamic;

namespace Neuralia.Blockchains.Tools.Serialization {
	/// <summary>
	///     A special class to help us in optimizing the serialization of array sizes. stores a 30 bit unsigned int on the
	///     minimum amount of byes posisble.
	/// </summary>
	public class SizeSerializationHelper : SerializationAdaptiveNumber<long> {

		private const byte SPECIAL_FLAG = 0x8;
		private const byte MAXIMUM_SINGLE_BYTE_VALUE = 0x80;

		private const byte LOWER_BYTES = 0x7;
		private const byte HIGHER_BYTES = 0x78;

		private const byte REBUILD_HIGHER_BYTES = 0xF0;

		public static readonly int OFFSET = 4;
		public static readonly long MAX_VALUE = long.MaxValue;

		public SizeSerializationHelper() {

		}

		public SizeSerializationHelper(long size) : base(size) {

		}

		public SizeSerializationHelper(SerializationAdaptiveNumber<long> other) : base(other) {

		}

		public override long MaxValue => MAX_VALUE;
		protected override int Offset => OFFSET;
		protected override int MinimumByteCount => 2;
		protected override int MaximumByteCount => 9;

		public static implicit operator long?(SizeSerializationHelper value) {
			if(value == null) {
				return null;
			}

			return value.Size;
		}

		public static implicit operator long(SizeSerializationHelper value) {

			if(value == null) {
				return 0;
			}

			return value.Size;
		}

		public static implicit operator SizeSerializationHelper(byte value) {
			return new SizeSerializationHelper(value);
		}

		public static implicit operator SizeSerializationHelper(short value) {
			return new SizeSerializationHelper(value);
		}

		public static implicit operator SizeSerializationHelper(ushort value) {
			return new SizeSerializationHelper(value);
		}

		public static implicit operator SizeSerializationHelper(int value) {
			return new SizeSerializationHelper(value);
		}

		public static implicit operator SizeSerializationHelper(uint value) {
			return new SizeSerializationHelper(value);
		}

		public static implicit operator SizeSerializationHelper(long value) {
			return new SizeSerializationHelper(value);
		}

		private bool HasSpecialFlag(byte entry) {
			return (entry & SPECIAL_FLAG) != 0;
		}

		private bool HasSpecialFlag(long entry) {
			return this.HasSpecialFlag(this.ToByte(entry));
		}

		private byte ToByte(long entry) {
			return (byte) (entry & 0xFF);
		}

		public override byte[] GetShrunkBytes() {
			long workingId = this.Size;

			byte[] shrunkBytes = null;

			if(workingId < MAXIMUM_SINGLE_BYTE_VALUE) {
				// it will fit on a single byte, lets perform the swaps
				shrunkBytes = new byte[1];

				// first set the magic flag
				shrunkBytes[0] |= SPECIAL_FLAG;

				// now fit the data around it. first the lower bytes
				byte lowers = (byte) (workingId & LOWER_BYTES);
				byte highers = (byte) (workingId & HIGHER_BYTES);

				shrunkBytes[0] |= lowers;
				shrunkBytes[0] |= (byte) (highers << 1);

			} else {
				// ok, it will take more bytes than one. lets perform it. first, we remove the maximum from the lot

				workingId -= MAXIMUM_SINGLE_BYTE_VALUE;
				shrunkBytes = this.BuildShrunkBytes(workingId);
			}

			return shrunkBytes;
		}

		public override (int serializationByteSize, int adjustedSerializationByteExtraSize, int bitValues) ReadByteSpecs(byte firstByte) {
			// set the buffer, so we can read the serialization 
			if(this.HasSpecialFlag(firstByte)) {
				// its a single byte :)
				return this.AdjustSerializationByteSize(1);
			}

			return base.ReadByteSpecs(firstByte);
		}

		protected override ulong prepareBuffer(ulong buffer, byte firstByte) {

			if(this.HasSpecialFlag(firstByte)) {
				// its a single byte, lets rebuild the number
				byte lowers = (byte) (firstByte & LOWER_BYTES);
				byte highers = (byte) (firstByte & REBUILD_HIGHER_BYTES);

				byte value = lowers;
				value |= (byte) (highers >> 1);

				return value;
			}

			buffer += MAXIMUM_SINGLE_BYTE_VALUE;

			return buffer;
		}

		protected override long ConvertTypeTo(ulong buffer) {
			return (long) buffer;
		}

		protected override ulong ConvertTypeFrom(long value) {
			return (ulong) value;
		}
	}
}