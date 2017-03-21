using System;
using Jiminy.ChannelListeners;

namespace Jiminy
{
    sealed class DefaultChannel : ChannelBase, IChannelSelectCapabilities {
        readonly Guid id = Guid.NewGuid();
        public override Guid Id => id;

        public void AddListener(IChannelListener listener) {
            listener.OnMessage(id, true);
        }
    }
}
