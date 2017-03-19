using System;

namespace Jiminy.Examples {
    sealed class Timers : IExample {
        public void Run() {
            for (var i = 0; i < 5; i++) {
                var timer = new Timer(TimeSpan.FromMilliseconds(500));
                var now = timer.Wait();
                if (now.HasValue) {
                    Console.WriteLine($"{now.Value.TimeOfDay} : timer completed");
                } else {
                    Console.WriteLine("Timer was stopped");
                }
            }
            Console.Write("\nExecuting a function after 2 seconds... ");

            var done = Channel.Make<bool>();
            var t = Timer.ExecuteAfter(TimeSpan.FromSeconds(2), () =>
            {
                Console.WriteLine("there we go");
                done.Send(true);
            });
            done.Receive();
        }
    }
}
