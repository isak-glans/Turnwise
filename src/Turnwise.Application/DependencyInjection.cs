using Microsoft.Extensions.DependencyInjection;
using Turnwise.Application.Dice;
using Turnwise.Application.Services;

namespace Turnwise.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTurnwiseApplication(this IServiceCollection services)
    {
        services.AddScoped<DiceRollingService>();
        services.AddScoped<EncounterService>();
        services.AddScoped<CharacterTemplateService>();
        services.AddScoped<EncounterFileService>();
        return services;
    }
}
