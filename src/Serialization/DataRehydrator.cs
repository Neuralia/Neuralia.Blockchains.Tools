using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Data.Arrays;
using Neuralia.Blockchains.Tools.General;

namespace Neuralia.Blockchains.Tools.Serialization {

	/// <summary>
	///     A data dehydrator
	/// </summary>
	/// <remarks>we store the version at the of byte, as well as null flags</remarks>
	public abstract class DataRehydrator : IDataRehydrator {

		public delegate void RehydrationDelegate<T>(IDataRehydrator rehydrator, ref T entry)
			where T : IBinaryRehydratable;

		// the length of our data
		protected readonly int length;

		// offset in the array. usually 0, unless we are in a slice
		protected readonly int offset;

		private readonly SizeSerializationHelper sizeSerializationHelper = new SizeSerializationHelper();

		protected int booleanFlagOffset;

		protected int booleanFlagOffsetSnapshot;

		protected BitArray booleanFlags;

		/// <summary>
		///     the maximum amount of memory we expect to read. Anything above this will cause an exception
		/// </summary>
		protected int maximumReadSize;

		// the position, always relative to the offset
		protected int position;

		protected int positionSnapshot;

		protected byte version = 0;

		public DataRehydrator(byte[] data, bool metadata = true) : this(ByteArray.Wrap(data), data.Length, metadata) {

		}

		public DataRehydrator(SafeArrayHandle data, bool metadata = true) : this(data, data.Length, metadata) {

		}

		public DataRehydrator(SafeArrayHandle data, int length, bool metadata = true) : this(data, length, length, metadata) {
		}

		public DataRehydrator(SafeArrayHandle data, int length, int maximumReadSize, bool metadata = true) : this(data, 0, length, maximumReadSize, metadata) {

		}

		/// <summary>
		///     create a sub array, or a slice inside another array
		/// </summary>
		/// <param name="data"></param>
		/// <param name="position"></param>
		/// <param name="length"></param>
		public DataRehydrator(SafeArrayHandle data, int offset, int length, int maximumReadSize, bool metadata = true) {
			// we dont use MemoryStream here because its more efficient to read directly from the source array
			this.offset = offset;

			this.SetVersion();

			if(this.version == 0) {
				throw new ApplicationException("The version number must be set");
			}

			int metadataSize = 0;

			if(metadata) {
				metadataSize = this.ExtractMetadata(data, this.offset, length);
			}

			this.Data = data;

			// make sure we adjust to remove the metadata
			this.length = length - metadataSize;
			this.maximumReadSize = maximumReadSize - metadataSize;
		}

		protected int ActualPosition => this.Offset;

		public bool IsEnd => this.position == this.length;

		public int Offset => this.offset + this.position;

		public int RemainingLength => this.length - this.ActualPosition;

		public SafeArrayHandle Data { get; } = SafeArrayHandle.Create();

		/// <summary>
		///     move forward by x amount
		/// </summary>
		/// <param name="forwardPosition"></param>
		/// <remarks>Be CAREFUL!!  this version does not change the boolean flags. make sure it is used for simple forwards only</remarks>
		public void Forward(int forwardPosition) {
			this.position += forwardPosition;
		}

		/// <summary>
		///     read section size and move forward by X
		/// </summary>
		public void SkipSection() {
			(int size, int booleanFlagCount, int _) = this.ReadSectionSize();

			this.Forward(size, booleanFlagCount);
		}

		public (int size, int booleanFlagCount, int sizesByteSize) ReadSectionSize() {
			(int value, int sizeByteSize) size = this.ReadSize();
			(int value, int sizeByteSize) booleanFlagCount = this.ReadSize();

			// return the size of the section, the amounts of flags inside and finally, the size of the size bytes themselves
			return (size.value, booleanFlagCount.value, size.sizeByteSize + booleanFlagCount.sizeByteSize);
		}

		public int ReadArraySize() {
			return this.ReadSize().value;
		}

		public void Skip(int length) {
			this.Forward(length);
		}

		public void SkipByte() {
			this.Forward(sizeof(byte));
		}

		public void SkipShort() {
			this.Forward(sizeof(short));
		}

		public void SkipUShort() {
			this.Forward(sizeof(ushort));
		}

		public void SkipInt() {
			this.Forward(sizeof(int));
		}

		public void SkipUInt() {
			this.Forward(sizeof(uint));
		}

		public void SkipLong() {
			this.Forward(sizeof(long));
		}

		public void SkipULong() {
			this.Forward(sizeof(ulong));
		}

		/// <summary>
		///     Here we simply skip a section size, since we may not need it
		/// </summary>
		public void SkipSectionSize() {
			this.ReadSectionSize();
		}

		public (int positionSnapshot, int booleanFlagOffsetSnapshot) SnapshotPosition() {
			this.positionSnapshot = this.position;
			this.booleanFlagOffsetSnapshot = this.booleanFlagOffset;

			return (this.positionSnapshot, this.booleanFlagOffsetSnapshot);
		}

		public void Rewind2Snapshot() {
			this.Rewind2Snapshot(this.positionSnapshot, this.booleanFlagOffsetSnapshot);
		}

		public void Rewind2Snapshot(int snapshot, int booleanFlagOffsetSnapshot) {
			this.position = snapshot;
			this.booleanFlagOffset = booleanFlagOffsetSnapshot;

			this.positionSnapshot = 0;
			this.booleanFlagOffsetSnapshot = 0;
		}

		public void Rewind2Start() {
			this.position = 0;
			this.booleanFlagOffset = 0;
		}

		public void RehydrateRewind(Action<DataRehydrator> action) {

			this.SnapshotPosition();
			action(this);
			this.Rewind2Snapshot();
		}
		
		public void RehydrateRewind<T>(T entry)
			where T : IBinaryRehydratable {

			this.SnapshotPosition();
			this.Rehydrate(entry);
			this.Rewind2Snapshot();
		}

		public T RehydrateRewind<T>()
			where T : IBinaryRehydratable, new() {
			T entry = new T();
			this.RehydrateRewind(entry);

			return entry;
		}

		public void Rehydrate<T>(T entry)
			where T : IBinaryRehydratable {

			entry.Rehydrate(this);
		}

		public T Rehydrate<T>()
			where T : IBinaryRehydratable, new() {
			T entry = new T();
			this.Rehydrate(entry);

			return entry;
		}

		public void Peek(Action<IDataRehydrator> action) {
			this.SnapshotPosition();

if(			action != null){ 			action(this);}

			this.Rewind2Snapshot();
		}

		public void UpdateMaximumReadSize(int maximumReadSize) {
			this.maximumReadSize = maximumReadSize;
		}

		/// <summary>
		///     this method will read an array, and return another hydrator pointing to the array memory, without any copying of
		///     bytes.
		/// </summary>
		/// <returns></returns>
		public IDataRehydrator GetArrayHydrator() {
			(int size, int booleanFlagCount, int _) = this.ReadSectionSize();

			return this.CreateRehydrator();
		}

		public ReadOnlySpan<byte> Span => this.Data.Span;

		public ReadOnlySpan<byte> GetSlice(int length) {
			return this.Span.Slice(this.ActualPosition, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReadBytes(in Span<byte> array) {

			this.ReadBytes(array, 0, array.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual void ReadBytes(in Span<byte> array, int offset, int length) {

			this.PreCheckMaximReadSize(length);

			var span = this.GetSlice(length);

			span.CopyTo(array.Slice(offset, length));
			this.position += length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual byte ReadByte() {

			this.PreCheckMaximReadSize(sizeof(byte));
			byte result = this.Data[this.ActualPosition];
			this.position += sizeof(byte);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte? ReadNullableByte() {

			if(this.ReadIsNull()) {
				return null;
			}

			return this.ReadByte();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual short ReadShort() {
			this.PreCheckMaximReadSize(sizeof(short));

			TypeSerializer.Deserialize(this.Data, this.ActualPosition, out short result);
			this.position += sizeof(short);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public short? ReadNullableShort() {
			if(this.ReadIsNull()) {
				return null;
			}

			return this.ReadShort();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual ushort ReadUShort() {
			this.PreCheckMaximReadSize(sizeof(ushort));

			TypeSerializer.Deserialize(this.Data, this.ActualPosition, out ushort result);
			this.position += sizeof(ushort);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ushort? ReadNullableUShort() {
			if(this.ReadIsNull()) {
				return null;
			}

			return this.ReadUShort();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual int ReadInt() {
			this.PreCheckMaximReadSize(sizeof(int));
			TypeSerializer.Deserialize(this.Data, this.ActualPosition, out int result);
			this.position += sizeof(int);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int? ReadNullableInt() {
			if(this.ReadIsNull()) {
				return null;
			}

			return this.ReadInt();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual uint ReadUInt() {
			this.PreCheckMaximReadSize(sizeof(uint));
			TypeSerializer.Deserialize(this.Data, this.ActualPosition, out uint result);
			this.position += sizeof(uint);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint? ReadNullableUInt() {
			if(this.ReadIsNull()) {
				return null;
			}

			return this.ReadUInt();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual long ReadLong() {
			this.PreCheckMaximReadSize(sizeof(long));
			TypeSerializer.Deserialize(this.Data, this.ActualPosition, out long result);
			this.position += sizeof(long);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long? ReadNullableLong() {
			if(this.ReadIsNull()) {
				return null;
			}

			return this.ReadLong();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual ulong ReadULong() {
			this.PreCheckMaximReadSize(sizeof(ulong));
			TypeSerializer.Deserialize(this.Data, this.ActualPosition, out ulong result);
			this.position += sizeof(ulong);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ulong? ReadNullableULong() {
			if(this.ReadIsNull()) {
				return null;
			}

			return this.ReadULong();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual float ReadFloat() {
			this.PreCheckMaximReadSize(sizeof(float));

			TypeSerializer.Deserialize(this.Data, this.ActualPosition, out float result);

			this.position += sizeof(float);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float? ReadNullableFloat() {
			if(this.ReadIsNull()) {
				return null;
			}

			return this.ReadFloat();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual double ReadDouble() {
			this.PreCheckMaximReadSize(sizeof(double));

			TypeSerializer.Deserialize(this.Data, this.ActualPosition, out double result);

			this.position += sizeof(double);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double? ReadNullableDouble() {
			if(this.ReadIsNull()) {
				return null;
			}

			return this.ReadDouble();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ReadBool() {

			return this.booleanFlags[this.booleanFlagOffset++];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool? ReadNullableBool() {
			if(this.ReadIsNull()) {
				return null;
			}

			return this.ReadBool();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Guid ReadGuid() {
			this.PreCheckMaximReadSize(16);

			Span<byte> buffer = stackalloc byte[16];

			this.Data.Span.Slice(this.ActualPosition, buffer.Length).CopyTo(buffer);

			this.position += buffer.Length;
			
				return new Guid(buffer);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Guid? ReadNullableGuid() {
			if(this.ReadIsNull()) {
				return null;
			}

			return this.ReadGuid();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ReadString() {

			ByteArray bytes = this.ReadNullEmptyArray();

			if(bytes == null) {
				return null;
			}

			string value = Encoding.UTF8.GetString(bytes.Bytes, bytes.Offset, bytes.Length);

			bytes.Return();

			return value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DateTime ReadDateTime() {
			long binary = this.ReadLong();

			return DateTime.FromBinary(binary);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DateTime? ReadNullableDateTime() {
			if(this.ReadIsNull()) {
				return null;
			}

			return this.ReadDateTime();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteArray ReadNonNullableArray() {

			int size = this.ReadSize().value;

			return size == 0 ? ByteArray.Empty() : this.ReadArray(size);

		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteArray ReadNullEmptyArray() {

			if(this.ReadIsNull()) {
				return null;
			}

			return this.ReadNonNullableArray();

		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteArray ReadArray() {

			ByteArray result = this.ReadNullEmptyArray();

			return result ?? ByteArray.Empty();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteArray ReadSmallArray() {

			ByteArray result = this.ReadNullableSmallArray();

			return result ?? ByteArray.Empty();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteArray ReadNullableSmallArray() {
			
			int size = this.ReadByte();

			return size == 0 ? null : this.ReadArray(size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteArray ReadArray(int length) {

			this.PreCheckMaximReadSize(length);

			ByteArray buffer = ByteArray.Create(length);
			buffer.CopyFrom(this.Data.Entry, this.ActualPosition, buffer.Length);

			this.position += buffer.Length;

			return buffer;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteArray ReadArrayToEnd() {

			this.PreCheckMaximReadSize(this.RemainingLength);

			ByteArray buffer = ByteArray.Create(this.RemainingLength);
			buffer.CopyFrom(this.Data.Entry, this.ActualPosition, buffer.Length);

			this.position += buffer.Length;

			if(!this.IsEnd) {
				throw new ApplicationException("Reading array until end did not reach the end of the buffer");
			}

			return buffer;
		}

		public List<T> ReadRehydratableArray<T>()
			where T : class, IBinaryRehydratable, new() {

			return this.ReadRehydratableArray(() => new T(), null);
		}

		public List<T> ReadRehydratableArray<T>(RehydrationDelegate<T> rehydrationDelegate)
			where T : class, IBinaryRehydratable {

			return this.ReadRehydratableArray((Func<T>) null, rehydrationDelegate);
		}

		public List<T> ReadRehydratableArray<T>(Func<T> creationDelegate, RehydrationDelegate<T> rehydrationDelegate)
			where T : class, IBinaryRehydratable {

			var results = new List<T>();

			this.ReadRehydratableArray(results, creationDelegate, rehydrationDelegate);

			return results;
		}

		public void ReadRehydratableArray<T>(List<T> collection)
			where T : IBinaryRehydratable, new() {

			this.ReadRehydratableArray(collection, () => new T(), null);
		}

		public void ReadRehydratableArray<T>(List<T> collection, RehydrationDelegate<T> rehydrationDelegate)
			where T : IBinaryRehydratable {

			this.ReadRehydratableArray(collection, null, rehydrationDelegate);
		}

		public void ReadRehydratableArray<T>(List<T> collection, Func<T> creationDelegate, RehydrationDelegate<T> rehydrationDelegate)
			where T : IBinaryRehydratable {

			bool any = this.ReadBool();

			if(any) {
				int count = this.ReadSize().value;

				for(int i = 0; i < count; i++) {
					collection.Add(this.ReadRehydratable(creationDelegate, rehydrationDelegate));
				}
			}
		}

		public T ReadRehydratable<T>()
			where T : IBinaryRehydratable, new() {

			return this.ReadRehydratable(() => new T(), null);
		}

		public T ReadRehydratable<T>(RehydrationDelegate<T> rehydrationDelegate)
			where T : IBinaryRehydratable {

			return this.ReadRehydratable(null, rehydrationDelegate);
		}

		public T ReadRehydratable<T>(Func<T> creationDelegate, RehydrationDelegate<T> rehydrationDelegate)
			where T : IBinaryRehydratable {

			T value = default;

			if(creationDelegate != null) {
				value = creationDelegate.Invoke();
			}

			if(rehydrationDelegate != null) {
				if(this.ReadBool()) {
					rehydrationDelegate(this, ref value);
				}
			} else {
				this.ReadRehydratable(ref value);
			}

			return value;
		}

		public void ReadRehydratable<T>(ref T entry)
			where T : IBinaryRehydratable {

			if(this.ReadBool()) {
				this.ReadNotNullableRehydratable(ref entry);
			} else {
				entry = default;
			}
		}

		public T ReadNotNullableRehydratable<T>()
			where T : IBinaryRehydratable, new() {

			return this.ReadNotNullableRehydratable(() => new T(), null);
		}

		public T ReadNotNullableRehydratable<T>(RehydrationDelegate<T> rehydrationDelegate)
			where T : IBinaryRehydratable {

			return this.ReadNotNullableRehydratable(null, rehydrationDelegate);
		}

		public T ReadNotNullableRehydratable<T>(Func<T> creationDelegate, RehydrationDelegate<T> rehydrationDelegate)
			where T : IBinaryRehydratable {

			T value = default;

			if(creationDelegate != null) {
				value = creationDelegate.Invoke();
			}

			if(rehydrationDelegate != null) {
				rehydrationDelegate(this, ref value);
			} else {
				this.ReadNotNullableRehydratable(ref value);
			}

			return value;
		}

		public void ReadNotNullableRehydratable<T>(ref T entry)
			where T : IBinaryRehydratable {
			entry.Rehydrate(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref byte value) {
			value = this.ReadByte();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref byte? value) {
			value = this.ReadNullableByte();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref short value) {
			value = this.ReadShort();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref short? value) {
			value = this.ReadNullableShort();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref ushort value) {
			value = this.ReadUShort();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref ushort? value) {
			value = this.ReadNullableUShort();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref int value) {
			value = this.ReadInt();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref int? value) {
			value = this.ReadNullableInt();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref uint value) {
			value = this.ReadUInt();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref uint? value) {
			value = this.ReadNullableUInt();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref long value) {
			value = this.ReadLong();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref long? value) {
			value = this.ReadNullableLong();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref ulong value) {
			value = this.ReadULong();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref ulong? value) {
			value = this.ReadNullableULong();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref bool value) {
			value = this.ReadBool();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref bool? value) {
			value = this.ReadNullableBool();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref float value) {
			value = this.ReadFloat();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref float? value) {
			value = this.ReadNullableFloat();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref double value) {
			value = this.ReadDouble();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref double? value) {
			value = this.ReadNullableDouble();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref Guid value) {
			value = this.ReadGuid();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref Guid? value) {
			value = this.ReadNullableGuid();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref DateTime value) {
			value = this.ReadDateTime();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref DateTime? value) {
			value = this.ReadNullableDateTime();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref string value) {
			value = this.ReadString();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDataRehydrator Read(ref SafeArrayHandle value) {
			value = this.ReadArray();

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PreCheckMaximReadSize(int expectedSize) {
			if((this.ActualPosition + expectedSize) > this.maximumReadSize) {
				throw new ApplicationException("More data would be read than is allowed by the maximum read size");
			}
		}

		protected virtual int ExtractMetadata(SafeArrayHandle data, int offset, int length) {

			// empty data has no metadata
			if((length - offset) == 0) {
				this.booleanFlags = new BitArray(0);

				return 0;
			}

			// now remove the version byte and the null flags
			var slice = data.Span.Slice(offset, length);

			int negativeOffset = 1;

			//always the last byte
			byte entry = slice[slice.Length - 1];

			BitSequence sequence = new BitSequence(entry, DataDehydrator.entries);

			if((byte) sequence.GetEntryValue(0) != this.version) {
				throw new ApplicationException("we have an invalid version");
			}

			// ok, now we extract the null flag array
			int nullFlagByteSize = (byte) sequence.GetEntryValue(1);

			if(nullFlagByteSize == 0xF) {
				// ok, we have a dynamic size, its in the second, third, fourth and fifth bytes depending on size
				byte firstEntry = slice[slice.Length - 2];

				int extraByteSize = (firstEntry & 0x3) + 1;

				int rebuiltSize = 0;

				for(int i = 0; i < extraByteSize; i++) {
					if(i == 0) {
						rebuiltSize = (firstEntry & 0xFC) >> 2;
					} else {
						rebuiltSize |= slice[slice.Length - 2 - i] << (((i - 1) * 8) + 6);
					}
				}

				nullFlagByteSize = rebuiltSize;
				negativeOffset += extraByteSize;
			}

			// ok, get the byte array
			int end = slice.Length - negativeOffset - 1;

			this.booleanFlags = new BitArray(slice.Slice(end - (nullFlagByteSize - 1), nullFlagByteSize).ToArray());

			// and return the metadata size
			return negativeOffset + nullFlagByteSize;
		}

		protected abstract void SetVersion();

		/// <summary>
		///     move forward by x amount
		/// </summary>
		/// <param name="forwardPosition"></param>
		public void Forward(int forwardPosition, int boolFlagCount) {
			this.position += forwardPosition;
			this.booleanFlagOffset += boolFlagCount;
		}

		protected (int value, int sizeByteSize) ReadSize() {

			int sizeOffset = this.sizeSerializationHelper.Rehydrate(this);

			return (this.sizeSerializationHelper.Size, sizeOffset);
		}

		protected abstract IDataRehydrator CreateRehydrator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool ReadIsNull() {
			// negative becase we dehydrated if it HAD a value. null is the oposite, when there is none.
			return !this.ReadBool();
		}
		
	#region Disposable
		

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {

			if(this.IsDisposed || !disposing) {
				return;
			}
			
			this.Data?.Dispose();

			this.IsDisposed = true;
		}

		~DataRehydrator() {
			this.Dispose(false);
		}

		public bool IsDisposed { get; private set; }

	#endregion
	}
}