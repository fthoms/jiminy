using System;

namespace Jiminy {
	sealed class AwaitableMessage<T> {
		readonly object sync = new object();
		readonly Awaitable awaitable = new Awaitable();

		public T Message { get; private set; }
		public Guid ReleasedBy => awaitable.ReleasedBy;
		public bool IsReleased => awaitable.IsReleased;

		public void Wait() => awaitable.Wait();

		public void Release(Guid by, T message) {
			lock (sync) {
				Message = message;
				awaitable.Release(by);
			}
		}
	}
}
