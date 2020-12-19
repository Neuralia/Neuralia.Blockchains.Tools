using System;

namespace Neuralia.Blockchains.Tools.General.ExclusiveOptions {
	public class ShortExclusiveOption {
		public ShortExclusiveOption() {

		}

		public ShortExclusiveOption(short value) : this() {
			this.Value = value;
		}

		public short Value { get; set; }

		public void SetOption(short option) {
			this.Value |= option;
		}

		public void SetOptionValue(short option, bool value) {
			if(value) {
				this.SetOption(option);
			} else {
				this.RemoveOption(option);
			}
		}

		public void RemoveOption(short option) {
			this.Value &= (short) ~option;
		}

		public void ToggleOption(short option) {
			this.Value ^= option;
		}

		public bool HasOption(short option) {
			return (this.Value & option) == option;
		}

		public bool MissesOption(short option) {
			return !this.HasOption(option);
		}

		public static implicit operator short(ShortExclusiveOption other) {
			return other.Value;
		}

		public static implicit operator ShortExclusiveOption(short other) {
			return new ShortExclusiveOption(other);
		}

		public override bool Equals(object obj) {
			if((obj == null) || !(obj is ShortExclusiveOption other)) {
				return false;
			}

			return this.Value == other.Value;
		}

		public override int GetHashCode() {
			return this.Value.GetHashCode();
		}
	}

	public class ShortExclusiveOption<T> : ShortExclusiveOption
		where T : struct, IConvertible {
		public ShortExclusiveOption() {
			if(!typeof(T).IsEnum) {
				throw new ArgumentException("T must be an enumerated type");
			}
		}

		public ShortExclusiveOption(short value) : base(value) {

		}

		public ShortExclusiveOption(T value) : base((short) (object) value) {

		}

		public void SetOptionValue(T option, bool value) {
			if(value) {
				this.SetOption(option);
			} else {
				this.RemoveOption(option);
			}
		}

		public void SetOption(T option) {
			this.SetOption((short) (object) option);
		}

		public void RemoveOption(T option) {
			this.RemoveOption((short) (object) option);
		}

		public void ToggleOption(T option) {
			this.ToggleOption((short) (object) option);
		}

		public bool HasOption(T option) {
			return this.HasOption((short) (object) option);
		}

		public bool MissesOption(T option) {
			return !this.HasOption(option);
		}

		public static implicit operator short(ShortExclusiveOption<T> other) {
			return other.Value;
		}

		public static implicit operator ShortExclusiveOption<T>(short other) {
			return new ShortExclusiveOption<T>(other);
		}

		public static implicit operator ShortExclusiveOption<T>(T other) {
			return new ShortExclusiveOption<T>(other);
		}
	}
}