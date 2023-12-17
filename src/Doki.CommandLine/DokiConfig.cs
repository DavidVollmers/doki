namespace Doki.CommandLine;

internal class DokiConfig
{
    public DokiConfigOutput[]? Outputs { get; set; }
    
    public class DokiConfigOutput
    {
        public string? Type { get; set; }

        public string? From { get; set; }
    }
}