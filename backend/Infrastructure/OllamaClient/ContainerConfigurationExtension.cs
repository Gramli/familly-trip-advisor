namespace familly_trip_advisor.Infrastructure.OllamaClient
{
    public static class ContainerConfigurationExtension
    {
        public static IServiceCollection AddOllama(this IServiceCollection serviceCollection, IConfigurationSection ollamaConfiguration)
        {
            serviceCollection.Configure<OllamaOptions>(ollamaConfiguration);
            serviceCollection.AddMemoryCache();
            return serviceCollection.AddSingleton<IOllamaClient, OllamaClient>();
        }
    }
}
