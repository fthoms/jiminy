namespace Jiminy {
	public interface IReceiver<T> : IClosable {
		(T Message, bool Ok) Receive();
	}
}
