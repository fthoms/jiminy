using System;
using System.Threading.Tasks;

namespace Jiminy.Examples {
    sealed class Tickers : IExample {
        public void Run() {
            var done = new WaitGroup(1);
            var ticker = new Ticker(TimeSpan.FromMilliseconds(500));
            Task.Run(() =>
            {
                foreach (var m in ticker.Chan.Range()) {
                    Console.WriteLine($"Tick at {m.TimeOfDay}");
                }
                done.Done();
            });
            Task.Delay(1600).Wait();
            ticker.Stop();
            done.Wait();
        }
    }
}
