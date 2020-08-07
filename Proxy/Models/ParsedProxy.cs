namespace Proxy.Models
{
    public class ParsedProxy
    {
        public string Host { get; }
        public int Port { get; }

        public ParsedProxy(string host, int port)
        {
            Host = host;
            Port = port;
        }
    }
}
