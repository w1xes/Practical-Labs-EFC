using TaskEFC.Data;

namespace TaskEFC;

public class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        // The EF Core tools also run Main; they stop it inside Build(), so seeding is not triggered.
        SeedDatabase(host);

        host.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());

    private static void SeedDatabase(IHost host)
    {
        // The context is a scoped service, so it is resolved from a dedicated scope.
        using var scope = host.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        try
        {
            SampleData.Initialize(serviceProvider);
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while creating and seeding the database.");
            throw;
        }
    }
}
