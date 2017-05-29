using System;
using System.Collections.Generic;

namespace Jiminy {
	sealed class Channel<T> : IChannel<T>, ISelectSupport<T> {
		readonly Queue<T> bufferedMessages;
		readonly int bufferSize;
		readonly object sync = new object();
		readonly Queue<Awaitable> blockedSenders;
		readonly Queue<AwaitableMessage<T>> blockedReceivers;

		internal Channel() : this(0) { }
		internal Channel(int bufferSize) {
			if (bufferSize < 0) throw new ArgumentException($"{bufferSize}: buffer size must be 0 or greater");
			this.bufferSize = bufferSize;
			bufferedMessages = new Queue<T>();
			blockedReceivers = new Queue<AwaitableMessage<T>>();
			blockedSenders = new Queue<Awaitable>();
		}

		public Guid Id { get; } = Guid.NewGuid();
		public bool IsClosed { get; private set; } = false;

		public void Close() {
			lock (sync) {
				IsClosed = true;
			}
		}

		public void Dispose() {
			Close();
		}

		public (T Message, bool Ok) Receive() {
			var messageReady = new AwaitableMessage<T>();
			Receive(messageReady);
			messageReady.Wait();
			return (messageReady.Message, true);
		}

		public void Receive(AwaitableMessage<T> awaitable) {
			lock (sync) {
				blockedReceivers.Enqueue(awaitable);
				if (bufferedMessages.Count > 0) {
					blockedReceivers.Dequeue().Release(Id, bufferedMessages.Dequeue());
					if (blockedSenders.Count > 0) {
						blockedSenders.Dequeue().Release(Id);
					}
				}
			}
		}

		public bool Send(T message) {
			var messageSent = new Awaitable();
			Send(message, messageSent);
			messageSent.Wait();
			return true;
		}

		public void Send(T message, Awaitable awaitable) {
			lock (sync) {
				bufferedMessages.Enqueue(message);
				if (bufferedMessages.Count > bufferSize) {
					blockedSenders.Enqueue(awaitable);
				} else {
					awaitable.Release(Id);
				}
				//drain used receivers
				while (blockedReceivers.Count > 0 && blockedReceivers.Peek().IsReleased) {
					blockedReceivers.Dequeue();
				}
				if (blockedReceivers.Count > 0) {
					blockedReceivers.Dequeue().Release(Id, bufferedMessages.Dequeue());
				}
			}
		}
	}

	public static class Channel {
		/// <summary>
		/// Create an unbuffered channel
		/// </summary>
		/// <typeparam name="T">The message type</typeparam>
		/// <returns></returns>
		public static IChannel<T> Make<T>() => new Channel<T>();

		/// <summary>
		/// Create a buffered channel
		/// </summary>
		/// <typeparam name="T">The message type</typeparam>
		/// <param name="bufferSize">The buffer size</param>
		/// <returns></returns>
		public static IChannel<T> Make<T>(int bufferSize) => new Channel<T>(bufferSize);
	}
}
