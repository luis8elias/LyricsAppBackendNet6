using MediatR;
using Microsoft.EntityFrameworkCore;
using LyricsApp.Application.Common.Behaviours;
using LyricsApp.Application.Helpers;
using LyricsApp.Application.Infrastructure.Persistence;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using LyricsApp.Application.Common.Configurations;
using LyricsApp.Application.Domain.Interfaces;
using NSwag;
using NSwag.Generation.Processors.Security;
using LyricsApp.Application.Infrastructure.Seeds;
using LyricsApp.Application.Services;

namespace LyricsApp.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddOpenApiDocument(c =>
        {
            c.Title = "LyricsApp APIs";
            c.Version = "v1";

            c.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = "Authorization",
                In = OpenApiSecurityApiKeyLocation.Header,
                Description = "Type into the textbox: Bearer {your JWT token}."
            });

            c.OperationProcessors.Add(
                new AspNetCoreOperationSecurityScopeProcessor("JWT"));
        });



        return services;
    }

    public static IServiceCollection AddCustomCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(name: AppConstants.CorsPolicy, builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        });

        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Default");

        services.AddDbContext<ApiDbContext>(options => options.UseSqlServer(connectionString));

        return services;
    }

    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        services.AddMediatR(typeof(Application.Application));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehaviour<,>));

        return services;
    }

    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
    {
        JwtConfiguration jwtConfiguration = config.GetSection("Jwt").Get<JwtConfiguration>() ?? new ();
        services.AddSingleton(jwtConfiguration);


        EmailConfiguration emailConfiguration = config.GetSection("EmailConfiguration").Get<EmailConfiguration>() ?? new ();

        services.AddSingleton(emailConfiguration);

        return services;
    }

    public static IServiceCollection AddJwt(this IServiceCollection services, IConfiguration configuration)
    {
        JwtConfiguration jwt = configuration.GetSection("Jwt").Get<JwtConfiguration>() ?? new ();

        services.AddHttpContextAccessor()
        .AddAuthorization()
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecurityKey))
        });

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<IDatabaseInitializer, DatabaseInitializer>();

        services.AddScoped<IEmailSender, EmailSenderService>();
        services.AddTransient<IAuthService, AuthService>();

        return services;
    }
}