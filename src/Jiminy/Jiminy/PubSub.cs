using System;
using System.Collections.Generic;

namespace Jiminy {
    public sealed class PubSub<T> : IDisposable {
        readonly List<IChannel<T>> subscriberChannels = new List<IChannel<T>>();
        bool isOpen = true;

        public Error Publish(T message) {
            if (!isOpen) {
                return "Publisher closed".AsError();
            }
            lock (subscriberChannels) {
                foreach (var chan in subscriberChannels) {
                    var err = chan.Send(message);
                    if (err != null) {
                        return err;
                    }
                }
            }
            return Error.Nil;
        }

        public IReceive<T> Subscribe() {
            lock (subscriberChannels) {
                var channel = Channel.Make<T>();
                subscriberChannels.Add(channel);
                if (!isOpen) {
                    channel.Close();
                }
                return channel;
            }
        }

        public void Close() {
            isOpen = false;
            lock (subscriberChannels) {
                foreach (var chan in subscriberChannels) {
                    chan.Close();
                }
            }
        }

        public void Dispose() {
            Close();
        }
    }
}
