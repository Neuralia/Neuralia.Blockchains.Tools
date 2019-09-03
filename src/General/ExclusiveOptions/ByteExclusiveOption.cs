using System;

namespace Neuralia.Blockchains.Tools.General.ExclusiveOptions {
	public class ByteExclusiveOption {
		public ByteExclusiveOption() {

		}

		public ByteExclusiveOption(byte value) : this() {
			this.Value = value;
		}

		public byte Value { get; set; }

		public void SetOption(byte option) {
			this.Value |= option;
		}

		public void SetOptionValue(byte option, bool value) {
			if(value) {
				this.SetOption(option);
			} else {
				this.RemoveOption(option);
			}
		}

		public void RemoveOption(byte option) {
			this.Value &= (byte) ~option;
		}

		public void ToggleOption(byte option) {
			this.Value ^= option;
		}

		public bool HasOption(byte option) {
			return (this.Value & option) == option;
		}

		public bool MissesOption(byte option) {
			return !this.HasOption(option);
		}

		public static implicit operator byte(ByteExclusiveOption other) {
			return other.Value;
		}

		public static implicit operator ByteExclusiveOption(byte other) {
			return new ByteExclusiveOption(other);
		}

		public override bool Equals(object obj) {
			if((obj == null) || !(obj is ByteExclusiveOption other)) {
				return false;
			}

			return this.Value == other.Value;
		}

		public override int GetHashCode() {
			return this.Value.GetHashCode();
		}
	}

	public class ByteExclusiveOption<T> : ByteExclusiveOption
		where T : struct, IConvertible {
		public ByteExclusiveOption() {
			if(!typeof(T).IsEnum) {
				throw new ArgumentException("T must be an enumerated type");
			}
		}

		public ByteExclusiveOption(byte value) : base(value) {

		}

		public ByteExclusiveOption(T value) : base((byte) (object) value) {

		}

		public void SetOption(T option) {
			this.SetOption((byte) (object) option);
		}

		public void SetOptionValue(T option, bool value) {
			if(value) {
				this.SetOption(option);
			} else {
				this.RemoveOption(option);
			}
		}

		public void RemoveOption(T option) {
			this.RemoveOption((byte) (object) option);
		}

		public void ToggleOption(T option) {
			this.ToggleOption((byte) (object) option);
		}

		public bool HasOption(T option) {
			return this.HasOption((byte) (object) option);
		}

		public bool MissesOption(T option) {
			return !this.HasOption(option);
		}

		public static implicit operator byte(ByteExclusiveOption<T> other) {
			return other.Value;
		}

		public static implicit operator ByteExclusiveOption<T>(byte other) {
			return new ByteExclusiveOption<T>(other);
		}

		public static implicit operator ByteExclusiveOption<T>(T other) {
			return new ByteExclusiveOption<T>(other);
		}
	}
}