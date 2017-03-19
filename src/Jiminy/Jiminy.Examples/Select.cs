using System;
using System.Threading.Tasks;

namespace Jiminy.Examples {
    /// <summary>
    /// This example demonstrates receiving from multiple channels at once
    /// </summary>
    sealed class Select : IExample {
        public void Run() {
            var done = Channel.Make<bool>();
            Task.Run(() => Receive(MakeChan("first"), MakeChan("second"), MakeChan("third"), done));
            done.Receive();
            Console.WriteLine("done");
        }

        IChannel<string> MakeChan(string name) {
            var chan = Channel.Make<string>();
            Task.Run(() =>
            {
                for(var i = 0; i < 5; i++) {
                    chan.Send($"{name} {i}");
                }
                chan.Close();
            });
            return chan;
        }

        void Receive(IChannel<string> ch1, IChannel<string> ch2, IChannel<string> ch3, IChannel<bool> done) {
            var error = Error.Nil;
            while (error == null) {
                error = Channel.Select()
                    .Case(ch1, m => Console.WriteLine(m))
                    .Case(ch2, m => Console.WriteLine(m))
                    .Case(ch3, m => Console.WriteLine(m))
                    .Receive();
            }
            done.Send(true);
        }
    }
}
