using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Infra.Extensions
{
    /// <summary>
    /// Provides extension methods for registering database services and repositories
    /// and performing database migrations.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Registers a database context of type <typeparamref name="TDbC"/> using the provided configuration.
        /// </summary>
        /// <typeparam name="TDbC">The type of the <see cref="DbContext"/> to register.</typeparam>
        /// <param name="services">The service collection to add the database context to.</param>
        /// <param name="configuration">The application configuration containing the connection string.</param>
        /// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
        public static IServiceCollection AddDatabase<TDbC>(this IServiceCollection services, IConfiguration configuration)
            where TDbC : DbContext
        {
            services.AddDatabaseContext<TDbC>(configuration);
            return services;
        }

        /// <summary>
        /// Registers a repository interface and its implementation with the dependency injection container.
        /// </summary>
        /// <typeparam name="TRepo">The repository interface type.</typeparam>
        /// <typeparam name="TRepoImpl">The concrete implementation type of the repository.</typeparam>
        /// <param name="services">The service collection to register the repository with.</param>
        /// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
        public static IServiceCollection AddRepos<TRepo, TRepoImpl>(this IServiceCollection services)
            where TRepo : class
            where TRepoImpl : class, TRepo
        {
            services.AddScoped<TRepo, TRepoImpl>();
            return services;
        }

        /// <summary>
        /// Applies pending database migrations for the specified <see cref="DbContext"/> type.
        /// </summary>
        /// <typeparam name="TDbc">The type of the <see cref="DbContext"/> to migrate.</typeparam>
        /// <param name="app">The application builder used to access services.</param>
        /// <returns>The updated <see cref="IApplicationBuilder"/> for chaining.</returns>
        /// <remarks>
        /// This method calls <see cref="DatabaseExtension.MigrateDatabase{T}"/> internally
        /// to ensure the database is up-to-date with all migrations.
        /// </remarks>
        public static IApplicationBuilder MigrateServiceDatabase<TDbc>(this IApplicationBuilder app) where TDbc : DbContext
        {
            app.ApplicationServices.MigrateDatabase<TDbc>();
            return app;
        }
    }
}
