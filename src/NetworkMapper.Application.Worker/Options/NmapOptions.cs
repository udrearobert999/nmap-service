namespace NetworkMapper.Application.Worker.Options;

internal sealed class NmapOptions
{
    public const string SectionName = "Nmap";
    
    public int TimeoutSeconds { get; set; } 
}