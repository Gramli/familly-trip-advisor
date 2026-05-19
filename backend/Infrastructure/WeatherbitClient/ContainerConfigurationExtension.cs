namespace familly_trip_advisor.Infrastructure.WeatherbitClient
{
    public static class ContainerConfigurationExtension
    {
        public static IServiceCollection AddWeatherbit(this IServiceCollection serviceCollection, IConfigurationSection weatherbitConfiguration)
        {
            serviceCollection.Configure<WeatherbitOptions>(weatherbitConfiguration);
            return serviceCollection.AddSingleton<IWeatherbitHttpClient, WeatherbitHttpClient>();
        }
    }
}
