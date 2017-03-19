using System;

namespace Jiminy.ChannelListeners {
    public interface IChannelListener {
        /// <summary>
        /// Handle a message and notify caller of the success
        /// </summary>
        /// <param name="channelId">Id of the channel</param>
        /// <param name="message">The message</param>
        /// <returns>True of the message was handled by this strategy, otherwise false</returns>
        bool OnMessage(Guid channelId, object message);
        
        /// <summary>
        /// Notify strategy that the channel was closed
        /// </summary>
        /// <param name="channelId"></param>
        void OnChannelClosed(Guid channelId);
        
        /// <summary>
        /// Get a value indicating if this listener has previously handled a message
        /// </summary>
        bool Discard { get; }
    }
}
