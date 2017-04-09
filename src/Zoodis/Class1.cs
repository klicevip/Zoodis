using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zoodis
{
    public class Class1
    {
        //public async Task Test()
        //{

        //    ZooKeeper zk = new ZooKeeper("192.168.31.203:2181", 10000, null);
        //    try
        //    {
        //        string node = "/jodis/codis-demo";
        //        var res = await zk.getChildrenAsync(node, new JodisWatcher(OnNodeEvent));
        //        if (res.Children != null && res.Children.Count > 0)
        //        {
        //            foreach (string child in res.Children)
        //            {
        //                var proxyNode = node + "/" + child;
        //                var proxy = await zk.getDataAsync(proxyNode, new JodisWatcher(OnNodeEvent));

        //                if (proxy?.Data?.Length > 0)
        //                {
        //                    string str = Encoding.UTF8.GetString(proxy.Data);

        //                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<ProxyData>(str);
        //                }
        //            }
        //        }
        //    }
        //    catch (org.apache.zookeeper.KeeperException kex)
        //    {

        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //}

        //async Task OnNodeEvent(WatchedEvent e)
        //{
        //    Console.WriteLine("path:{0},state:{1},type:{2}", e.getPath(), e.getState(), e.get_Type());
        //}
    }
}
