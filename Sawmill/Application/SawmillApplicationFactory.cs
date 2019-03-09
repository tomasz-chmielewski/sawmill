using Microsoft.Extensions.DependencyInjection;
using Sawmill.Application.Abstractions;
using Sawmill.Components.Alerts;
using Sawmill.Components.Alerts.Abstractions;
using Sawmill.Components.Statistics;
using Sawmill.Components.Statistics.Abstractions;

namespace Sawmill.Application
{
    public sealed class SawmillApplicationFactory : ISawmillApplicationFactory
    {
        private ServiceProvider ServiceProvider { get; set; }

        public ISawmillApplication Create()
        {
            if(this.ServiceProvider == null)
            {
                var serviceCollection = new ServiceCollection();

                serviceCollection.AddSingleton<ISawmillApplication, SawmillApplication>();

                serviceCollection.AddTransient<IAlertManager, AlertManager>();
                serviceCollection.AddTransient<IAlertHandler, AlertHandler>();

                serviceCollection.AddTransient<IStatisticsManager, StatisticsManager>();
                serviceCollection.AddTransient<IStatisticsCollectionFactory, StatisticsCollectionFactory>();
                serviceCollection.AddTransient<IReportHandler, ReportHandler>();

                this.ServiceProvider = serviceCollection.BuildServiceProvider();
            }

            return this.ServiceProvider.GetService<ISawmillApplication>();
        }

        public void Dispose()
        {
            this.ServiceProvider?.Dispose();
        }
    }
}
