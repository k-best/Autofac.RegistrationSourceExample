using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Features.Indexed;
using FluentAssertions;
using Moq;
using Xunit;

namespace RegistrationSourceExample.Test
{
    public class CheckHandlerTransform
    {
        [Fact]
        public void ResolveInnerHandlerShouldReturnWrapperOverOuterHandler()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Handler1>().As<IPXMessageHandler<Message1>>();
            builder.RegisterType<HandlerFactory>().As<IHandlerFactory>();
            builder.RegisterSource(new HandlerRegistrationSource());
            var container = builder.Build();
            var messageHandler = container.Resolve<IHandleMessages<Message1>>();
            messageHandler.GetType().Should().Be<PXMessageHandler<Message1>>();
        }

        [Fact]
        public void ResolveManyHandlersForSameMessageTypeShouldReturnProperNumberOfWrapperClasses()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Handler1>().As<IPXMessageHandler<Message1>>();
            builder.RegisterType<Handler2>().As<IPXMessageHandler<Message1>>();
            builder.RegisterType<HandlerFactory>().As<IHandlerFactory>();
            builder.RegisterSource(new HandlerRegistrationSource());
            var container = builder.Build();
            var outerHandlers = container.Resolve<IEnumerable<IPXMessageHandler<Message1>>>().ToArray();
            var messageHandlers = container.Resolve<IEnumerable<IHandleMessages<Message1>>>().ToArray();
            outerHandlers.Count().Should().Be(messageHandlers.Count());
            foreach (var messageHandler in messageHandlers)
            {
                messageHandler.GetType().Should().Be<PXMessageHandler<Message1>>();
            }
        }

        [Fact]
        public void ResolvedWrapperShouldDelegateHandleToRegisteredService()
        {
            var builder = new ContainerBuilder();
            var handlerMock = new Moq.Mock<IPXMessageHandler<Message1>>();
            handlerMock.Setup(c => c.Handle(It.IsAny<Message1>()));
            builder.RegisterInstance(handlerMock.Object).As<IPXMessageHandler<Message1>>();
            builder.RegisterType<HandlerFactory>().As<IHandlerFactory>();
            builder.RegisterSource(new HandlerRegistrationSource());
            var container = builder.Build();
            var messageHandlers = container.Resolve<IEnumerable<IHandleMessages<Message1>>>().ToArray();
            foreach (var handler in messageHandlers)
            {
                handler.Handle(new Message1());
            }
            handlerMock.Verify(m=>m.Handle(It.IsAny<Message1>()), Times.Once);
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

    public class Handler2 : IPXMessageHandler<Message1>
    {
        public Task Handle(Message1 message)
        {
            Console.WriteLine(message.Subj + "2");
            return Task.CompletedTask;
        }
    }
}
