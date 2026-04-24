using GeoToolkit.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GeoToolkit;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGeoToolkit(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<IGeoUnionService>()
            .AddClasses(c => c.AssignableTo<IScopedService>())
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo<ISingletonService>())
                .AsImplementedInterfaces()
                .WithSingletonLifetime());

        return services;
    }
}
