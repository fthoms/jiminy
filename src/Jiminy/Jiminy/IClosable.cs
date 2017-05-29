using System;

namespace Jiminy {
	public interface IClosable : IDisposable {
		bool IsClosed { get; }
		void Close();
	}
}
