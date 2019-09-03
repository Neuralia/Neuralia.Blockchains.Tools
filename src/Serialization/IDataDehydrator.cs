using System;
using System.Buffers;
using System.Collections.Generic;
using Neuralia.Blockchains.Tools.Data;

namespace Neuralia.Blockchains.Tools.Serialization {
	public interface IDataDehydrator : IDisposable2 {
		long Position { get; }
		int Length { get; }
		int MetadataSize { get; }
		byte Version { get; }
		IDataDehydrator Write(IEnumerable<IBinaryDehydratable> value);
		IDataDehydrator Write(IBinaryDehydratable value);
		IDataDehydrator WriteNoNullable(IBinaryDehydratable value);

		IDataDehydrator Write(byte value);
		IDataDehydrator Write(byte? value);
		IDataDehydrator Write(short value);
		IDataDehydrator Write(short? value);
		IDataDehydrator Write(ushort value);
		IDataDehydrator Write(ushort? value);
		IDataDehydrator Write(int value);
		IDataDehydrator Write(int? value);
		IDataDehydrator Write(uint value);
		IDataDehydrator Write(uint? value);
		IDataDehydrator Write(long value);
		IDataDehydrator Write(long? value);
		IDataDehydrator Write(ulong value);
		IDataDehydrator Write(ulong? value);
		IDataDehydrator Write(float value);
		IDataDehydrator Write(float? value);
		IDataDehydrator Write(double value);
		IDataDehydrator Write(double? value);
		IDataDehydrator Write(bool value);
		IDataDehydrator Write(bool? value);
		IDataDehydrator Write(Guid value);
		IDataDehydrator Write(Guid? value);
		IDataDehydrator Write(string value);
		IDataDehydrator Write(DateTime value);
		IDataDehydrator Write(DateTime? value);
		IDataDehydrator WriteRawArray(IByteArray array);

		IDataDehydrator WriteRawArray(ByteArray array);
		IDataDehydrator WriteRawArray(ReadOnlySequence<byte> sequence);
		IDataDehydrator WriteRawArray(byte[] array);

		IDataDehydrator WriteRawArray(Span<byte> span);
		IDataDehydrator Write(byte[] array);
		IDataDehydrator WriteNonNullable(byte[] array);
		IDataDehydrator Write(in Span<byte> span);

		IDataDehydrator WriteSmallArray(byte[] array);
		IDataDehydrator WriteSmallArray(in Span<byte> span);
		IDataDehydrator Write(IByteArray array);
		IDataDehydrator WriteNonNullable(IByteArray array);
		IDataDehydrator WriteWrappedContent(Action<IDataDehydrator> action);

		/// <summary>
		///     Inject the contents of a dehydrator as is.
		/// </summary>
		/// <param name="other"></param>
		void InjectDehydrator(IDataDehydrator other, bool insertRaw = false);

		/// <summary>
		///     This method will return the content without the metadata. it is intended to be injected in another dehydrator
		/// </summary>
		/// <returns></returns>
		(IByteArray data, List<bool> booleanFlags) ToComponentsArray();

		/// <summary>
		///     Return the raw awway without any metadata details
		/// </summary>
		/// <returns></returns>
		IByteArray ToRawArray();

		/// <summary>
		///     will return null if the stream is empty
		/// </summary>
		/// <returns></returns>
		IByteArray ToNullableRawArray();

		/// <summary>
		///     return the contents and the size of the metadata
		/// </summary>
		/// <returns></returns>
		(IByteArray data, int metadataSize) ToArrayAndMetadata();

		/// <summary>
		///     This method will return the content wrapped with the required metadata.
		/// </summary>
		/// <returns></returns>
		IByteArray ToArray();

		/// <summary>
		///     will return null if the stream is empty
		/// </summary>
		/// <returns></returns>
		IByteArray ToNullableArray();
	}
}