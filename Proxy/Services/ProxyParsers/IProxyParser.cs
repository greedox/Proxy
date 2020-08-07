using Proxy.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Proxy.Services.ProxyParsers
{
    public interface IProxyParser
    {
        IAsyncEnumerable<string> GetPagesForParse();
        Task<ParsedProxy[]> ParsePage(string url);
    }
}
