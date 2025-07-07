using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserNotifySseDemo.Data;

public class UserNotifySseDemoDbContextFactory : IDesignTimeDbContextFactory<UserNotifySseDemoDbContext>
{
    public UserNotifySseDemoDbContext CreateDbContext(string[] args)
    {
        UserNotifySseDemoEfCoreEntityExtensionMappings.Configure();
        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<UserNotifySseDemoDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));

        return new UserNotifySseDemoDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}