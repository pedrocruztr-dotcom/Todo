using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Todo.Api.Tests;

// Levanta a API a sério em memória, com base de dados própria para não mexer na de trabalho.
public sealed class TodoApiFactory : WebApplicationFactory<Program>
{
    private const string ConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=Todo_ApiTests;Integrated Security=true;TrustServerCertificate=true;Application Name=Todo.Api.Tests";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(configuration =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:carrodopingodoce"] = ConnectionString
            }));
    }
}
