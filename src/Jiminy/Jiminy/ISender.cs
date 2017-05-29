namespace Jiminy {
	public interface ISender<T> : IClosable {
		bool Send(T message);
	}
}
