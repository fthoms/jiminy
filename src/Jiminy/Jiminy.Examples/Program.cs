namespace Jiminy.Examples {
    class Program {
        static void Main(string[] args) {
            //var example = new SendReceive();
            //var example = new SimpleSendReceive();
            //var example = new ChannelBuffering();
            //var example = new Select();
            //var example = new ChannelDirections();
            //var example = new PublishSubscribe();
            //var example = new Timers();
            //var example = new Timeouts();
            //var example = new FanOutFanIn();
            //var example = new Tickers();
            //var example = new ChannelSynchronisation();
            //var example = new ChannelRange();
            //var example = new ClosingAChannel();
            //var example = new ChannelDefaultSelect();
            var example = new RateLimiting();
            example.Run();
        }
    }
}
