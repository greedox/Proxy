using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proxy.Models.Entities;
using Proxy.Services.ProxyParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Proxy.Workers
{
    public class ProxyParserProcess : BackgroundService
    {
        private readonly IEnumerable<IProxyParser> _parsers;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ProxyParserProcess(IEnumerable<IProxyParser> parsers, IServiceScopeFactory serviceScopeFactory)
        {
            _parsers = parsers;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                var tasks = _parsers.Select(Handle);
                await Task.WhenAll(tasks);
                await Task.Delay(TimeSpan.FromSeconds(60));
            }
        }

        private async Task Handle(IProxyParser parser)
        {
            await foreach (var url in parser.GetPagesForParse())
            {
                var proxies = await parser.ParsePage(url);
                
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                    foreach (var proxy in proxies)
                    {
                        var dbProxy = await dbContext.Proxies.FirstOrDefaultAsync(x => x.Host == proxy.Host && x.Port == proxy.Port);
                        if (dbProxy == null)
                        {
                            dbContext.Proxies.Add(new ProxyEntity
                            {
                                Host = proxy.Host,
                                Port = proxy.Port
                            });
                        }
                    }

                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
