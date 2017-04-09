using org.apache.zookeeper;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zoodis
{
    /// <summary>
    /// 连接池，c#版的jodis客户端，实现了多个codis-proxy之间的负载均衡以及失败处理
    /// </summary>
    public class ConnectionPool
    {
        Connection[] _connections;
        int _curIndex;

        string _zookeeper;
        string _connectionZkNode;
        ConfigurationOptions _options;

        /// <summary>
        /// 创建连接池
        /// </summary>
        /// <param name="zookeeper">zookeeper服务器</param>
        /// <param name="connectionZkNode">连接信息在zookeeper中的节点</param>
        public ConnectionPool(string zookeeper, string connectionZkNode) : this(zookeeper, connectionZkNode, new ConfigurationOptions()) { }

        /// <summary>
        /// 创建连接池
        /// </summary>
        /// <param name="zookeeper">zookeeper服务器</param>
        /// <param name="connectionZkNode">连接信息在zookeeper中的节点</param>
        /// <param name="options">连接选项</param>
        public ConnectionPool(string zookeeper, string connectionZkNode, ConfigurationOptions options)
        {
            if (string.IsNullOrEmpty(zookeeper))
                throw new ArgumentNullException(nameof(zookeeper));
            if (string.IsNullOrEmpty(connectionZkNode))
                throw new ArgumentNullException(nameof(connectionZkNode));
            _zookeeper = zookeeper;
            _connectionZkNode = connectionZkNode;
            _options = options;
        }

        /// <summary>
        /// 获取连接
        /// </summary>
        /// <returns></returns>
        public async Task<ConnectionMultiplexer> GetConnectionAsync()
        {
            var con = await GetZoodisConnectionAsync();
            return con.ConnectionMultiplexer;
        }

        public async Task<Connection> GetZoodisConnectionAsync()
        {
            if (_connections == null)
                await LoadConnectionAsync();
            if (_connections == null)
                throw new InvalidOperationException("redis connections not available");
            return _connections[_curIndex++ % _connections.Length];
        }

        DateTime _lastLoadTime;
        volatile bool _loading = false;
        /// <summary>
        /// 创建连接
        /// </summary>
        async Task LoadConnectionAsync()
        {
            if (_lastLoadTime.AddSeconds(10) > DateTime.Now)
                return;
            if (_loading)
                return;
            await ForceLoadConnection();
        }

        ZooKeeper _zk;
        async Task ForceLoadConnection()
        {
            _loading = true;
            _lastLoadTime = DateTime.Now;
            List<Connection> copy = CopyConnection();

            try
            {
                if (_zk == null)
                {
                    lock (this)
                    {
                        if (_zk == null)
                        {
                            _zk = new ZooKeeper(_zookeeper, 10000, null);
                        }
                    }
                }

                var rootNode = await _zk.getChildrenAsync(_connectionZkNode, new ZoodisWatcher(OnNodeEvent));
                if (rootNode == null || rootNode.Children == null || rootNode.Children.Count == 0)
                {
                    _connections = null;
                    return;
                }

                List<Connection> newConnections = new List<Connection>(rootNode.Children.Count);
                foreach (string child in rootNode.Children)
                {
                    var connectionNode = _connectionZkNode + "/" + child;
                    var connectionData = await _zk.getDataAsync(connectionNode, new ZoodisWatcher(OnNodeEvent));

                    if (connectionData == null || connectionData.Data == null || connectionData.Data.Length == 0)
                        continue;
                    string str = Encoding.UTF8.GetString(connectionData.Data);
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<ProxyData>(str);
                    if (data == null || string.IsNullOrEmpty(data.addr) || data.state != "online")
                        continue;

                    Connection connection = null;
                    if (copy != null)
                    {
                        connection = copy.Find(c => c.Address == data.addr);
                        if (connection != null) copy.Remove(connection);
                    }
                    if (connection == null)
                    {
                        connection = await CreateConnection(data.addr);
                    }
                    newConnections.Add(connection);
                }
                _connections = newConnections.ToArray();
            }
            finally
            {
                if (copy != null && copy.Count > 0)
                    foreach (var con in copy)
                        con.Dispose();
                _loading = false;
            }
        }

        async Task ForceLoadConnection(string connectionNode)
        {

            var connectionData = await _zk.getDataAsync(connectionNode, new ZoodisWatcher(OnNodeEvent));

            if (connectionData == null || connectionData.Data == null || connectionData.Data.Length == 0)
                return;
            string str = Encoding.UTF8.GetString(connectionData.Data);
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<ProxyData>(str);
            if (data == null || string.IsNullOrEmpty(data.addr))
                return;
            List<Connection> copy = CopyConnection();
            if (data.state != "online")
            {
                //连接下线
                var con = copy.Find(c => c.Address == data.addr);
                copy.Remove(con);
                _connections = copy.ToArray();
                con.Dispose();
            }
            else
            {
                //连接上线
                var con = await CreateConnection(data.addr);
                copy.Add(con);
                _connections = copy.ToArray();
            }
        }
        async Task OnNodeEvent(WatchedEvent e)
        {
            if (e == null)
                return;
            try
            {
                string nodePath = e.getPath();
                if (nodePath != _connectionZkNode)
                {
                    //连接节点只处理数据修改事件，避免节点删除时重复加载
                    if (e.get_Type() == Watcher.Event.EventType.NodeDataChanged)
                    {
                        await ForceLoadConnection(nodePath);
                    }
                }
                else
                {
                    await ForceLoadConnection();
                }
            }
            catch
            { }
        }
        List<Connection> CopyConnection()
        {
            List<Connection> copy = null;
            if (_connections != null)
                copy = new List<Connection>(_connections);
            return copy;
        }

        async Task<Connection> CreateConnection(string addr)
        {
            ConfigurationOptions options = _options.Clone();
            options.EndPoints.Clear();
            options.EndPoints.Add(addr);
            return new Connection
            {
                ConnectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(options),
                Address = addr
            };
        }
    }
}
