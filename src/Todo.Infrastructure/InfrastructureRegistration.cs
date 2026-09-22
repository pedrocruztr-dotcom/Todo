using Microsoft.Extensions.DependencyInjection;
using Todo.Application;

namespace Todo.Infrastructure;

public static class InfrastructureRegistration
{
    public static IServiceCollection AddTodoInfrastructure(this IServiceCollection services, string connectionString)
    {
        DapperTypeHandlers.Register();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(new TodoDatabase(connectionString));
        services.AddScoped<ITodoRepository, SqlTodoRepository>();
        services.AddScoped<TodoService>();

        return services;
    }
}
