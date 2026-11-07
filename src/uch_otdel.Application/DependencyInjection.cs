using Microsoft.Extensions.DependencyInjection;
using uch_otdel.Application.Events;
using uch_otdel.Application.Plans;
using uch_otdel.Application.Workload;
using uch_otdel.Domain.Workload;

namespace uch_otdel.Application;

public static class DependencyInjection
{
    /// <summary>Регистрирует сценарии. Реализацию доступа к данным регистрирует Infrastructure.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services, WorkloadNorms norms)
    {
        services.AddSingleton(norms);
        services.AddScoped<WorkloadService>();
        services.AddScoped<PlansService>();

        // Общие для всех вкладок: сервер приложения один, события рассылаются внутри процесса
        services.AddSingleton<IAppEventBus, InProcessAppEventBus>();
        services.AddSingleton<CalculationService>();
        return services;
    }
}
