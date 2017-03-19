using System;
using System.Threading.Tasks;

namespace Jiminy.Examples {
    sealed class SimpleSendReceive : IExample {
        public void Run() {
            var messages = Channel.Make<string>();

            Task.Run(() => messages.Send("ping"));

            var (msg, _) = messages.Receive();

            Console.WriteLine(msg);
        }
    }
}
