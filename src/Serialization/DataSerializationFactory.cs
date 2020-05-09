using System;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Data.Arrays;
using Neuralia.Blockchains.Tools.General;
using Neuralia.Blockchains.Tools.Serialization.V1;

namespace Neuralia.Blockchains.Tools.Serialization {
	public static class DataSerializationFactory {

		public const byte CURRENT_DEFAULT_VERSION = 1;

		private static byte current_version = CURRENT_DEFAULT_VERSION;

		public static void SetCurrentVersion(byte version = CURRENT_DEFAULT_VERSION) {
			current_version = version;
		}

		public static IDataDehydrator CreateDehydrator(byte? version = null) {

			if(!version.HasValue) {
				version = current_version;
			}

			if(version == 1) {
				return new DataDehydratorV1();
			}

			throw new ApplicationException("Unsupported dehydrator version");
		}

		public static IDataRehydrator CreateRehydrator(byte[] data) {

			return CreateRehydrator(ByteArray.Wrap(data));
		}

		public static IDataRehydrator CreateRehydrator(SafeArrayHandle data) {

			return CreateRehydrator(data, data?.Length ?? 0);
		}

		public static IDataRehydrator CreateRehydrator(SafeArrayHandle data, int length) {

			return CreateRehydrator(data, length, length);
		}

		public static IDataRehydrator CreateRehydrator(SafeArrayHandle data, int length, int maximumReadSize) {

			return CreateRehydrator(data, 0, length, maximumReadSize);
		}

		public static IDataRehydrator CreateRehydrator(SafeArrayHandle data, int offset, int length, int maximumReadSize) {

			byte version = GetVersion(data, offset, length);

			if(version == 1) {
				return new DataRehydratorV1(data.Branch(), offset, length, maximumReadSize);
			}

			throw new ApplicationException("Unsupported dehydrator version");
		}

		/// <summary>
		///     Get the value from the dehydrated data which is always in the last byte
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		private static byte GetVersion(SafeArrayHandle data, int offset, int length) {
			if(data.IsEmpty) {
				return CURRENT_DEFAULT_VERSION;
			}

			Span<byte> slice = data.Span.Slice(offset, length);

			//always the last byte
			byte entry = slice[slice.Length - 1];

			BitSequence sequence = new BitSequence(entry, DataDehydrator.entries);

			return (byte) sequence.GetEntryValue(0);
		}
	}
}