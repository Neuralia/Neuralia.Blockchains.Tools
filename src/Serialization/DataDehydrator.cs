using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.IO;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Data.Arrays;
using Neuralia.Blockchains.Tools.General;

namespace Neuralia.Blockchains.Tools.Serialization {
	public abstract class DataDehydrator : IDataDehydrator {

		public static readonly List<BitSequence.Entry> entries = new();

		private static readonly int ARRAY_MAX_SIZE = int.MaxValue >> 2; // we reserve two bits as flags.

		/// <summary>
		///     The null flag collecting buffer
		/// </summary>
		private readonly List<bool> booleanFlags = new();

		private readonly SizeSerializationHelper sizeSerializationHelper = new();

		private RecyclableMemoryStream stream;

		protected byte version = 0;

		static DataDehydrator() {

			BitSequence.Entry entry = new();
			entry.name = "version";
			entry.bitSize = 4;
			entry.offset = 0;

			entries.Add(entry);

			entry = new BitSequence.Entry();
			entry.name = "flagsize";
			entry.bitSize = 4;
			entry.offset = 4;

			entries.Add(entry);
		}

		public DataDehydrator() {

			//TODO:  should we learn from ZeroFormatter? https://github.com/neuecc/ZeroFormatter

			this.stream = (RecyclableMemoryStream) MemoryUtils.Instance.recyclableMemoryStreamManager.GetStream();

			this.SetVersion();

			if(this.version == 0) {
				throw new ApplicationException("The version number must be set");
			}
		}

		public long Position => this.stream.Length;

		public int Length => (int) this.stream.Position;

		// get the size the metadata will occupy
		public int MetadataSize {
			get {
				(int _, int _, int metadataSize) = this.BuildMetadataSize();

				return metadataSize;
			}
		}

		public byte Version => this.version;

		public IDataDehydrator Write(IEnumerable<IBinaryDehydratable> collection) {

			IBinaryDehydratable[] binaryDehydratables = collection as IBinaryDehydratable[] ?? collection.ToArray();

			bool any = binaryDehydratables.Any();
			this.Write(any);

			if(any) {
				this.sizeSerializationHelper.Size = binaryDehydratables.Length;
				this.sizeSerializationHelper.Dehydrate(this);

				foreach(IBinaryDehydratable entry in binaryDehydratables) {
					this.Write(entry);
				}
			}

			return this;
		}

		public IDataDehydrator Write(IBinaryDehydratable value) {
			this.Write(value != null);

			if(value != null) {
				this.WriteNoNullable(value);
			}

			return this;
		}

		public IDataDehydrator WriteNoNullable(IBinaryDehydratable value) {
			value.Dehydrate(this);

			return this;
		}

		public IDataDehydrator Write(byte value) {
			this.stream.WriteByte(value);

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(byte? value) {
			this.WriteNull(value.HasValue);

			if(value.HasValue) {
				this.stream.WriteByte(value.Value);
			}

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(short value) {

			Span<byte> buffer = stackalloc byte[sizeof(short)];

			TypeSerializer.Serialize(value, buffer);

			return this.WriteRawArray(buffer);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(short? value) {
			this.WriteNull(value.HasValue);

			if(value.HasValue) {
				this.Write(value.Value);
			}

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(ushort value) {
			Span<byte> buffer = stackalloc byte[sizeof(ushort)];

			TypeSerializer.Serialize(value, buffer);

			return this.WriteRawArray(buffer);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(ushort? value) {
			this.WriteNull(value.HasValue);

			if(value.HasValue) {
				this.Write(value.Value);
			}

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(int value) {
			Span<byte> buffer = stackalloc byte[sizeof(int)];

			TypeSerializer.Serialize(value, buffer);

			return this.WriteRawArray(buffer);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(int? value) {
			this.WriteNull(value.HasValue);

			if(value.HasValue) {
				this.Write(value.Value);
			}

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(uint value) {
			Span<byte> buffer = stackalloc byte[sizeof(uint)];

			TypeSerializer.Serialize(value, buffer);

			return this.WriteRawArray(buffer);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(uint? value) {
			this.WriteNull(value.HasValue);

			if(value.HasValue) {
				this.Write(value.Value);
			}

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(long value) {
			Span<byte> buffer = stackalloc byte[sizeof(long)];

			TypeSerializer.Serialize(value, buffer);

			return this.WriteRawArray(buffer);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(long? value) {
			this.WriteNull(value.HasValue);

			if(value.HasValue) {
				this.Write(value.Value);
			}

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(ulong value) {
			Span<byte> buffer = stackalloc byte[sizeof(ulong)];

			TypeSerializer.Serialize(value, buffer);

			return this.WriteRawArray(buffer);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(ulong? value) {
			this.WriteNull(value.HasValue);

			if(value.HasValue) {
				this.Write(value.Value);
			}

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(float value) {

			Span<byte> buffer = stackalloc byte[sizeof(float)];

			//TODO: should the whole class use marshal?
			TypeSerializer.Serialize(value, buffer);

			this.WriteRawArray(buffer);

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(float? value) {

			this.WriteNull(value.HasValue);

			if(value.HasValue) {
				this.Write(value.Value);
			}

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(double value) {

			Span<byte> buffer = stackalloc byte[sizeof(double)];

			//TODO: should the whole class use marshal?
			TypeSerializer.Serialize(value, buffer);

			this.WriteRawArray(buffer);

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(double? value) {
			this.WriteNull(value.HasValue);

			if(value.HasValue) {
				this.Write(value.Value);
			}

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(bool value) {

			this.booleanFlags.Add(value);

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(bool? value) {
			this.WriteNull(value.HasValue);

			if(value.HasValue) {
				this.Write(value.Value);
			}

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(Guid value) {
			return this.WriteRawArray(value.ToByteArray());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(Guid? value) {
			this.WriteNull(value.HasValue);

			if(value.HasValue) {
				this.Write(value.Value);
			}

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(string value) {

			byte[] bytes = null;

			if(value != null) {
				bytes = Encoding.UTF8.GetBytes(value);
			}

			this.Write(bytes);

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(DateTime value) {
			return this.Write(value.ToBinary());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(DateTime? value) {
			this.WriteNull(value.HasValue);

			if(value.HasValue) {
				this.Write(value.Value);
			}

			return this;
		}

		public IDataDehydrator Write(TimeSpan value) {
			return this.Write(value.Ticks);
		}

		public IDataDehydrator Write(TimeSpan? value) {
			this.WriteNull(value.HasValue);

			if(value.HasValue) {
				this.Write(value.Value);
			}

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator WriteRawArray(SafeArrayHandle array) {

			this.stream.Write(array.Bytes, array.Offset, array.Length);

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator WriteRawArray(ByteArray array) {

			this.stream.Write(array.Bytes, array.Offset, array.Length);

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator WriteRawArray(ReadOnlySequence<byte> sequence) {

			foreach(ReadOnlyMemory<byte> entry in sequence) {

				this.stream.Write(entry.Span);
			}

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator WriteRawArray(byte[] array, int offset, int length) {
			this.stream.Write(array, offset, length);

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator WriteRawArray(byte[] array) {
			this.WriteRawArray(array, 0, array.Length);

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator WriteRawArray(Span<byte> span) {
			this.stream.Write(span);

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator WriteNonNullable(byte[] array, int offset, int length) {
			if(length > ARRAY_MAX_SIZE) {
				throw new ApplicationException($"Blocks can not have an array with a length greater than {ARRAY_MAX_SIZE} bytes");
			}

			this.sizeSerializationHelper.Size = length;
			this.sizeSerializationHelper.Dehydrate(this);

			if(length > 0) {
				this.WriteRawArray(array, offset, length);

			}

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator WriteNonNullable(byte[] array) {

			return this.WriteNonNullable(array, 0, array?.Length??0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(byte[] array) {

			return this.Write(array, 0, array?.Length??0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(byte[] array, int offset, int length) {

			bool isNull = (array == null) || (length == 0);
			this.WriteNull(!isNull);

			if(isNull == false) {

				return this.WriteNonNullable(array, offset, length);
			}

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(in Span<byte> span) {

			if(span.Length > ARRAY_MAX_SIZE) {
				throw new ApplicationException($"Blocks can not have an array with a length greater than {ARRAY_MAX_SIZE} bytes");
			}

			this.sizeSerializationHelper.Size = span.Length;
			this.sizeSerializationHelper.Dehydrate(this);

			if(span.Length > 0) {
				this.WriteRawArray(span);

			}

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator WriteSmallArray(byte[] array, int offset, int length) {
			if(length > byte.MaxValue) {
				throw new ApplicationException($"A small array can not have an array with a length greater than {byte.MaxValue} bytes");
			}

			this.Write((byte) length);

			if(length > 0) {
				this.WriteRawArray(array, offset, length);
			}

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator WriteSmallArray(byte[] array) {

			return this.WriteSmallArray(array, 0, array.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator WriteSmallArray(in Span<byte> span) {

			if(span.Length > byte.MaxValue) {
				throw new ApplicationException($"A small array can not have an array with a length greater than {byte.MaxValue} bytes");
			}

			this.Write((byte) span.Length);

			if(span.Length > 0) {
				this.WriteRawArray(span);
			}

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator WriteNonNullable(SafeArrayHandle array) {

			return this.WriteNonNullable(array.Entry);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator WriteNonNullable(ByteArray array) {

			return this.WriteNonNullable(array?.Bytes, array?.Offset??0, array?.Length??0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(ByteArray array) {
			bool isNull = (array == null) || array.IsEmpty || (array.Length == 0);
			this.WriteNull(!isNull);

			if(isNull == false) {

				return this.WriteNonNullable(array);
			}

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator Write(SafeArrayHandle array) {
			return this.Write(array?.Entry);
		}

		// method to insert inner content into the memory stream and then come back to set the size at the begining
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataDehydrator WriteWrappedContent(Action<IDataDehydrator> action) {

			// backup our good stream
			RecyclableMemoryStream streamBackup = this.stream;

			//create a temporary replacement stream
			this.stream = (RecyclableMemoryStream) MemoryUtils.Instance.recyclableMemoryStreamManager.GetStream();

			int boolCount = this.booleanFlags.Count;

			action(this);

			// how many new flags were inserted during this content
			int newFlags = this.booleanFlags.Count - boolCount;

			//restore the stream
			using RecyclableMemoryStream temporaryStream = this.stream;
			this.stream = streamBackup;

			// write the section size
			this.sizeSerializationHelper.Size = (int) temporaryStream.Length;
			this.sizeSerializationHelper.Dehydrate(this);

			// and the bool flag count
			this.sizeSerializationHelper.Size = newFlags;
			this.sizeSerializationHelper.Dehydrate(this);

			// and write the stream
			temporaryStream.WriteTo(this.stream);

			return this;
		}

		/// <summary>
		///     Inject the contents of a dehydrator as is.
		/// </summary>
		/// <param name="other"></param>
		public void InjectDehydrator(IDataDehydrator other, bool insertRaw = false) {

			if(other.Version != this.version) {
				throw new ApplicationException("Data dehydrators of different version can not be combined.");
			}

			(SafeArrayHandle data, List<bool> booleanFlags) otherComponents = other.ToComponentsArray();

			// inject the data
			if(insertRaw) {
				this.WriteRawArray(otherComponents.data);
			} else {
				this.Write(otherComponents.data);
			}

			//inject the null flags too
			this.booleanFlags.AddRange(otherComponents.booleanFlags);
		}

		/// <summary>
		///     This method will return the content without the metadata. it is intended to be injected in another dehydrator
		/// </summary>
		/// <returns></returns>
		public (SafeArrayHandle data, List<bool> booleanFlags) ToComponentsArray() {

			return (this.ToRawArray(), this.booleanFlags.ToList());
		}

		/// <summary>
		///     Return the raw awway without any metadata details
		/// </summary>
		/// <returns></returns>
		public SafeArrayHandle ToRawArray() {
			return SafeArrayHandle.Create(this.stream);
		}

		/// <summary>
		///     will return null if the stream is empty
		/// </summary>
		/// <returns></returns>
		public SafeArrayHandle ToNullableRawArray() {
			if(this.stream.Length == 0) {
				return null;
			}

			return this.ToRawArray();
		}

		/// <summary>
		///     return the contents and the size of the metadata
		/// </summary>
		/// <returns></returns>
		public (SafeArrayHandle data, int metadataSize) ToArrayAndMetadata() {

			int dataLength = (int) this.stream.Length;

			if((dataLength == 0) && !this.booleanFlags.Any()) {
				// a zero length array returns nothing, so we save space on empty
				return (SafeArrayHandle.Create(), 0);
			}

			using(SafeArrayHandle metadata = this.CreateMetadata()) {

				//TODO: improve this allocation above
				SafeArrayHandle block = SafeArrayHandle.Create(dataLength + metadata.Length);

				byte[] data = this.stream.GetBuffer();

				int metadataLength = metadata.Length;

				block.Entry.CopyFrom(ref data, 0, 0, dataLength);
				block.Entry.CopyFrom(metadata.Entry, 0, dataLength, metadataLength);

				return (block, metadataLength);

			}
		}

		/// <summary>
		///     This method will return the content wrapped with the required metadata.
		/// </summary>
		/// <returns></returns>
		public virtual SafeArrayHandle ToArray() {
			return this.ToArrayAndMetadata().data;
		}

		public virtual ByteArray ToReleasedArray() {
			using SafeArrayHandle data = this.ToArray();

			return data.Release();
		}

		/// <summary>
		///     will return null if the stream is empty
		/// </summary>
		/// <returns></returns>
		public SafeArrayHandle ToNullableArray() {
			if(this.stream.Length == 0) {
				return null;
			}

			return this.ToArray();
		}

		protected abstract void SetVersion();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void WriteNull(bool hasValue) {
			this.Write(hasValue);
		}

		protected (int bitArraySize, int extraSizeByteSize, int metadataSize) BuildMetadataSize(BitSequence sequence = null) {

			if(sequence == null) {
				sequence = new BitSequence(0, entries);
			}

			// the version byte is always present
			int metadataSize = 1;

			// build a bitvector with the nullables

			int bitArraySize = 0;

			// determine how many bytes are needed to store these bits
			if(this.booleanFlags.Count > 0) {
				bitArraySize = BitUtilities.GetBytesRequiredToStoreBits(this.booleanFlags.Count);
			}

			// the version
			sequence.SetEntryValue(0, this.version);

			// our total metadata size with the bit bytes
			metadataSize += bitArraySize;

			int extraSizeByteSize = 0;

			// if we are higher then 14, we use 15 as a special marker, to look into another byte for the size
			if(bitArraySize > (0xF - 1)) {
				// we need extra bytes
				if(bitArraySize > 0x3FFFFFFF) {
					throw new Exception("We reched the maximum size for this metadata buffer");
				}

				if(bitArraySize <= 0x3F) {
					//smaller than 128 bytes, we can store it in one byte
					extraSizeByteSize = 1;
				} else if(bitArraySize <= 0x3FFF) {
					//smaller than 128 bytes, we can store it in one byte
					extraSizeByteSize = 2;
				} else if(bitArraySize <= 0x3FFFFF) {
					//smaller than 128 bytes, we can store it in one byte
					extraSizeByteSize = 3;
				} else {
					//smaller than 128 bytes, we can store it in one byte
					extraSizeByteSize = 4;
				}
			}

			metadataSize += extraSizeByteSize;

			return (bitArraySize, extraSizeByteSize, metadataSize);
		}

		/// <summary>
		///     ok, the trailing metadata is created here
		/// </summary>
		/// <returns></returns>
		protected SafeArrayHandle CreateMetadata() {

			// first the version info
			BitSequence sequence = new(0, entries);

			(int bitArraySize, int extraSizeByteSize, int metadataSize) = this.BuildMetadataSize(sequence);

			// the array
			SafeArrayHandle metadata = SafeArrayHandle.Create(metadataSize);

			// now we copy the bits. to avoid an allocation to use copyTo, we simply do it ourselves.
			int index = 0;

			foreach(bool bit in this.booleanFlags) {

				BitUtilities.SetBit(metadata, index, bit);
				index++;
			}

			int entrySize = bitArraySize;

			// again, if the byte size is bigger than 14, we use an extra byte to store it
			if(bitArraySize >= 0xF) {
				// we need an extra byte
				entrySize = 0xF; // special flag

				//in the first 4 bits, we write th byte size of the size information
				metadata[metadata.Length - 2] = (byte) ((extraSizeByteSize - 1) & 0x3);

				//now the size info in the remaining bits
				int size = bitArraySize;

				for(int i = 0; i < extraSizeByteSize; i++) {

					if(i == 0) {
						// the first one, we must skip the first two bits to store up to 64 
						metadata[metadata.Length - 2] |= (byte) ((size << 2) & 0xFC);
						size >>= 6;
					} else {
						metadata[metadata.Length - 2 - i] = (byte) (size & 0xFF);
						size >>= 8;
					}
				}
			}

			// set the size indicator
			sequence.SetEntryValue(1, (ulong) entrySize);

			// set the version and size info
			metadata[metadata.Length - 1] = (byte) sequence.GetBuffer().buffer;

			return metadata;
		}

	#region Disposable

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if(disposing && !this.IsDisposed) {
				this.stream.Dispose();
			}

			this.IsDisposed = true;
		}

		~DataDehydrator() {
			this.Dispose(false);
		}

		public bool IsDisposed { get; private set; }

	#endregion

	}
}