using PharmaceuticalManagerBot;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using PharmaceuticalManagerBot.Data;
using Microsoft.Extensions.Configuration;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        var secret = builder.Configuration;
        builder.Services.AddHostedService<PharmaceuticalManagerBotWorker>();
        builder.Services.AddDbContext<PharmaceuticalManagerBotContext>(options => options.UseNpgsql(secret["PostgresDatabase"]));
        builder.Services.AddSingleton<IUserStateTracker, UserStateTracker>();
        builder.Services.AddHostedService<ExpiryNotificationService>();

        var host = builder.Build();

        var scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();
        using (var scope = scopeFactory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PharmaceuticalManagerBotContext>();
            if (db.Database.EnsureCreated()) { }
        }


        await host.RunAsync();
    }
}