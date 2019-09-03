using Neuralia.Blockchains.Tools.Data;

namespace Neuralia.Blockchains.Tools.Serialization.V1 {
	public class DataRehydratorV1 : DataRehydrator {

		public DataRehydratorV1(byte[] data, bool metadata = true) : base(data, metadata) {
		}

		public DataRehydratorV1(IByteArray data, bool metadata = true) : base(data, metadata) {
		}

		public DataRehydratorV1(IByteArray data, int length, bool metadata = true) : base(data, length, metadata) {
		}

		public DataRehydratorV1(IByteArray data, int length, int maximumReadSize, bool metadata = true) : base(data, length, maximumReadSize, metadata) {
		}

		public DataRehydratorV1(IByteArray data, int offset, int length, int maximumReadSize, bool metadata = true) : base(data, offset, length, maximumReadSize, metadata) {
		}

		protected override void SetVersion() {
			this.version = 1;
		}

		protected override IDataRehydrator CreateRehydrator() {
			return DataSerializationFactory.CreateRehydrator(this.Data, this.position, this.length);
		}
	}
}