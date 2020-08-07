using Proxy.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Proxy.Services.ProxyParsers
{
    public class HideMyParser : IProxyParser
    {
        private readonly HttpClient _http;

        public HideMyParser(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient();
        }

        public async IAsyncEnumerable<string> GetPagesForParse()
        {
            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(1500);
                yield return $"https://hidemy.name/ru/proxy-list/?start={ i * 64 }#list";
            }
        }

        public async Task<ParsedProxy[]> ParsePage(string url)
        {
            var body = await _http.GetStringAsync(url);
            var regex = Regex.Matches(body, @"(\d{2,3}\.\d{2,3}\.\d{2,3}\.\d{2,3})[^\d]*(\d{2,5})");
            
            var proxies = regex.Select(s =>
            {
                var host = s.Groups[1].Value;
                var port = int.Parse(s.Groups[2].Value);

                return new ParsedProxy(host, port);
            }).ToArray();

            return proxies;
        }
    }
}
