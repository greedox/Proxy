using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proxy.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Proxy.Workers
{
    public class ProxyCheckerProcess : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ProxyCheckerActor _checkerActor;

        public ProxyCheckerProcess(IServiceScopeFactory serviceScopeFactory, ProxyCheckerActor checkerActor)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _checkerActor = checkerActor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                    var proxies = dbContext.Proxies.Select(x => x).ToList();

                    foreach (var proxy in proxies)
                    {
                        await _checkerActor.SendAsync(proxy);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(60));
                }
            }
        }
    }
}
