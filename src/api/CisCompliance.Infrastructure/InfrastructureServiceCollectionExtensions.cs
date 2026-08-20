using CisCompliance.Core.Abstractions;
using CisCompliance.Infrastructure.OpenAi;
using CisCompliance.Infrastructure.Persistence;
using CisCompliance.Infrastructure.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace CisCompliance.Infrastructure;

// The only DI-wiring surface this project exposes. Program.cs calls this
// once, it never needs to know about the individual types underneath.
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddCisComplianceInfrastructure(
        this IServiceCollection services, IConfiguration configuration, string contentRootPath)
    {
        services.Configure<OpenAiOptions>(configuration.GetSection("OpenAI"));
        services.Configure<RulesRepositoryOptions>(options =>
        {
            configuration.GetSection("Rules").Bind(options);
            // Rules:* in appsettings.json holds paths relative to the Api
            // project's output folder (e.g. "Data/cis_rules.md"); resolved
            // to absolute here so FileRulesRepository never has to guess
            // the current working directory.
            options.RulesMarkdownPath = Path.Combine(contentRootPath, options.RulesMarkdownPath);
            options.RulesIndexPath = Path.Combine(contentRootPath, options.RulesIndexPath);
        });

        services.AddSingleton(sp =>
        {
            var openAi = sp.GetRequiredService<IOptions<OpenAiOptions>>().Value;
            if (string.IsNullOrWhiteSpace(openAi.ApiKey))
            {
                throw new InvalidOperationException(
                    "OpenAI:ApiKey is not configured. Set it via 'dotnet user-secrets set \"OpenAI:ApiKey\" \"<key>\"' " +
                    "in Development, or the OPENAI__ApiKey environment variable elsewhere.");
            }

            return new ChatClient(openAi.Model, openAi.ApiKey);
        });

        services.AddSingleton<IClassificationModelClient, OpenAiClassificationModelClient>();
        services.AddSingleton<IExtractionModelClient, OpenAiExtractionModelClient>();
        services.AddSingleton<IRulesRepository, FileRulesRepository>();

        var connectionString = configuration.GetConnectionString("CisComplianceDb")
            ?? throw new InvalidOperationException("ConnectionStrings:CisComplianceDb is not configured.");
        services.AddDbContext<CisComplianceDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IInvoiceRepository, EfInvoiceRepository>();

        return services;
    }
}
