using System;

namespace Neuralia.Blockchains.Tools.General.ExclusiveOptions {
	public class UShortExclusiveOption {
		public UShortExclusiveOption() {

		}

		public UShortExclusiveOption(ushort value) : this() {
			this.Value = value;
		}

		public ushort Value { get; set; }

		public void SetOption(ushort option) {
			this.Value |= option;
		}

		public void SetOptionValue(ushort option, bool value) {
			if(value) {
				this.SetOption(option);
			} else {
				this.RemoveOption(option);
			}
		}

		public void RemoveOption(ushort option) {
			this.Value &= (ushort) ~option;
		}

		public void ToggleOption(ushort option) {
			this.Value ^= option;
		}

		public bool HasOption(ushort option) {
			return (this.Value & option) == option;
		}

		public bool MissesOption(ushort option) {
			return !this.HasOption(option);
		}

		public static implicit operator ushort(UShortExclusiveOption other) {
			return other.Value;
		}

		public static implicit operator UShortExclusiveOption(ushort other) {
			return new UShortExclusiveOption(other);
		}

		public override bool Equals(object obj) {
			if((obj == null) || !(obj is UShortExclusiveOption other)) {
				return false;
			}

			return this.Value == other.Value;
		}

		public override int GetHashCode() {
			return this.Value.GetHashCode();
		}
	}

	public class UShortExclusiveOption<T> : UShortExclusiveOption
		where T : struct, IConvertible {
		public UShortExclusiveOption() {
			if(!typeof(T).IsEnum) {
				throw new ArgumentException("T must be an enumerated type");
			}
		}

		public UShortExclusiveOption(ushort value) : base(value) {

		}

		public UShortExclusiveOption(T value) : base((ushort) (object) value) {

		}

		public void SetOptionValue(T option, bool value) {
			if(value) {
				this.SetOption(option);
			} else {
				this.RemoveOption(option);
			}
		}

		public void SetOption(T option) {
			this.SetOption((ushort) (object) option);
		}

		public void RemoveOption(T option) {
			this.RemoveOption((ushort) (object) option);
		}

		public void ToggleOption(T option) {
			this.ToggleOption((ushort) (object) option);
		}

		public bool HasOption(T option) {
			return this.HasOption((ushort) (object) option);
		}

		public bool MissesOption(T option) {
			return !this.HasOption(option);
		}

		public static implicit operator ushort(UShortExclusiveOption<T> other) {
			return other.Value;
		}

		public static implicit operator UShortExclusiveOption<T>(ushort other) {
			return new UShortExclusiveOption<T>(other);
		}

		public static implicit operator UShortExclusiveOption<T>(T other) {
			return new UShortExclusiveOption<T>(other);
		}
	}
}