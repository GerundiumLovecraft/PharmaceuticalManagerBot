using PharmaceuticalManagerBot;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using PharmaceuticalManagerBot.Data;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using PharmaceuticalManagerBot.Methods;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        var secret = builder.Configuration;
        builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(secret["TelegramApiKey"]));
        builder.Services.AddSingleton<IUserStateTracker, UserStateTracker>();
        builder.Services.AddScoped<DbMethods>();
        builder.Services.AddHostedService<ExpiryNotificationService>();
        builder.Services.AddHostedService<PharmaceuticalManagerBotWorker>();
        builder.Services.AddDbContext<PharmaceuticalManagerBotContext>(options => options.UseNpgsql(secret["PostgresDatabase"]));
        

        var host = builder.Build();

        IServiceScopeFactory scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();
        using (IServiceScope scope = scopeFactory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PharmaceuticalManagerBotContext>();
            //If the database is created, the medication types will be seeded to the db
            if (db.Database.EnsureCreated()) { SeedMedTypes.Initialise(db); }
        }


        await host.RunAsync();
    }
}