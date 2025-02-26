using NetCoreBasics.Interfaces;
using NetCoreBasics.Services;

namespace NetCoreBasics
{
    public static class Extension
    {
        #region Configurando Logger
        public static void ConfigureLogger(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger<ApplicationLogger>>();
            services.AddSingleton<ILogger>(logger!);
        }
        #endregion

        #region Injeção de dependência
        /* Injeção de dependência
            - Singleton: Criado uma vez e usado globalmente.
            - Scoped: Criado por requisição e compartilhado dentro dela.
            - Transient: Criado toda vez que for solicitado.
         */
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISingletonService, SingletonService>();
            services.AddScoped<IScopedService, ScopedService>();
            services.AddTransient<ITransientService, TransientService>();
        }
        #endregion
    }
}
