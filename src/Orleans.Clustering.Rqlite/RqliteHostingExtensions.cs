using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Messaging;
using Orleans.Runtime.Membership;
using Orleans.Configuration;

namespace Orleans.Hosting
{

    public static class RqliteHostingExtensions
    {
        /// <summary>
        /// Configures the silo to use Rqlite for cluster membership.
        /// </summary>
        /// <param name="builder">
        /// The builder.
        /// </param>
        /// <param name="configureOptions">
        /// The configuration delegate.
        /// </param>
        /// <returns>
        /// The provided <see cref="ISiloBuilder"/>.
        /// </returns>
        public static ISiloBuilder UseRqliteClustering(
            this ISiloBuilder builder,
            Action<RqliteClusteringSiloOptions> configureOptions)
        {
            return builder.ConfigureServices(
                services =>
                {
                    if (configureOptions != null)
                    {
                        services.Configure(configureOptions);
                    }

                    services.AddSingleton<IMembershipTable, RqliteBasedMembershipTable>();
                });
        }

        /// <summary>
        /// Configures the silo to use Rqlite for cluster membership.
        /// </summary>
        /// <param name="builder">
        /// The builder.
        /// </param>
        /// <param name="configureOptions">
        /// The configuration delegate.
        /// </param>
        /// <returns>
        /// The provided <see cref="ISiloBuilder"/>.
        /// </returns>
        public static ISiloBuilder UseRqliteClustering(
            this ISiloBuilder builder,
            Action<OptionsBuilder<RqliteClusteringSiloOptions>> configureOptions)
        {
            return builder.ConfigureServices(
                services =>
                {
                    configureOptions?.Invoke(services.AddOptions<RqliteClusteringSiloOptions>());
                    services.AddSingleton<IMembershipTable, RqliteBasedMembershipTable>();
                });
        }

        /// <summary>
        /// Configure the client to use Rqlite for clustering.
        /// </summary>
        /// <param name="builder">
        /// The builder.
        /// </param>
        /// <param name="configureOptions">
        /// The configuration delegate.
        /// </param>
        /// <returns>
        /// The provided <see cref="IClientBuilder"/>.
        /// </returns>
        public static IClientBuilder UseRqliteClustering(
            this IClientBuilder builder,
            Action<RqliteGatewayListProviderOptions> configureOptions)
        {
            return builder.ConfigureServices(
                services =>
                {
                    if (configureOptions != null)
                    {
                        services.Configure(configureOptions);
                    }

                    services.AddSingleton<IGatewayListProvider, RqliteGatewayListProvider>();
                });
        }

        /// <summary>
        /// Configure the client to use Rqlite for clustering.
        /// </summary>
        /// <param name="builder">
        /// The builder.
        /// </param>
        /// <param name="configureOptions">
        /// The configuration delegate.
        /// </param>
        /// <returns>
        /// The provided <see cref="IClientBuilder"/>.
        /// </returns>
        public static IClientBuilder UseRqliteClustering(
            this IClientBuilder builder,
            Action<OptionsBuilder<RqliteGatewayListProviderOptions>> configureOptions)
        {
            return builder.ConfigureServices(
                services =>
                {
                    configureOptions?.Invoke(services.AddOptions<RqliteGatewayListProviderOptions>());
                    services.AddSingleton<IGatewayListProvider, RqliteGatewayListProvider>();
                });
        }
    }
}
