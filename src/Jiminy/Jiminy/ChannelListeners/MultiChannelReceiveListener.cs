using System;
using System.Collections.Generic;
using System.Threading;

namespace Jiminy.ChannelListeners {
    sealed class MultiChannelReceiveListener : IChannelListener {
        readonly ManualResetEventSlim waithandle = new ManualResetEventSlim(false);
        readonly Dictionary<Guid, Action<object>> messageHandlersByChannelId = new Dictionary<Guid, Action<object>>();
        Error error = Error.Nil;
        bool alreadyHandledMessage = false;

        public bool Discard => alreadyHandledMessage;

        public void AddMessageHandler(Guid channelId, Action<object> onMessage) {
            lock (this) {
                messageHandlersByChannelId.Add(channelId, onMessage);
            }
        }

        public Error Success() {
            waithandle.Wait();
            return error;
        }

        public void OnChannelClosed(Guid channelId) {
            lock (this) {
                messageHandlersByChannelId.Remove(channelId);
                if (messageHandlersByChannelId.Count == 0) {
                    error = Error.ChannelClosed;
                    alreadyHandledMessage = true;
                    waithandle.Set();
                }
            }
        }

        public bool OnMessage(Guid channelId, object message) {
            lock (this) {
                if (alreadyHandledMessage) {
                    return false;
                }
                alreadyHandledMessage = true;
                try {
                    messageHandlersByChannelId[channelId](message);
                } catch (Exception ex) {
                    error = ex.AsError();
                }
                waithandle.Set();
                return true;
            }
        }
    }
}
