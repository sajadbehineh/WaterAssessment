using Microsoft.Extensions.DependencyInjection;

namespace WaterAssessment.Helpers
{
    public class ServiceProvider : IServiceProvider
    {
        private readonly IServiceCollection _services;

        public ServiceProvider(IServiceCollection services)
        {
            _services = services;
        }

        public object GetService(Type serviceType)
        {
            var descriptor = _services.FirstOrDefault(d => d.ServiceType == serviceType);

            if (descriptor == null)
            {
                throw new InvalidOperationException($"Service of type {serviceType} not registered.");
            }

            if (descriptor.ImplementationInstance != null)
            {
                return descriptor.ImplementationInstance;
            }

            if (descriptor.ImplementationFactory != null)
            {
                return descriptor.ImplementationFactory(this);
            }

            if (descriptor.ImplementationType != null)
            {
                var constructor = descriptor.ImplementationType.GetConstructors().First();
                var parameters = constructor.GetParameters().Select(p => GetService(p.ParameterType)).ToArray();
                return Activator.CreateInstance(descriptor.ImplementationType, parameters);
            }

            throw new InvalidOperationException($"Invalid service descriptor for type {serviceType}.");
        }
    }
}
