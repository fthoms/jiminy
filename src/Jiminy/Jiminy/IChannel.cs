using Jiminy.ChannelListeners;
using System;
using System.Collections.Generic;

namespace Jiminy {
    public interface IChannel : IDisposable {
        /// <summary>
        /// Gets the id of the channel
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets a value indicating if the channel is closed.
        /// Note: if there are unprocessed messages waiting then it is still possible
        /// to Receive then even if the channel is closed.
        /// </summary>
        bool IsClosed { get; }

        /// <summary>
        /// Closes the channel. Any messages sent before closing can still be read.
        /// </summary>
        void Close();
    }

    public interface ISend<T> : IChannel {
        /// <summary>
        /// Send a message. This operation blocks unless the channel is buffered and there are still slots available.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>An error if the channel is closed, null if the operation was successful</returns>
        Error Send(T message);

        /// <summary>
        /// Non-blocking send of a message. If no receivers are waiting, or if there are no available slots in a buffered channel, then
        /// the default action is executed.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="defaultAction"></param>
        /// <returns>An error if the channel is closed or if the action throws an exception, null if the operation was successful</returns>
        Error Send(T message, Action defaultAction);
    }

    /// <summary>
    /// Marker interface
    /// </summary>
    public interface IReceive : IChannel {
    }

    public interface IReceive<T> : IReceive {
        /// <summary>
        /// Receive a message from the channel. This operation blocks until a message is waiting in the channel.
        /// </summary>
        /// <returns>(message,null) if successful, otherwise (&lt;undefined&gt;,error)</returns>
        (T Message, Error Error) Receive();

        /// <summary>
        /// Gets an enumerable over the messages in the channel. When the channel closes
        /// and there are no more messages to consume, the collection stops.
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> Range();
    }

    public interface IChannel<T> : ISend<T>, IReceive<T>, IChannelSelectCapabilities {
    }

    /// <summary>
    /// Interface with methods needed by channel select
    /// </summary>
    public interface IChannelSelectCapabilities {
        void AddListener(IChannelListener listener);
        (object Message, Error Error) ReceiveIfAny();
    }
}
