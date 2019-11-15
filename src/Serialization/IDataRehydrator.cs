using System;
using System.Collections.Generic;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Data.Arrays;

namespace Neuralia.Blockchains.Tools.Serialization {
	public interface IDataRehydrator : IDisposableExtended {
		bool IsEnd { get; }
		int Offset { get; }
		int RemainingLength { get; }
		SafeArrayHandle Data { get; }
		ReadOnlySpan<byte> Span { get; }

		/// <summary>
		///     move forward by x amount
		/// </summary>
		/// <param name="forwardPosition"></param>
		void Forward(int forwardPosition);

		/// <summary>
		///     read section size and move forward by X
		/// </summary>
		void SkipSection();

		(int size, int booleanFlagCount, int sizesByteSize) ReadSectionSize();
		void Skip(int length);
		void SkipByte();
		void SkipShort();
		void SkipUShort();
		void SkipInt();
		void SkipUInt();
		void SkipLong();
		void SkipULong();

		/// <summary>
		///     Here we simply skip a section size, since we may not need it
		/// </summary>
		void SkipSectionSize();

		/// <summary>
		///     set a snapshot, read some data and rewind
		/// </summary>
		/// <param name="action"></param>
		void Peek(Action<IDataRehydrator> action);

		(int positionSnapshot, int booleanFlagOffsetSnapshot) SnapshotPosition();
		void Rewind2Snapshot();
		void Rewind2Snapshot(int snapshot, int booleanFlagOffsetSnapshot);
		void Rewind2Start();

		void RehydrateRewind(Action<DataRehydrator> action);
		void RehydrateRewind<T>(T entry)
			where T : IBinaryRehydratable;

		T RehydrateRewind<T>()
			where T : IBinaryRehydratable, new();

		void Rehydrate<T>(T entry)
			where T : IBinaryRehydratable;

		T Rehydrate<T>()
			where T : IBinaryRehydratable, new();

		void UpdateMaximumReadSize(int maximumReadSize);
		int ReadArraySize();

		/// <summary>
		///     this method will read an array, and return another hydrator pointing to the array memory, without any copying of
		///     bytes.
		/// </summary>
		/// <returns></returns>
		IDataRehydrator GetArrayHydrator();

		ReadOnlySpan<byte> GetSlice(int length);
		void ReadBytes(in Span<byte> array);
		void ReadBytes(in Span<byte> array, int offset, int length);
		byte ReadByte();
		byte? ReadNullableByte();
		short ReadShort();
		short? ReadNullableShort();
		ushort ReadUShort();
		ushort? ReadNullableUShort();
		int ReadInt();
		int? ReadNullableInt();
		uint ReadUInt();
		uint? ReadNullableUInt();
		long ReadLong();
		long? ReadNullableLong();
		ulong ReadULong();
		ulong? ReadNullableULong();
		float ReadFloat();
		float? ReadNullableFloat();
		double ReadDouble();
		double? ReadNullableDouble();
		bool ReadBool();
		bool? ReadNullableBool();
		Guid ReadGuid();
		Guid? ReadNullableGuid();
		string ReadString();
		DateTime ReadDateTime();
		DateTime? ReadNullableDateTime();
		ByteArray ReadNonNullableArray();
		ByteArray ReadNullEmptyArray();
		ByteArray ReadArray();
		ByteArray ReadSmallArray();
		ByteArray ReadNullableSmallArray();
		ByteArray ReadArray(int length);
		ByteArray ReadArrayToEnd();

		List<T> ReadRehydratableArray<T>()
			where T : class, IBinaryRehydratable, new();

		List<T> ReadRehydratableArray<T>(DataRehydrator.RehydrationDelegate<T> rehydrationDelegate)
			where T : class, IBinaryRehydratable;

		List<T> ReadRehydratableArray<T>(Func<T> creationDelegate, DataRehydrator.RehydrationDelegate<T> rehydrationDelegate)
			where T : class, IBinaryRehydratable;

		void ReadRehydratableArray<T>(List<T> collection)
			where T : IBinaryRehydratable, new();

		void ReadRehydratableArray<T>(List<T> collection, DataRehydrator.RehydrationDelegate<T> rehydrationDelegate)
			where T : IBinaryRehydratable;

		void ReadRehydratableArray<T>(List<T> collection, Func<T> creationDelegate, DataRehydrator.RehydrationDelegate<T> rehydrationDelegate)
			where T : IBinaryRehydratable;

		T ReadRehydratable<T>()
			where T : IBinaryRehydratable, new();

		T ReadRehydratable<T>(DataRehydrator.RehydrationDelegate<T> rehydrationDelegate)
			where T : IBinaryRehydratable;

		T ReadRehydratable<T>(Func<T> creationDelegate, DataRehydrator.RehydrationDelegate<T> rehydrationDelegate)
			where T : IBinaryRehydratable;

		void ReadRehydratable<T>(ref T entry)
			where T : IBinaryRehydratable;

		T ReadNotNullableRehydratable<T>()
			where T : IBinaryRehydratable, new();

		T ReadNotNullableRehydratable<T>(DataRehydrator.RehydrationDelegate<T> rehydrationDelegate)
			where T : IBinaryRehydratable;

		T ReadNotNullableRehydratable<T>(Func<T> creationDelegate, DataRehydrator.RehydrationDelegate<T> rehydrationDelegate)
			where T : IBinaryRehydratable;

		void ReadNotNullableRehydratable<T>(ref T entry)
			where T : IBinaryRehydratable;

		IDataRehydrator Read(ref byte value);
		IDataRehydrator Read(ref byte? value);
		IDataRehydrator Read(ref short value);
		IDataRehydrator Read(ref short? value);
		IDataRehydrator Read(ref ushort value);
		IDataRehydrator Read(ref ushort? value);
		IDataRehydrator Read(ref int value);
		IDataRehydrator Read(ref int? value);
		IDataRehydrator Read(ref uint value);
		IDataRehydrator Read(ref uint? value);
		IDataRehydrator Read(ref long value);
		IDataRehydrator Read(ref long? value);
		IDataRehydrator Read(ref ulong value);
		IDataRehydrator Read(ref ulong? value);
		IDataRehydrator Read(ref bool value);
		IDataRehydrator Read(ref bool? value);
		IDataRehydrator Read(ref float value);
		IDataRehydrator Read(ref float? value);
		IDataRehydrator Read(ref double value);
		IDataRehydrator Read(ref double? value);
		IDataRehydrator Read(ref Guid value);
		IDataRehydrator Read(ref Guid? value);
		IDataRehydrator Read(ref DateTime value);
		IDataRehydrator Read(ref DateTime? value);
		IDataRehydrator Read(ref string value);
		IDataRehydrator Read(ref SafeArrayHandle value);
		void PreCheckMaximReadSize(int expectedSize);
	}
}