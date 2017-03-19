using System;
using System.Threading.Tasks;

namespace Jiminy {
    public sealed class Timer {
        readonly PubSub<DateTime> timerExpired = new PubSub<DateTime>();

        public Timer(TimeSpan duration) {
            Task.Run(() =>
            {
                Task.Delay(duration).Wait();
                timerExpired.Publish(DateTime.Now);
                timerExpired.Close();
            });
        }

        public static Timer ExecuteAfter(TimeSpan duration, Action onExpiration) {
            var timer = new Timer(duration);
            Task.Run(() =>
            {
                if (timer.Wait().HasValue) {
                    onExpiration();
                }
            });
            return timer;
        }

        public void Stop() {
            timerExpired.Close();
        }

        /// <summary>
        /// Wait for the timer to complete
        /// </summary>
        /// <returns>The current datetime if the timer ran to completion, null if it was stopped prematurely</returns>
        public DateTime? Wait() {
            var chan = timerExpired.Subscribe();
            var (r, err) = chan.Receive();
            if (err != null) {
                return null;
            }
            return r;
        }

        /// <summary>
        /// Start a new timer and return a read-only channel that will receive the current datetime when the timer expires
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static IReceive<DateTime> After(TimeSpan duration) {
            var timer = new Timer(duration);
            return timer.timerExpired.Subscribe();
        }
    }
}
