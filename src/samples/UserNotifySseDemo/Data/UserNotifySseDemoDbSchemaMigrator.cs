using Volo.Abp.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace UserNotifySseDemo.Data;

public class UserNotifySseDemoDbSchemaMigrator : ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public UserNotifySseDemoDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        
        /* We intentionally resolving the UserNotifySseDemoDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<UserNotifySseDemoDbContext>()
            .Database
            .MigrateAsync();

    }
}
