using System;

namespace Jiminy.Examples {
    sealed class ChannelDirections : IExample {
        public void Run() {
            var pings = Channel.Make<string>(1);
            var pongs = Channel.Make<string>(1);
            Ping(pings, "passed message");
            Pong(pings, pongs);
            Console.WriteLine(pongs.Receive().Message);
        }

        void Ping(ISend<string> pings, string msg) {
            pings.Send(msg);
        }

        void Pong(IReceive<string> pings, ISend<string> pongs) {
            var (msg,_) = pings.Receive();
            pongs.Send(msg);
        }
    }
}
