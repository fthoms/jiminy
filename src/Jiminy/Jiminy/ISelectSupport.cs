using System;

namespace Jiminy {
	interface ISelectSupport<T> {
		Guid Id { get; }
		void Send(T message, Awaitable awaitable);
		void Receive(AwaitableMessage<T> awaitable);
	}
}
