using Proxy.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Proxy.Services.ProxyParsers
{
    public class FreeProxyListParser : IProxyParser
    {
        private readonly HttpClient _http;

        public FreeProxyListParser(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient();
            _http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0;) Gecko/20100101 Firefox/77.0");
        }

        public async IAsyncEnumerable<string> GetPagesForParse()
        {
            yield return "https://getfreeproxylists.blogspot.com/";
        }

        public async Task<ParsedProxy[]> ParsePage(string url)
        {
            var response = await _http.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();

            return Regex.Matches(body, @"(\d{2,3}\.\d{2,3}\.\d{2,3}\.\d{2,3}):(\d{2,5})")
                .Select(s =>
                {
                    var host = s.Groups[1].Value;
                    var port = int.Parse(s.Groups[2].Value);

                    return new ParsedProxy(host, port);
                })
                .ToArray();
        }
    }
}
