using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Clustering.Rqlite;
using Orleans.Messaging;

namespace Microsoft.Extensions.Hosting
{
    public static class RqliteHostingExtensions
    {
        public static ISiloBuilder UseRqliteMembership(this ISiloBuilder builder,
           Action<RqliteClusteringOptions> configureOptions)
        {
            return builder.ConfigureServices(services => services.UseRqliteMembership(configureOptions));
        }

        public static ISiloBuilder UseRqliteMembership(this ISiloBuilder builder,
            Action<OptionsBuilder<RqliteClusteringOptions>> configureOptions)
        {
            return builder.ConfigureServices(services => services.UseRqliteMembership(configureOptions));
        }

        public static ISiloBuilder UseRqliteMembership(this ISiloBuilder builder)
        {
            return builder.ConfigureServices(services =>
            {
                services.AddOptions<RqliteClusteringOptions>();
                services.AddSingleton<IMembershipTable, RqliteMembershipTable>();
            });
        }

        public static IClientBuilder UseRqliteGatewayListProvider(this IClientBuilder builder, Action<RqliteGatewayOptions> configureOptions)
        {
            return builder.ConfigureServices(services => services.UseRqliteGatewayListProvider(configureOptions));
        }

        public static IClientBuilder UseRqliteGatewayListProvider(this IClientBuilder builder)
        {
            return builder.ConfigureServices(services =>
            {
                services.AddOptions<RqliteGatewayOptions>();
                services.AddSingleton<IGatewayListProvider, RqliteGatewayListProvider>();
            });
        }

        public static IClientBuilder UseRqliteGatewayListProvider(this IClientBuilder builder, Action<OptionsBuilder<RqliteGatewayOptions>> configureOptions)
        {
            return builder.ConfigureServices(services => services.UseRqliteGatewayListProvider(configureOptions));
        }

        public static IServiceCollection UseRqliteMembership(this IServiceCollection services,
            Action<RqliteClusteringOptions> configureOptions)
        {
            return services.UseRqliteMembership(ob => ob.Configure(configureOptions));
        }

        public static IServiceCollection UseRqliteMembership(this IServiceCollection services,
            Action<OptionsBuilder<RqliteClusteringOptions>> configureOptions)
        {
            configureOptions?.Invoke(services.AddOptions<RqliteClusteringOptions>());
            return services.AddSingleton<IMembershipTable, RqliteMembershipTable>();
        }

        public static IServiceCollection UseRqliteGatewayListProvider(this IServiceCollection services,
            Action<RqliteGatewayOptions> configureOptions)
        {
            return services.UseRqliteGatewayListProvider(ob => ob.Configure(configureOptions));
        }

        public static IServiceCollection UseRqliteGatewayListProvider(this IServiceCollection services,
            Action<OptionsBuilder<RqliteGatewayOptions>> configureOptions)
        {
            configureOptions?.Invoke(services.AddOptions<RqliteGatewayOptions>());
            return services.AddSingleton<IGatewayListProvider, RqliteGatewayListProvider>();
        }
    }
}
