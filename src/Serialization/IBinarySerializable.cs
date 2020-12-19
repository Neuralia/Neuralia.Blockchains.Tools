namespace Neuralia.Blockchains.Tools.Serialization {
	public interface IBinaryRehydratable {
		void Rehydrate(IDataRehydrator rehydrator);
	}

	public interface IBinaryDehydratable {
		void Dehydrate(IDataDehydrator dehydrator);
	}

	public interface IBinarySerializable : IBinaryRehydratable, IBinaryDehydratable {
	}
}