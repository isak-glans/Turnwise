using Microsoft.Extensions.DependencyInjection;
using Turnwise.Application.Abstractions;
using Turnwise.Infrastructure.Persistence;
using Turnwise.Infrastructure.Random;

namespace Turnwise.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTurnwiseInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IRandomSource, SystemRandomSource>();
        services.AddSingleton<ICharacterTemplateStore, JsonCharacterTemplateStore>();
        services.AddSingleton<IEncounterStore, JsonEncounterStore>();
        return services;
    }
}
