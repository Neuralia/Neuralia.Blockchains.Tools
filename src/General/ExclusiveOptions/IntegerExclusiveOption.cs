using System;

namespace Neuralia.Blockchains.Tools.General.ExclusiveOptions {
	public class IntegerExclusiveOption {
		public IntegerExclusiveOption() {

		}

		public IntegerExclusiveOption(int value) : this() {
			this.Value = value;
		}

		public int Value { get; set; }

		public void SetOption(int option) {
			this.Value |= option;
		}

		public void SetOptionValue(int option, bool value) {
			if(value) {
				this.SetOption(option);
			} else {
				this.RemoveOption(option);
			}
		}

		public void RemoveOption(int option) {
			this.Value &= ~option;
		}

		public void ToggleOption(int option) {
			this.Value ^= option;
		}

		public bool HasOption(int option) {
			return (this.Value & option) == option;
		}

		public bool MissesOption(int option) {
			return !this.HasOption(option);
		}

		public static implicit operator int(IntegerExclusiveOption other) {
			return other.Value;
		}

		public static implicit operator IntegerExclusiveOption(int other) {
			return new IntegerExclusiveOption(other);
		}

		public override bool Equals(object obj) {
			if((obj == null) || !(obj is IntegerExclusiveOption other)) {
				return false;
			}

			return this.Value == other.Value;
		}

		public override int GetHashCode() {
			return this.Value.GetHashCode();
		}
	}

	public class IntegerExclusiveOption<T> : IntegerExclusiveOption
		where T : struct, IConvertible {
		public IntegerExclusiveOption() {
			if(!typeof(T).IsEnum) {
				throw new ArgumentException("T must be an enumerated type");
			}
		}

		public IntegerExclusiveOption(int value) : base(value) {

		}

		public IntegerExclusiveOption(T value) : base((int) (object) value) {

		}

		public void SetOption(T option) {
			this.SetOption((int) (object) option);
		}

		public void SetOptionValue(T option, bool value) {
			if(value) {
				this.SetOption(option);
			} else {
				this.RemoveOption(option);
			}
		}

		public void RemoveOption(T option) {
			this.RemoveOption((int) (object) option);
		}

		public void ToggleOption(T option) {
			this.ToggleOption((int) (object) option);
		}

		public bool HasOption(T option) {
			return this.HasOption((int) (object) option);
		}

		public bool MissesOption(T option) {
			return !this.HasOption(option);
		}

		public static implicit operator int(IntegerExclusiveOption<T> other) {
			return other.Value;
		}

		public static implicit operator IntegerExclusiveOption<T>(int other) {
			return new IntegerExclusiveOption<T>(other);
		}

		public static implicit operator IntegerExclusiveOption<T>(T other) {
			return new IntegerExclusiveOption<T>(other);
		}
	}
}