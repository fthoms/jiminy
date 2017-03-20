using System.Threading.Tasks;

namespace Jiminy {
    public static class Channel {
        /// <summary>
        /// Create an unbuffered channel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IChannel<T> Make<T>() {
            return new Channel<T>();
        }

        /// <summary>
        /// Create a buffered channel. If limit is 0, an unbuffered channel is created instead.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static IChannel<T> Make<T>(int limit) {
            if (limit == 0) {
                return Make<T>();
            }
            return new Channel<T>(limit);
        }

        /// <summary>
        /// Start a new multi-channel read
        /// </summary>
        /// <returns></returns>
        public static IChannelSelector Select() => new ChannelSelect();

        /// <summary>
        /// Merge multiple channels into one read-only channel, enabling fan-in.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sources"></param>
        /// <returns></returns>
        public static IReceive<T> Merge<T>(params IReceive<T>[] sources) {
            var output = Make<T>();
            Task.Run(() =>
            {
                var loop = true;
                while (loop) {
                    var select = Select();
                    foreach(var chan in sources) {
                        select.Case(chan, m => output.Send(m));
                    }
                    var err = select.Receive();
                    loop = err == null;
                }
                output.Close();
            });
            return output;
        }
    }
}
