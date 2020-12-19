namespace Neuralia.Blockchains.Tools.Serialization.V1 {
	public class DataDehydratorV1 : DataDehydrator {

		protected override void SetVersion() {
			this.version = 1;
		}
	}
}