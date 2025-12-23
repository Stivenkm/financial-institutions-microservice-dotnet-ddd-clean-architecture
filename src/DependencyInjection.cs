using System.Runtime.CompilerServices;
using FluentValidation;
using Intec.Banking.FinancialInstitutions.Infrastructure;
using Intec.Banking.FinancialInstitutions.Infrastructure.Filters;
using Intec.Banking.FinancialInstitutions.Infrastructure.SnowflakeId;
using Intec.Banking.FinancialInstitutions.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Intec.Banking.FinancialInstitutions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Context
        services.AddDbContext<FinancialInstitutionsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default"))
        );

        // Configure IdGenerator (Snowflake)
        var workerId = configuration.GetValue<ushort>("IdGenerator:WorkerId");
        var datacenterId = configuration.GetValue<ushort>("IdGenerator:DatacenterId");

        var idGeneratorOptions = new IdGeneratorOptions
        {
            WorkerId = workerId,
            DatacenterId = datacenterId
        };

        services.AddSingleton<IIdGeneratorPool>(sp => new DefaultIdGeneratorPool(idGeneratorOptions));
        services.AddSingleton<IIdGenerator, SnowflakeIdGenerator>();

        // Repository & Unit of Work
        services.AddScoped<IFinancialInstitutionRepository, FinancialInstitutionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();


        // CQRS Dispatchers
        services.AddScoped<CommandDispatcher>();
        services.AddScoped<QueryDispatcher>();

        // Register all Command Handlers
        RegisterHandlers(services, typeof(ICommandHandler<,>));

        // Register all Query Handlers
        RegisterHandlers(services, typeof(IQueryHandler<,>));

        // FluentValidation - Register all validators from assembly
       // services.AddValidatorsFromAssemblyContaining<>(ServiceLifetime.Scoped);

        // Filters
        services.AddScoped<ValidationFilter>();

        // Database Seeder
        /*
        if (app.Environment.IsDevelopment())
        {
            DatabaseSeeder.SeedData(configuration); 
        }

        */
        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Type handlerInterfaceType)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        var handlers = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces(), (type, iface) => new { Type = type, Interface = iface })
            .Where(x => x.Interface.IsGenericType && x.Interface.GetGenericTypeDefinition() == handlerInterfaceType)
            .ToList();

        foreach (var handler in handlers)
        {
            services.AddScoped(handler.Interface, handler.Type);
        }
    }
}