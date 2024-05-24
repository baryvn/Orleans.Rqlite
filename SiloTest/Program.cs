using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Clustering.Rqlite;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Persistence.Rqlite.Hosting;
using Orleans.Reminders.Rqlite;
using System.Net;

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .UseOrleans(silo =>
    {
        silo.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "DEV";
            options.ServiceId = "DEV";

        });
        silo.UseRqliteClustering(option =>
        {
            option.Uri = "http://192.168.68.41:4001";
        });
        silo.UseRqliteReminder((RqliteReminderStorageOptions options) =>
        {

        });
        silo.AddRqliteGrainStorage("test", options =>
        {
            options.Uri = "http://192.168.68.41:4001";
        });
        silo.ConfigureLogging(logging => logging.AddConsole());

        silo.ConfigureEndpoints(
            siloPort: 11111,
            gatewayPort: 30001,
            advertisedIP: IPAddress.Parse("192.168.68.44"),
            listenOnAnyHostAddress: true
            );

        silo.Configure<ClusterMembershipOptions>(options =>
        {
            options.EnableIndirectProbes = true;
            options.UseLivenessGossip = true;
        });
    })
    .UseConsoleLifetime();

using IHost host = builder.Build();

await host.RunAsync();