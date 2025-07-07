using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserNotifySignalRDemo.Data;

public class UserNotifySignalRDemoDbContextFactory : IDesignTimeDbContextFactory<UserNotifySignalRDemoDbContext>
{
    public UserNotifySignalRDemoDbContext CreateDbContext(string[] args)
    {
        UserNotifySignalRDemoEfCoreEntityExtensionMappings.Configure();
        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<UserNotifySignalRDemoDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));

        return new UserNotifySignalRDemoDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}