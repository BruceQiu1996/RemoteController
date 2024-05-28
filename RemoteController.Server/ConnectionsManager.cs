using EasyTcp4Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RemoteController.Server
{
    /// <summary>
    /// 全局的连接管理器
    /// </summary>
    public class ConnectionsManager
    {
        private readonly object _lock = new object();
        private readonly List<Connection> _connections;
        private readonly EasyTcpServer _server;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConnectionsManager> _logger;
        public ConnectionsManager(IConfiguration configuration, ILogger<ConnectionsManager> logger)
        {
            _connections = new List<Connection>();
            _logger = logger;
            _configuration = configuration;
            _server = new EasyTcpServer(_configuration.GetValue<ushort>("ListenPort"), new EasyTcpServerOptions()
            {
                ConnectionsLimit = _configuration.GetValue<int>("MaxConnections"),
                IsSsl = false
            });
        }

        public void Start()
        {
            try
            {
                _server.StartListen();
                _server.OnReceivedData += OnReceivedData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
        }

        private async void OnReceivedData(object obj, ServerDataReceiveEventArgs eventArgs)
        {

        }

        public bool AddConnection(Connection connection)
        {
            try
            {
                lock (_lock)
                {
                    var temp = _connections.FirstOrDefault(x => x.EquipmentId == connection.EquipmentId
                        && x.EquipmentSecret == connection.EquipmentSecret);
                    if (temp != null)
                    {
                        temp.Session?.Dispose();
                        _connections.Remove(temp);
                    }

                    _connections.Add(connection);

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return false;
            }
        }
    }

    public class Connection
    {
        public ClientSession Session { get; set; }
        public string EquipmentId { get; set; }
        public string EquipmentSecret { get; set; }
    }
}
