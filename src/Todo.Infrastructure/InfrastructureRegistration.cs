using Microsoft.Extensions.DependencyInjection;
using Todo.Application;

namespace Todo.Infrastructure;

public static class InfrastructureRegistration
{
    public static IServiceCollection AddTodoInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Uma vez ao arrancar. O registo do Dapper é global ao processo.
        DapperTypeHandlers.Register();

        // Construção: nascem uma vez e vivem enquanto a app viver.
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(new TodoDatabase(connectionString));

        // Pedido: nascem e morrem a cada pedido, que é onde se toca na base de dados.
        services.AddScoped<ITodoRepository, SqlTodoRepository>();
        services.AddScoped<TodoService>();

        return services;
    }
}
