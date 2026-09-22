using Todo.Api;
using Todo.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Todo")
    ?? throw new InvalidOperationException("Connection string 'Todo' is not configured.");

builder.Services.AddTodoInfrastructure(connectionString);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<UnhandledExceptionHandler>();
builder.Services.AddOpenApi();

var app = builder.Build();

await app.Services.GetRequiredService<TodoDatabase>().EnsureCreatedAsync(CancellationToken.None);

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapTodoEndpoints();

app.Run();

public partial class Program;
