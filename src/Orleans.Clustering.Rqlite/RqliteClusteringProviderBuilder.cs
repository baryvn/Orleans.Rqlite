using System;
using Orleans.Providers;
using Microsoft.Extensions.Configuration;
using Orleans;
using Orleans.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: RegisterProvider("Rqlite", "Clustering", "Client", typeof(RqliteClusteringProviderBuilder))]
[assembly: RegisterProvider("Rqlite", "Clustering", "Silo", typeof(RqliteClusteringProviderBuilder))]

namespace Orleans.Hosting;

internal sealed class RqliteClusteringProviderBuilder : IProviderBuilder<ISiloBuilder>, IProviderBuilder<IClientBuilder>
{
    public void Configure(ISiloBuilder builder, string name, IConfigurationSection configurationSection)
    {
        builder.UseRqliteClustering(options => options.Bind(configurationSection));
    }

    public void Configure(IClientBuilder builder, string name, IConfigurationSection configurationSection)
    {
        builder.UseRqliteClustering(options => options.Bind(configurationSection));
    }
}
