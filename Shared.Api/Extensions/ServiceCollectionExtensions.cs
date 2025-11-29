using System.Text;
using FluentValidation;
using Freelance.Shared.Application.Behaviours;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared.Infra.Settings;

namespace Freelance.Shared.Api.Extensions;

  public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseContext<TContext>(
            this IServiceCollection services, 
            IConfiguration configuration) 
            where TContext : DbContext
        {
            services.AddDbContext<TContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
            
            return services;
        }

        public static IServiceCollection AddMediatorWithValidation(
            this IServiceCollection services,
            Type mediatorAssemblyMarker,
            Type validatorAssemblyMarker,
            Type mappingProfileType)
        {
            services.AddMediatR(cfg => 
                cfg.RegisterServicesFromAssembly(mediatorAssemblyMarker.Assembly));
            
            services.AddAutoMapper(cfg => { }, mappingProfileType.Assembly);
            
            services.AddValidatorsFromAssembly(validatorAssemblyMarker.Assembly);
            
            services.AddTransient(
                typeof(IPipelineBehavior<,>), 
                typeof(ValidationBehavior<,>));
            
            return services;
        }

        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            services.Configure<JwtSettings>(jwtSettings);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var key = jwtSettings["Key"];
                if (string.IsNullOrEmpty(key))
                    throw new InvalidOperationException("JWT Key is missing in configuration.");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };
            });

            return services;
        }

        public static IServiceCollection AddRoleBasedAuthorization(
            this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("FreelancerOnly", 
                    policy => policy.RequireRole("Freelancer"));
                options.AddPolicy("ClientOnly", 
                    policy => policy.RequireRole("Client"));
            });

            return services;
        }

        public static IServiceCollection AddOpenTelemetryObservability(
            this IServiceCollection services,
            string otelEndpoint,
            string serviceName,
            ResourceBuilder resourceBuilder)
        {
            services.AddOpenTelemetry()
                .ConfigureResource(rb => rb.AddService(
                    serviceName: serviceName, 
                    serviceVersion: "1.0.0"))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation(opts =>
                        {
                            opts.RecordException = true;
                        })
                        .AddHttpClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation(options =>
                        {
                            options.EnrichWithIDbCommand = (activity, command) =>
                            {
                                activity.SetTag("db.statement", command.CommandText);
                            };
                        })
                        .AddSource(serviceName)
                        .AddOtlpExporter(o =>
                        {
                            o.Endpoint = new Uri(otelEndpoint);
                        });
                })
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddProcessInstrumentation()
                        .AddEventCountersInstrumentation(c =>
                        {
                            c.AddEventSources("Microsoft.AspNetCore.Hosting");
                        })
                        .AddOtlpExporter(o =>
                        {
                            o.Endpoint = new Uri(otelEndpoint);
                        });
                });

            return services;
        }

        public static ILoggingBuilder AddOpenTelemetryLogging(
            this ILoggingBuilder logging,
            string otelEndpoint,
            ResourceBuilder resourceBuilder)
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resourceBuilder);
                options.IncludeScopes = true;
                options.IncludeFormattedMessage = true;
                options.ParseStateValues = true;
                options.AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(otelEndpoint);
                });
            });

            return logging;
        }

        public static IServiceCollection AddSwaggerWithJwtAuth(
            this IServiceCollection services,
            string title,
            string version = "v1")
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(version, new OpenApiInfo { Title = title, Version = version });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter JWT Bearer token **_only_**",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                options.AddSecurityDefinition("Bearer", securityScheme);
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { securityScheme, Array.Empty<string>() }
                });
            });

            return services;
        }

        public static ResourceBuilder CreateServiceResourceBuilder(
            string serviceName,
            string environmentName,
            string serviceVersion = "1.0.0")
        {
            return ResourceBuilder.CreateDefault()
                .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
                .AddAttributes(new[]
                {
                    new KeyValuePair<string, object>("deployment.environment", environmentName),
                    new KeyValuePair<string, object>("service.namespace", "freelance"),
                });
        }
    }

