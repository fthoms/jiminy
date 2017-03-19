using Jiminy;
using System;

namespace Jiminy {
	public sealed class Error {
		public readonly string description;

		Error(string description) {
			this.description = description;
		}

		public bool Equals(Error other) {
			if (ReferenceEquals(other, null)) return false;
			return other.description.Equals(description);
		}

		public override bool Equals(object obj) {
			return Equals(obj as Error);
		}

		public override int GetHashCode() {
			return description.GetHashCode();
		}

		public override string ToString() {
			return description;
		}

		public static bool operator ==(Error left, Error right) {
			if (ReferenceEquals(left, null)) {
				if (ReferenceEquals(right, null)) return true;
				return false;
			}
			return left.Equals(right);
		}

		public static bool operator !=(Error left, Error right) => !(left == right);

		public static Error DescribedBy(string description) => new Error(description);
		public static Error DescribedBy(Exception exception) => DescribedBy($"{exception.GetType().Name}: {exception.Message}");
		public static Error DescribedBy(string description, Exception exception) => DescribedBy($"{description}: {exception.Message}");
		public static implicit operator Error(string description) => DescribedBy(description);
		public static implicit operator Error(Exception exception) => DescribedBy(exception);

		public static Error ChannelClosed => "Channel closed".AsError();
		public static Error Nil => null;
        public static Error NoMessages => "No messages".AsError();
	}
}

public static class ErrorExtensions {
	public static Error AsError(this string description) => Error.DescribedBy(description);
	public static Error AsError(this Exception exception) => Error.DescribedBy(exception);
}
