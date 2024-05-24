using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;

namespace Orleans.Reminders.Rqlite
{
    public static class RqliteHostingExtensions
    {
    

        public static ISiloBuilder UseRqliteReminder(this ISiloBuilder builder,
           Action<RqliteReminderStorageOptions> configureOptions)
        {
            return builder.ConfigureServices(services => services.UseRqliteReminder(configureOptions));
        }

        public static ISiloBuilder UseRqliteReminder(this ISiloBuilder builder,
            Action<OptionsBuilder<RqliteReminderStorageOptions>> configureOptions)
        {
            return builder.ConfigureServices(services => services.UseRqliteReminder(configureOptions));
        }

        public static ISiloBuilder UseRqliteReminder(this ISiloBuilder builder)
        {
            return builder.ConfigureServices(services =>
            {
                services.AddOptions<RqliteReminderStorageOptions>();
                services.AddSingleton<IReminderTable, RqliteReminderTable>();
            });
        }

        public static IServiceCollection UseRqliteReminder(this IServiceCollection services,
            Action<RqliteReminderStorageOptions> configureOptions)
        {
            return services.UseRqliteReminder(ob => ob.Configure(configureOptions));
        }

        public static IServiceCollection UseRqliteReminder(this IServiceCollection services,
            Action<OptionsBuilder<RqliteReminderStorageOptions>> configureOptions)
        {
            configureOptions?.Invoke(services.AddOptions<RqliteReminderStorageOptions>());
            return services.AddSingleton<IReminderTable, RqliteReminderTable>();
        }

    }
}
