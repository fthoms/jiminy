using Jiminy.ChannelListeners;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Jiminy {
    sealed class Channel<T> : ChannelBase, IChannel<T> {
        readonly Guid id = Guid.NewGuid();
        readonly Queue<SendHandle> unreleasedHandles = new Queue<SendHandle>();
        readonly Queue<SendHandle> releasedHandles = new Queue<SendHandle>();
        readonly Queue<IChannelListener> listeners = new Queue<IChannelListener>();
        readonly int limit;

        public override Guid Id => id;
        public bool IsClosed { get; private set; } = false;
        bool HasMessages => (unreleasedHandles.Count + releasedHandles.Count) > 0;
        bool HasReceivers => listeners.Count > 0;

        public Channel() : this(0) {
        }

        public Channel(int limit) {
            if (limit < 0) {
                throw new ArgumentException($"{limit}: invalid buffer limit");
            }
            this.limit = limit;
        }

        public void Close() {
            lock (this) {
                IsClosed = true;
                ProcessMessages();
            }
        }

        public void Dispose() {
            Close();
        }

        public (T Message, Error Error) Receive() {
            var listener = new ReceiveListener<T>();
            AddListener(listener);
            return listener.Result();
        }

        public (object Message, Error Error) ReceiveIfAny() {
            var listener = new ReceiveIfAnyListener<T>();
            AddListener(listener);
            return listener.Result();
        }

        public void AddListener(IChannelListener listener) {
            lock (this) {
                if (IsClosed && !HasMessages) {
                    listener.OnChannelClosed(Id);
                    return;
                }
                listeners.Enqueue(listener);
                ProcessMessages();
                TrimHandles();
            }
        }

        public IEnumerable<T> Range() {
            var error = Error.Nil;
            T result;
            while (error == null) {
                (result, error) = Receive();
                if (error == null) {
                    yield return result;
                }
            }
        }

        public Error Send(T message) {
            if (IsClosed) {
                return Error.ChannelClosed;
            }
            var handle = new SendHandle(message);
            lock (this) {
                unreleasedHandles.Enqueue(handle);
                TrimHandles();
                ProcessMessages();
            }
            handle.Wait();
            return Error.Nil;
        }

        public Error Send(T message, Action defaultAction) {
            lock (this) {
                if (IsClosed) {
                    return Error.ChannelClosed;
                }
                if (((releasedHandles.Count + unreleasedHandles.Count) - listeners.Count) < limit) {
                    Send(message);
                } else {
                    defaultAction();
                }
                return Error.Nil;
            }
        }

        void ProcessMessages() {
            lock (this) {
                //remove discarded listeners
                while (HasReceivers && listeners.Peek().Discard) {
                    listeners.Dequeue();
                }
                while (HasMessages && HasReceivers) {
                    TrimHandles();
                    var handle = PeekHandle();
                    var handled = false;
                    while(HasReceivers && !handled) {
                        var listener = listeners.Dequeue();
                        handled = listener.OnMessage(Id, handle.message);
                    }
                    if (handled) {
                        DequeueHandle();
                        handle.Release();
                    }
                }
                if (IsClosed && !HasMessages) {
                    while (HasReceivers) {
                        listeners.Dequeue().OnChannelClosed(Id);
                    }
                }
            }
        }

        void TrimHandles() {
            while (unreleasedHandles.Count > 0 && releasedHandles.Count < limit) {
                var handle = unreleasedHandles.Dequeue();
                handle.Release();
                releasedHandles.Enqueue(handle);
            }
        }

        SendHandle PeekHandle() {
            if (releasedHandles.Count > 0) {
                return releasedHandles.Peek();
            }
            return unreleasedHandles.Peek();
        }

        void DequeueHandle() {
            if (releasedHandles.Count > 0) {
                releasedHandles.Dequeue();
            } else {
                unreleasedHandles.Dequeue();
            }
        }

        sealed class SendHandle {
            readonly ManualResetEventSlim wh = new ManualResetEventSlim();
            public readonly T message;
            public SendHandle(T message) {
                this.message = message;
            }
            public void Wait() => wh.Wait();
            public void Release() => wh.Set();
        }
    }
}
