using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyTcp4Net;
using RemoteController.Client.Helpers;
using System.Text;

namespace RemoteController.Client.ViewModels
{
    public class MainPageViewModel : ObservableObject
    {
        private string _id;
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _secret;
        public string Secret
        {
            get => _secret;
            set => SetProperty(ref _secret, value);
        }

        public AsyncRelayCommand LoadCommandAsync { get; set; }

        private readonly EasyTcpClient _easyTcpClient;
        private readonly EasyTcpServer _easyTcpServer;
        private readonly NetHelper _netHelper;
        public MainPageViewModel(NetHelper netHelper)
        {
            _netHelper = netHelper;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            _easyTcpClient = new EasyTcpClient("127.0.0.1",50005);
            _easyTcpServer = new EasyTcpServer((ushort)_netHelper.GetFirstAvailablePort());
        }

        private async Task LoadAsync()
        {
            Id = IdentityHelper.GetMachineCode();
            GenerateSecret();
            _easyTcpServer.StartListen();
            await _easyTcpClient.ConnectAsync();
        }

        public void GenerateSecret()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in Enumerable.Range(0, 6))
            {
                stringBuilder.Append(IdentityHelper.GetMix());
            }

            Secret = stringBuilder.ToString();
        }
    }
}
