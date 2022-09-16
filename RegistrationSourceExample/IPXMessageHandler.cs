using System.Threading.Tasks;

namespace RegistrationSourceExample
{
    public interface IPXMessageHandler<T>
    {
        Task Handle(T message);
    }
}