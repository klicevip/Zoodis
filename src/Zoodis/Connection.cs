using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zoodis
{
    public class Connection
    {
        public string Address { get; set; }
        public ConnectionMultiplexer ConnectionMultiplexer { get; set; }

        volatile bool _disposed = false;
        internal void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            try
            {
                ConnectionMultiplexer.Dispose();
            }
            catch { }
        }
    }
}
