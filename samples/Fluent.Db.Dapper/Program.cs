using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fluent.Db.Dapper
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            // Sample test
            using (ServiceProvider sp = services.BuildServiceProvider())
            {
                ILogger logger = sp.GetService<ILogger>();
                FluentDbCommandSample sampleDbCommand = new FluentDbCommandSample();
                sampleDbCommand.CleanData().Wait();
                sampleDbCommand.InsertDataByObjectCommand().Wait();
                sampleDbCommand.ExecuteManyCommandWithTransaction().Wait();
                sampleDbCommand.ExecuteWithDispatcher(logger).Wait();
            }
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole());
        }
    }
}
