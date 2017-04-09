using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zoodis
{
    public class ZoodisWatcher : Watcher
    {
        Func<WatchedEvent, Task> _func;
        public ZoodisWatcher(Func<WatchedEvent, Task> func)
        {
            _func = func;
        }

        public override async Task process(WatchedEvent watchEvent)
        {
            if (_func != null)
                await _func(watchEvent);
        }
    }
}
