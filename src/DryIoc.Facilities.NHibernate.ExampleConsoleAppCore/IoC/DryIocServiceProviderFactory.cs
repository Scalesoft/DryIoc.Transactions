using System;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DryIoc.Facilities.NHibernate.ExampleConsoleApp.IoC
{
    internal class DryIocServiceProviderFactory<TContainer> : IServiceProviderFactory<IContainer> where TContainer : IContainer, new()
    {
        public IContainer CreateBuilder(
            IServiceCollection services
        ) => new TContainer().WithDependencyInjectionAdapter(services);

        public IServiceProvider CreateServiceProvider(
            IContainer containerBuilder
        ) => containerBuilder.ConfigureServiceProvider<TContainer>();
    }
}
