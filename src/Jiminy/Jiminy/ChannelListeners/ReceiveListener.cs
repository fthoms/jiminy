using System;

namespace Jiminy.ChannelListeners {
    sealed class ReceiveListener<T> : ChannelListenerBase<T>, IChannelListener {
        public void OnChannelClosed(Guid channelId) {
            error = Error.ChannelClosed;
            waithandle.Set();
        }

        public bool OnMessage(Guid channelId, object message) {
            this.message = (T)message;
            waithandle.Set();
            Discard = true;
            return true;
        }
    }
}
