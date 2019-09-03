using Neuralia.Blockchains.Tools.Data;

namespace Neuralia.Blockchains.Tools.Serialization {
	public interface IBinaryByteSerializable {
		IByteArray Dehydrate();
		void Rehydrate(IByteArray data);
	}
}