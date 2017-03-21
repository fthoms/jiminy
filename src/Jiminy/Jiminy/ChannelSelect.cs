using Jiminy.ChannelListeners;
using System;
using System.Collections.Generic;

namespace Jiminy {
    public interface IChannelSelector {
        /// <summary>
        /// Add a case to the select. This will invoke the onMessage action if and when a message arrives in the channel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="onMessage"></param>
        /// <returns></returns>
        IChannelSelector Case<T>(IReceive<T> channel, Action<T> onMessage);

        /// <summary>
        /// Block and wait until a message arrives on one of the channels, or all channels are closed.
        /// </summary>
        /// <returns>An error if all channels are closed, otherwise null</returns>
        Error Receive();

        /// <summary>
        /// Read a message from one of the channels. If none can be read, the default action is executed.
        /// </summary>
        /// <param name="defaultAction"></param>
        /// <returns>An error if all channels are closed or the default action throws an exception, otherwise null</returns>
        Error Otherwise(Action defaultAction);
    }

    sealed class ChannelSelect : IChannelSelector {
        readonly HashSet<IReceive> channels = new HashSet<IReceive>();
        readonly Dictionary<Guid, Action<object>> messageHandlersByChannelId = new Dictionary<Guid, Action<object>>();

        internal ChannelSelect() {
        }

        public IChannelSelector Case<T>(IReceive<T> channel, Action<T> onMessage) {
            channels.Add(channel);
            messageHandlersByChannelId.Add(channel.Id, m => onMessage((T)m));
            return this;
        }

        public Error Receive() {
            var listener = new MultiChannelReceiveListener();
            HookupListenerToChannels(listener);
            return listener.Success();
        }

        void HookupListenerToChannels(MultiChannelReceiveListener listener) {
            foreach (var kvp in messageHandlersByChannelId) {
                listener.AddMessageHandler(kvp.Key, kvp.Value);
            }
            foreach (var chan in channels) {
                (chan as IChannelSelectCapabilities).AddListener(listener);
            }
        }

        public Error Otherwise(Action defaultAction) {
            var defaultChannel = new DefaultChannel();
            messageHandlersByChannelId.Add(defaultChannel.Id, _ => defaultAction());
            var listener = new MultiChannelReceiveListener();
            HookupListenerToChannels(listener);
            defaultChannel.AddListener(listener);
            return listener.Success();
        }
    }
}
