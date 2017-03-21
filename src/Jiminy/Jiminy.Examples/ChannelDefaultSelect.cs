using System;

namespace Jiminy.Examples {
    sealed class ChannelDefaultSelect : IExample {
        public void Run() {
            var chan1 = Channel.Make<int>();
            var chan2 = Channel.Make<int>();
            var err = Channel.Select()
                .Case(chan1, _ => Console.WriteLine("channel 1"))
                .Case(chan2, _ => Console.WriteLine("channel 1"))
                .Otherwise(() => Console.WriteLine("no messages"));
        }
    }
}
