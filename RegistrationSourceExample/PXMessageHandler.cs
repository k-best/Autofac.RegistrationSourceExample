using System.Threading.Tasks;

namespace RegistrationSourceExample
{
    public class PXMessageHandler<T> : IHandleMessages<T>
    {
        private readonly IPXMessageHandler<T> _outerHandler;

        public PXMessageHandler(IPXMessageHandler<T> outerHandler)
        {
            _outerHandler = outerHandler;
        }

        public async Task Handle(T message)
        {
            await _outerHandler.Handle(message);
        }
    }

    public interface IHandlerFactory
    {
        IHandleMessages<T> GetHandler<T>(IPXMessageHandler<T> outerHandler);
    }

    public class HandlerFactory : IHandlerFactory
    {
        public IHandleMessages<T> GetHandler<T>(IPXMessageHandler<T> outerHandler)
        {
            return new PXMessageHandler<T>(outerHandler);
        }
    }
}