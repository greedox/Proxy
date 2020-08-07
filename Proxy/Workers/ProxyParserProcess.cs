using Microsoft.Extensions.Hosting;
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

        public ProxyParserProcess(IEnumerable<IProxyParser> parsers)
        {
            _parsers = parsers;
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
                var proxies = parser.ParsePage(url);
                //TODO: save proxy to DB
            }
        }
    }
}
