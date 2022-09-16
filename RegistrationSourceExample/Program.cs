using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Autofac;

namespace RegistrationSourceExample
{
    public class Program
    {
        private static IContainer Container { get; set; }

        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Handler1>().As<IPXMessageHandler<Message1>>();
            builder.RegisterType<HandlerFactory>().As<IHandlerFactory>();
            builder.RegisterSource(new HandlerRegistrationSource());
            Container = builder.Build();
            var messageHandler = Container.Resolve<IHandleMessages<Message1>>();
            messageHandler.Handle(new Message1() { Subj = "tratata" });
        }
    }

    public class Message1
    {
        public string Subj { get; set; }
    }

    public class Handler1 : IPXMessageHandler<Message1>
    {
        public Task Handle(Message1 message)
        {
            Console.WriteLine(message.Subj);
            return Task.CompletedTask;
        }
    }
}
