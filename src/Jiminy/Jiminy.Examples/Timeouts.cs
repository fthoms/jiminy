using System;
using System.Threading.Tasks;

namespace Jiminy.Examples {
    sealed class Timeouts : IExample {
        public void Run() {
            var c1 = Channel.Make<string>(1);
            Task.Run(() =>
            {
                Task.Delay(TimeSpan.FromSeconds(2));
                c1.Send("result 1");
            });

            Channel.Select()
                .Case(c1, m => Console.WriteLine(m))
                .Case(Timer.After(TimeSpan.FromSeconds(1)), _ => Console.WriteLine("timeout 1"))
                .Receive();

            var c2 = Channel.Make<string>(1);
            Task.Run(() =>
            {
                Task.Delay(TimeSpan.FromSeconds(2));
                c2.Send("result 2");
            });

            Channel.Select()
                .Case(c1, m => Console.WriteLine(m))
                .Case(Timer.After(TimeSpan.FromSeconds(3)), _ => Console.WriteLine("timeout 2"))
                .Receive();
        }
    }
}
