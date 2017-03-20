using System;
using System.Threading.Tasks;

namespace Jiminy {
    public sealed class Ticker {
        readonly IChannel<DateTime> publicChannel;
        readonly TimeSpan interval;
        Timer timer;

        public IReceive<DateTime> Channel => publicChannel;

        public Ticker(TimeSpan interval) : this(interval, interval) {
        }

        public Ticker(TimeSpan delay, TimeSpan interval) {
            publicChannel = Jiminy.Channel.Make<DateTime>(100);
            this.interval = interval;
            StartTimer(delay);
        }

        public void Stop() {
            timer?.Stop();
        }

        void StartTimer(TimeSpan intrvl) {
            Task.Run(() =>
            {
                timer = new Timer(intrvl);
                var expired = timer.Wait();
                if (expired.HasValue) {
                    publicChannel.Send(expired.Value);
                    StartTimer(interval);
                } else {
                    publicChannel.Close();
                }
            });
        }
    }
}
