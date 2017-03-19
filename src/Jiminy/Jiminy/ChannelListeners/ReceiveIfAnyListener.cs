using System;

namespace Jiminy.ChannelListeners {
    sealed class ReceiveIfAnyListener<T> : ChannelListenerBase<T>, IChannelListener {
        readonly Guid id = Guid.NewGuid();

        public ReceiveIfAnyListener() {
            error = Error.NoMessages;
            waithandle.Set(); //continue even though no message has been received yet
        }

        public void OnChannelClosed(Guid channelId) {
            error = Error.ChannelClosed;
        }

        public bool OnMessage(Guid channelId, object message) {
            error = Error.Nil;
            this.message = (T)message;
            Discard = true;
            return true;
        }

        public override (T, Error) Result() {
            /* if the result is asked for, then regardless of outcome this will disqualify this strategy in the future,
             * so it is automatically removed when being matched with a message. Otherwise it might stay in the queue
             * until a new message arrives, which will mess up the next valid receiver strategy.
             */
            Discard = true;
            return base.Result();
        }
    }
}
