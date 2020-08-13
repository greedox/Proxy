using Microsoft.Extensions.DependencyInjection;
using Proxy.Models.Entities;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Proxy.Services
{
    public class ProxyCheckerActor : AbstractActor<ProxyEntity>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public override int ThreadCount => 1;

        public ProxyCheckerActor(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public override async Task HandleMessage(ProxyEntity message)
        {
            var handler = new HttpClientHandler
            {
                Proxy = new WebProxy(message.Host, message.Port)
            };
            //TODO: Check proxy types for HTTPS, SOCKS4/5
            using var http = new HttpClient(handler);

            var stopwatch = new Stopwatch();

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1)))
            {
                stopwatch.Start();
                var response = await http.GetAsync("http://icanhazip.com", cts.Token);
                stopwatch.Stop();
            }

            var ellapsed = stopwatch.Elapsed;

            var updatedProxy = new ProxyEntity
            {
                Id = message.Id,
                Host = message.Host,
                Port = message.Port,
                Location = "Unkown", //need API for geolocation
                CheckTime = DateTime.UtcNow.ToUniversalTime(),
                Type = ProxyType.Http,
                IsWorked = true,
                Timeout = ellapsed.Milliseconds
            };

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

                dbContext.Proxies.Update(updatedProxy);
                await dbContext.SaveChangesAsync();
            }
        }

        public override async Task HandleError(ProxyEntity message, Exception ex)
        {
            var updatedProxy = new ProxyEntity
            {
                Id = message.Id,
                Host = message.Host,
                Port = message.Port,
                Location = "Unkown", //need API for geolocation
                CheckTime = DateTime.UtcNow.ToUniversalTime(),
                Type = ProxyType.Unkown,
                IsWorked = false,
                Timeout = -1
            };

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

                dbContext.Proxies.Update(updatedProxy);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
