using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;

namespace RegistrationSourceExample
{
    public class HandlerRegistrationSource : IRegistrationSource
    {
        private static readonly MethodInfo CreateWrapperRegistrationMethod = typeof(HandlerRegistrationSource).GetDeclaredMethod(nameof(CreateWrapperHandler));

        public IEnumerable<IComponentRegistration> RegistrationsFor(
            Service service,
            Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
        {
            var swt = service as IServiceWithType;
            if(swt == null || !swt.ServiceType.IsGenericType || typeof(IHandleMessages<>) != swt.ServiceType.GetGenericTypeDefinition())
            {
                return Enumerable.Empty<IComponentRegistration>();
            }

            var messageType = swt.ServiceType.GetGenericArguments()[0];
            var outerTypeService = swt.ChangeType(typeof(IPXMessageHandler<>).MakeGenericType(messageType));
            var outerHandlers = registrationAccessor(outerTypeService);
            if(outerHandlers == null)
                return Enumerable.Empty<IComponentRegistration>();
            return outerHandlers.Select(outerRegistration => RegistrationBuilder.ForDelegate(swt.ServiceType, (c, p) =>
                    {
                        var outerHandler = c.ResolveComponent(new ResolveRequest(outerTypeService, outerRegistration, p, null));

                        var method = CreateWrapperRegistrationMethod.MakeGenericMethod(messageType);
                        return method.Invoke(null, new[] { outerHandler });
                    })
                    .As(service)
                    .Targeting(outerRegistration.Registration)
                    .InheritRegistrationOrderFrom(outerRegistration.Registration))
                .Select(rb => rb.CreateRegistration())
                .ToList();
        }

        public bool IsAdapterForIndividualComponents { get{ return false; } }

        private static IHandleMessages<T> CreateWrapperHandler<T>(IPXMessageHandler<T> outerHandler)
        {
            return new PXMessageHandler<T>(outerHandler);
        }
    }

    internal static class RegistrationSourceExtensions
    {
        internal static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> InheritRegistrationOrderFrom<TLimit, TActivatorData, TSingleRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration,
            IComponentRegistration source)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            var sourceRegistrationOrder = source.GetRegistrationOrder();
            registration.RegistrationData.Metadata["__RegistrationOrder"] = sourceRegistrationOrder;

            return registration;
        }

        internal static long GetRegistrationOrder(this IComponentRegistration registration)
        {
            return registration.Metadata.TryGetValue("__RegistrationOrder", out object? value) ? (long)value! : long.MaxValue;
        }
    }
}