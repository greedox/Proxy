using Proxy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Proxy.Services.ProxyParsers
{
    public class FreeProxyCzParser : IProxyParser
    {
        private readonly HttpClient _http;

        public FreeProxyCzParser(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient();
            _http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0;) Gecko/20100101 Firefox/77.0");
        }
        public async IAsyncEnumerable<string> GetPagesForParse()
        {
            for (int i = 0; i < 150; i++)
            {
                await Task.Delay(1500);
                yield return $"http://free-proxy.cz/ru/proxylist/main/{ i + 1 }";
            }
        }

        public async Task<ParsedProxy[]> ParsePage(string url)
        {
            var body = await _http.GetStringAsync(url);

            var regex = Regex.Matches(body, @"(?<=Base64\.decode\(\"")([^""]+)[^\\d]+(\\d{2,5})");

            var proxies = regex.Select(s =>
            {
                var host = Decode(s.Groups[1].Value);
                var port = int.Parse(s.Groups[2].Value);

                return new ParsedProxy(host, port);
            }).ToArray();

            return proxies;
        }

        private string Decode(string input)
        {
            byte[] buffer = Convert.FromBase64String(input);
            return Encoding.ASCII.GetString(buffer);
        }
    }
}
