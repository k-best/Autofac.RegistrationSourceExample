using System.Threading.Tasks;

namespace RegistrationSourceExample
{
    public interface IHandleMessages<T>
    {
        Task Handle(T message);
    }
}