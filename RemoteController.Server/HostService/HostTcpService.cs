using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RemoteController.Server.HostService
{
    /// <summary>
    /// 服务端 Tcp Host服务
    /// </summary>
    public class HostTcpService : BackgroundService
    {
        private readonly ConnectionsManager _connectionsManager;
        private readonly ILogger<HostTcpService> _logger;
        public HostTcpService(ConnectionsManager connectionsManager, ILogger<HostTcpService> logger)
        {
            _logger = logger;
            _connectionsManager = connectionsManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _connectionsManager.Start();

                await Task.CompletedTask;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex.ToString());
                _logger.LogInformation($"Server side tcp service startup occur error.");
            }
        }
    }
}
