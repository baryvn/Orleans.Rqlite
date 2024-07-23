
# Orleans Rqlite Providers
[Orleans](https://github.com/dotnet/orleans) is a framework that provides a straight-forward approach to building distributed high-scale computing applications, without the need to learn and apply complex concurrency or other scaling patterns. 

[Rqlite](https://www.Rqlite.io/) rqlite is a distributed relational database that combines the simplicity of SQLite with the robustness of a fault-tolerant, highly available cluster. It's developer-friendly, its operation is straightforward, and its design ensures reliability with minimal complexity.


## Why Orleans :sparkling_heart: Rqlite
- Orleans is used to manage application logic, distribute actors to handle user requests, and distribute workload.
- Rqlite is used to store and manage distributed data, ensuring consistency and high reliability of the system.

This setup optimizes performance and scalability for small-scale distributed applications while ensuring data consistency and availability. Such a combination is particularly suitable for applications requiring high reliability and easy scalability, such as IoT applications, multiplayer games, or high-traffic web systems.


## **Orleans.Rqlite** 
is a package that use Rqlite as a backend for Orleans providers like Cluster Membership, Grain State storage and Reminders. 

# Installation 
Nuget Packages are provided:
- Orleans.Persistence.Rqlite
- Orleans.Clustering.Rqlite

## Coming soon
- Orleans.Reminder.Rqlite
- Authen username/password for Rqlite
  
## Silo
```
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
            option.Uri = "http://localhost:4001";
        });
        silo.UseRqliteReminder((RqliteReminderStorageOptions options) =>
        {

        });
        silo.AddRqliteGrainStorage("test", options =>
        {
            options.Uri = "http://localhost:4001";
        });
        silo.ConfigureLogging(logging => logging.AddConsole());

        silo.ConfigureEndpoints(
            siloPort: 11111,
            gatewayPort: 30001
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
```

## Client 
```
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseOrleansClient(client =>
{
    client.UseRqliteClustering(option =>
    {
        option.Uri = "http://localhost:4001";
    });

    client.Configure<ClusterOptions>(options =>
    {
        options.ClusterId = "DEV";
        options.ServiceId = "DEV";

    });
});

```
