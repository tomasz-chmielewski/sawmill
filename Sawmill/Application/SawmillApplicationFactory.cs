using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sawmill.Application.Abstractions;
using Sawmill.Components.Alerts;
using Sawmill.Components.Alerts.Abstractions;
using Sawmill.Components.Statistics;
using Sawmill.Components.Statistics.Abstractions;
using System.IO;
using System.Reflection;

namespace Sawmill.Application
{
    public sealed class SawmillApplicationFactory : ISawmillApplicationFactory
    {
        public SawmillApplicationFactory(string[] args)
        {
            this.CommandLineArgs = args;
        }

        private string[] CommandLineArgs { get; }
        private ServiceProvider ServiceProvider { get; set; }

        public ISawmillApplication Create()
        {
            if(this.ServiceProvider == null)
            {
                this.ServiceProvider = this.BuildServiceProvider();
            }

            return this.ServiceProvider.GetService<ISawmillApplication>();
        }

        public void Dispose()
        {
            this.ServiceProvider?.Dispose();
        }

        private ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            this.AddConfiguration(services);
            this.AddServices(services);

            return services.BuildServiceProvider();
        }

        private void AddConfiguration(IServiceCollection services)
        {
            var builder = 
                new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("appsettings.json", true, false)
                .AddCommandLine(this.CommandLineArgs);

            var configuration = builder.Build();

            services.Configure<AlertManagerOptions>(configuration.GetSection("Alerts"));
        }

        private void AddServices(IServiceCollection services)
        {
            services.AddSingleton<ISawmillApplication, SawmillApplication>();

            services.AddTransient<IAlertManager, AlertManager>();
            services.AddTransient<IAlertHandler, AlertHandler>();

            services.AddTransient<IStatisticsManager, StatisticsManager>();
            services.AddTransient<IStatisticsCollectionFactory, StatisticsCollectionFactory>();
            services.AddTransient<IReportHandler, ReportHandler>();
        }
    }
}
