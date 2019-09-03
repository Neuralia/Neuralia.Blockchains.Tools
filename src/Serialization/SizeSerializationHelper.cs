using System;
using System.Collections.Generic;
using Neuralia.Blockchains.Tools.General;

namespace Neuralia.Blockchains.Tools.Serialization {
	/// <summary>
	///     A special class to help us in optimizing the serialization of array sizes. stores a 30 bit unsigned int on the
	///     minimum amount of byes posisble.
	/// </summary>
	public class SizeSerializationHelper {

		public enum SerializationSizeType : byte {
			Bytes1 = 0,
			Bytes2 = 1,
			Bytes3 = 2,
			Bytes4 = 3
		}

		private static readonly List<BitSequence.Entry> entries = new List<BitSequence.Entry>();

		private readonly BitSequence sequence = new BitSequence(0L, entries);

		private int cachedSize;

		static SizeSerializationHelper() {

			entries.Add(new BitSequence.Entry {name = "identifier", bitSize = 2, offset = 0});
			entries.Add(new BitSequence.Entry {name = "size", bitSize = 30, offset = 2});
		}

		public int Size {
			get => this.cachedSize;
			set {
				this.cachedSize = (int) this.sequence.MaskEntryValue(1, (ulong) value);

				// determine the size it will take when serialized
				int bitSize = BitUtilities.GetValueBitSize((ulong) this.cachedSize);

				if(bitSize <= ((8 * 1) - 2)) {
					this.sequence.SetEntryValue(0, (byte) SerializationSizeType.Bytes1);
				} else if(bitSize <= ((8 * 2) - 2)) {
					this.sequence.SetEntryValue(0, (byte) SerializationSizeType.Bytes2);
				} else if(bitSize <= ((8 * 3) - 2)) {
					this.sequence.SetEntryValue(0, (byte) SerializationSizeType.Bytes3);
				} else if(bitSize <= ((8 * 4) - 2)) {
					this.sequence.SetEntryValue(0, (byte) SerializationSizeType.Bytes4);
				} else {
					throw new ApplicationException("Invalid array size value. bit size is too big!");
				}

				this.sequence.SetEntryValue(1, (ulong) this.cachedSize);
			}
		}

		public void Dehydrate(IDataDehydrator dehydrator) {

			SerializationSizeType serializationType = (SerializationSizeType) this.sequence.GetEntryValue(0);

			int id = (int) this.sequence.GetBuffer().buffer;

			// ensure the important type bits are set too

			if(serializationType == SerializationSizeType.Bytes1) {
				Span<byte> data = stackalloc byte[1];
				TypeSerializer.SerializeBytes(data, id);
				dehydrator.WriteRawArray(data);
			} else if(serializationType == SerializationSizeType.Bytes2) {
				Span<byte> data = stackalloc byte[2];
				TypeSerializer.SerializeBytes(data, id);
				dehydrator.WriteRawArray(data);
			} else if(serializationType == SerializationSizeType.Bytes3) {
				Span<byte> data = stackalloc byte[3];
				TypeSerializer.SerializeBytes(data, id);
				dehydrator.WriteRawArray(data);
			} else if(serializationType == SerializationSizeType.Bytes4) {
				Span<byte> data = stackalloc byte[4];
				TypeSerializer.SerializeBytes(data, id);
				dehydrator.WriteRawArray(data);
			} else {
				throw new ApplicationException("Invalid options");
			}
		}

		public int Rehydrate(IDataRehydrator rehydrator) {

			this.cachedSize = 0;
			byte firstByte = rehydrator.ReadByte();

			// set the buffer, so we can read the serialization type
			this.sequence.SetBuffer(firstByte);

			SerializationSizeType serializationType = (SerializationSizeType) this.sequence.GetEntryValue(0);

			Span<byte> longbytes = stackalloc byte[4];
			longbytes[0] = firstByte;
			int readLength = 0;

			switch(serializationType) {
				case SerializationSizeType.Bytes1:

					break;

				case SerializationSizeType.Bytes2:
					readLength = 1;

					break;

				case SerializationSizeType.Bytes3:
					readLength = 2;

					break;

				case SerializationSizeType.Bytes4:
					readLength = 3;

					break;

				default:

					throw new ApplicationException("Invalid options");
			}

			if(readLength != 0) {
				rehydrator.ReadBytes(longbytes, 1, readLength);
			}

			TypeSerializer.DeserializeBytes(longbytes, out uint buffer);

			this.sequence.SetBuffer(buffer);

			this.cachedSize = (int) this.sequence.GetEntryValue(1);

			return readLength + 1;
		}
	}
}