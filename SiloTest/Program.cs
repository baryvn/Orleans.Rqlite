using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Clustering.Rqlite;
using Orleans.Persistence.Rqlite.Hosting;
using Orleans.Reminders.Rqlite;

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .UseOrleans(silo =>
    {
        silo.UseRqliteMembership((RqliteClusteringOptions options) =>
        {

        });
        silo.UseRqliteReminder((RqliteReminderStorageOptions options) =>
        {

        });
        silo.AddRqliteGrainStorage("", options =>
        {

        });
        silo.ConfigureLogging(logging => logging.AddConsole());
    })
    .UseConsoleLifetime();

using IHost host = builder.Build();

await host.RunAsync();