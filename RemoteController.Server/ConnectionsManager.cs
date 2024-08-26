using EasyTcp4Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RemoteController.Common.Dtos;
using RemoteController.Common.Dtos.API.Equipment;
using RemoteController.Common.Dtos.Regist;

namespace RemoteController.Server
{
    /// <summary>
    /// 全局的连接管理器
    /// </summary>
    public class ConnectionsManager
    {
        private readonly object _lock = new object();
        private readonly List<Connection> _connections;
        private readonly List<ClientSession> _waittingConnections;
        private readonly EasyTcpServer _server;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConnectionsManager> _logger;
        public ConnectionsManager(IConfiguration configuration, ILogger<ConnectionsManager> logger)
        {
            _waittingConnections = new List<ClientSession>();
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
                _server.OnClientConnectionChanged += OnNewConnection;
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

        /// <summary>
        /// 接收到一个新的连接
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="eventArgs"></param>
        private async void OnNewConnection(object obj, ServerSideClientConnectionChangeEventArgs eventArgs)
        {
            var sessionId = eventArgs.ClientSession.SessionId;
            if (eventArgs.Status == ConnectsionStatus.Connected)
            {
                await eventArgs.ClientSession.SendAsync(new Packet<ConnectedAck>()
                {
                    MessageType = MessageType.ConnectedAck,
                    Body = new ConnectedAck()
                    {
                        SessionId = sessionId
                    }
                }.Serialize());
            }

            var session = eventArgs.ClientSession;
            _waittingConnections.Add(session);
            var _ = new Task(async () =>
            {
                await Task.Delay(30 * 1000);
                _waittingConnections.Remove(session);
            });
        }

        public bool Regist(RegistDto registDto)
        {
            var session = _waittingConnections.FirstOrDefault(x => x.SessionId == registDto.SessionId);
            if (session == null)

                return false;

            lock (_lock)
            {
                var connection = _connections
                    .FirstOrDefault(x => x.EquipmentId == registDto.EquipmentId);
                if (connection != null)
                {
                    _connections.Remove(connection);
                    connection.Session.Dispose();
                }

                _connections.Add(new Connection()
                {
                    Session = session,
                    EquipmentId = registDto.EquipmentId,
                    EquipmentSecret = registDto.EquipmentSecret,
                    Port = registDto.Port
                });

                return true;
            }
        }
    }

    public class Connection
    {
        public ClientSession Session { get; set; }
        public string EquipmentId { get; set; }
        public string EquipmentSecret { get; set; }
        public ushort Port { get; set; }
    }
}
