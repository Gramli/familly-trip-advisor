namespace familly_trip_advisor.Infrastructure.OllamaClient
{
    public class OllamaOptions
    {
        public const string Ollama = "Ollama";

        public string BaseUrl { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
    }
}
