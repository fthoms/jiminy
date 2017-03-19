using System.Threading;

namespace Jiminy.ChannelListeners {
    abstract class ChannelListenerBase<T> {
        protected readonly ManualResetEventSlim waithandle = new ManualResetEventSlim(false);
        protected T message;
        protected Error error = Error.Nil;

        public bool Discard { get; protected set; }

        public virtual (T, Error) Result() {
            waithandle.Wait();
            return (message, error);
        }
    }
}
