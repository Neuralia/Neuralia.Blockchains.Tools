namespace Neuralia.Blockchains.Tools.General {
	
	/// <summary>
	/// used to shield byval values from closure wrapping
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ClosureWrapper<T> {
		
		public ClosureWrapper(T value = default) {
			this.Value = value;
		}
		public T Value { get; set; }

		public bool IsDefault => this.Value?.Equals(default)??true;
		
		public static implicit operator ClosureWrapper<T>(T value) {
			return new ClosureWrapper<T>(value);
		}

		public static implicit operator T(ClosureWrapper<T> wrapper) {
			return wrapper.ToT();
		}

        public T ToT()
        {
	        return this.Value;
        }
    }
}