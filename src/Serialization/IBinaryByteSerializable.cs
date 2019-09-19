using Neuralia.Blockchains.Tools.Data;

namespace Neuralia.Blockchains.Tools.Serialization {
	public interface IBinaryByteSerializable {
		SafeArrayHandle Dehydrate();
		void Rehydrate(SafeArrayHandle data);
	}
}