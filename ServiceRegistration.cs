namespace PvPmo
{
    public static class ServiceRegistration
    {
        public static void AddPvPmoServices(this IServiceCollection services)
        {
            services.AddScoped<ICaricaTabNormRepository, CaricaTabNormRepository>();
            services.AddScoped<ICaricaTabNormService, CaricaTabNormService>();

            services.AddScoped<ICaricaTabOriginiRepository, CaricaTabOriginiRepository>();
            services.AddScoped<ICaricaTabOriginiService, CaricaTabOriginiService>();

            services.AddScoped<ICaricaTabFinalizzaRepository, CaricaTabFinalizzaRepository>();
            services.AddScoped<ICaricaTabFinalizzaService, CaricaTabFinalizzaService>();

            services.AddScoped<ICaricaTabDataRepository, CaricaTabDataRepository>();
            services.AddScoped<ICaricaTabDataService, CaricaTabDataService>();           

        }
    }

}
