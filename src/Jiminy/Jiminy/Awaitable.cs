using System;
using System.Threading;

namespace Jiminy {
	sealed class Awaitable {
		readonly object sync = new object();
		readonly ManualResetEventSlim waithandle = new ManualResetEventSlim(false);

		public bool IsReleased { get; private set; }
		public Guid ReleasedBy { get; private set; }

		public void Wait() {
			waithandle.Wait();
		}

		public void Release(Guid by) {
			lock (sync) {
#if DEBUG
				if (IsReleased) throw new InvalidOperationException("Awaitable already closed");
#endif
				IsReleased = true;
				ReleasedBy = by;
			}
			waithandle.Set();
		}
	}
}
