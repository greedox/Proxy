using System;

namespace Proxy.Models.Entities
{
    public class ProxyEntity
    {
        public int Id { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public ProxyType Type { get; set; }
        public int Timeout { get; set; }
        public string Location { get; set; }
        public DateTime CheckTime { get; set; }
        public bool IsWorked { get; set; }
    }

    public enum ProxyType
    {
        Unkown,
        Http,
        Https,
        Socks4,
        Socks5
    }
}
