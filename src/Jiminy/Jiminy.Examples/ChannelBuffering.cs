using System;

namespace Jiminy.Examples {
    sealed class ChannelBuffering : IExample {
        public void Run() {
            var messages = Channel.Make<string>(2);

            messages.Send("buffered");
            messages.Send("channel");

            messages.Close();

            Console.WriteLine(messages.Receive().Message);
            Console.WriteLine(messages.Receive().Message);
        }
    }
}
