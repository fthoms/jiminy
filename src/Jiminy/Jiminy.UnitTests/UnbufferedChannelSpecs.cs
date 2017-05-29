using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Jiminy.UnitTests {
	public class UnbufferedChannelSpecs {
		[Fact(DisplayName = "When sending a message it is received")]
		void foo() {
			var cut = Channel.Make<int>();
			var expectedMessage = 124;
			var sndMsg = new WaitGroup(1);
			Task.Run(() => {
				cut.Send(expectedMessage).ShouldBeTrue();
				sndMsg.Done();
			});
			sndMsg.Wait(1000.milliseconds()).ShouldBeTrue("Send timed out");
			var (msg, ok) = cut.Receive();
			ok.ShouldBeTrue();
			msg.ShouldBe(expectedMessage);
		}
	}
}
