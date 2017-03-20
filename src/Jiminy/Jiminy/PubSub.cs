using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jiminy {
    public sealed class PubSub<T> : IDisposable {
        readonly List<IChannel<T>> subscriberChannels = new List<IChannel<T>>();
        readonly IChannel<T> publisherChannel;
        volatile bool isOpen = true;
        public ISend<T> Channel => publisherChannel;

        public PubSub() : this(0) {
        }

        public PubSub(int buffer) {
            publisherChannel = buffer == 0 ? Jiminy.Channel.Make<T>() : Jiminy.Channel.Make<T>(buffer);
            Task.Run(() =>
            {
                var loop = isOpen;
                while (loop) {
                    var (msg, err) = publisherChannel.Receive();
                    if (err == null) {
                        lock (subscriberChannels) {
                            foreach (var chan in subscriberChannels) {
                                chan.Send(msg);
                            }
                        }
                    }
                    loop = isOpen && err == null;
                }
                lock (subscriberChannels) {
                    foreach (var chan in subscriberChannels) {
                        chan.Close();
                    }
                }
            });
        }

        public Error Publish(T message) {
            if (!isOpen) {
                return "Publisher closed".AsError();
            }
            return publisherChannel.Send(message);
        }

        public IReceive<T> Subscribe() {
            lock (subscriberChannels) {
                var channel = Jiminy.Channel.Make<T>();
                subscriberChannels.Add(channel);
                if (!isOpen) {
                    channel.Close();
                }
                return channel;
            }
        }

        public void Close() {
            isOpen = false;
            publisherChannel.Close();
        }

        public void Dispose() {
            Close();
        }
    }
}
