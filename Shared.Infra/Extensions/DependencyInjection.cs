using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Infra.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddDatabase<TDbC>(this IServiceCollection services,IConfiguration configuration)
        where TDbC : DbContext
    {
        services.AddDatabaseContext<TDbC>(configuration);
        return services;
    }

    public static IServiceCollection AddRepos<TRepo, TRepoImpl>(this IServiceCollection services)
        where TRepo : class
        where TRepoImpl : class, TRepo
    {
        services.AddScoped<TRepo, TRepoImpl>();
        return services;
    }

    public static IApplicationBuilder MigrateServiceDatabase<TDbc>(this IApplicationBuilder app) where TDbc : DbContext
    {
        app.ApplicationServices.MigrateDatabase<TDbc>();
        return app;
    }
}
