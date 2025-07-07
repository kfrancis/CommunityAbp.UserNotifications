using Volo.Abp.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace UserNotifySignalRDemo.Data;

public class UserNotifySignalRDemoDbSchemaMigrator : ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public UserNotifySignalRDemoDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        
        /* We intentionally resolving the UserNotifySignalRDemoDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<UserNotifySignalRDemoDbContext>()
            .Database
            .MigrateAsync();

    }
}
