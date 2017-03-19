using System;
using System.Threading;

namespace Jiminy {
    public sealed class WaitGroup {
        readonly ManualResetEventSlim waithandle = new ManualResetEventSlim(false);
        volatile int remaining = 0;

        public bool IsDone => remaining == 0;

        public WaitGroup() {
        }

        public WaitGroup(int n) {
            Add(n);
        }

        public void Add(int n) {
            lock (this) {
                if (n < 1) {
                    throw new ArgumentException($"WaitGroup.Add({n}): invalid argument");
                }
                remaining += n;
            }
        }

        public void Done() {
            lock (this) {
                remaining--;
                if (remaining < 1) {
                    waithandle.Set();
                }
            }
        }

        public void Wait() {
            waithandle.Wait();
        }
    }
}
