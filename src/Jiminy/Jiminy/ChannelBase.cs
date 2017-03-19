using System;

namespace Jiminy {
	abstract class ChannelBase {
		public abstract Guid Id { get; }

		public bool Equals(ChannelBase other) {
			if (ReferenceEquals(other, null)) return false;
			return other.Id == Id;
		}

		public override bool Equals(object obj) {
			return Equals(obj as ChannelBase);
		}

		public override int GetHashCode() {
			return Id.GetHashCode();
		}
	}
}
