namespace familly_trip_advisor.Infrastructure.GeoapifyClient
{
    public static class ContainerConfigurationExtension
    {
        public static IServiceCollection AddGeoapify(this IServiceCollection serviceCollection, IConfigurationSection geoapifyConfiguration)
        {
            serviceCollection.Configure<GeoapifyOptions>(geoapifyConfiguration);
            return serviceCollection.AddSingleton<IGeoapifyHttpClient, GeoapifyHttpClient>();
        }
    }
}
