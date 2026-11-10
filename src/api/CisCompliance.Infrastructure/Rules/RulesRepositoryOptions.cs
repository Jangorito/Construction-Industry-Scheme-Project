namespace CisCompliance.Infrastructure.Rules;

// Paths are resolved to absolute paths by InfrastructureServiceCollectionExtensions
// at registration time (using the host's content root), so this class library
// never needs an ASP.NET-specific type like IWebHostEnvironment to find its files.
public sealed class RulesRepositoryOptions
{
    public string RulesMarkdownPath { get; set; } = string.Empty;
    public string RulesIndexPath { get; set; } = string.Empty;
}
