using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyTcp4Net;
using RemoteController.Client.Helpers;
using RemoteController.Common.Dtos;
using RemoteController.Common.Dtos.API.Equipment;
using RemoteController.Common.Dtos.Regist;
using System.Buffers.Binary;
using System.Text;
using System.Text.Json;

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
        /// <summary>
        /// 连接服务器的socket
        /// </summary>
        private readonly EasyTcpClient _easyTcpClient;
        /// <summary>
        /// 用于监听远程桌面连接的服务
        /// </summary>
        private readonly EasyTcpServer _easyTcpServer;
        private readonly NetHelper _netHelper;
        private readonly ushort _listeningPort;
        private readonly HttpRequest _httpHelper;
        public MainPageViewModel(NetHelper netHelper, HttpRequest httpHelper)
        {
            _netHelper = netHelper;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            _easyTcpClient = new EasyTcpClient("127.0.0.1", 54445);
            _listeningPort = (ushort)_netHelper.GetFirstAvailablePort();
            _easyTcpServer = new EasyTcpServer(_listeningPort);
            _easyTcpClient.OnReceivedData += OnReceiveDataAsync;
            _httpHelper = httpHelper;
        }

        private async Task LoadAsync()
        {
            Id = IdentityHelper.GetMachineCode();
            GenerateSecret();
            _easyTcpServer.StartListen();
            await _easyTcpClient.ConnectAsync();
        }

        private async void OnReceiveDataAsync(object obj, ClientDataReceiveEventArgs eventArgs)
        {
            var type = (MessageType)BinaryPrimitives.ReadInt32BigEndian(eventArgs.Data.Slice(12, 4).Span);
            switch (type)
            {
                case MessageType.ConnectedAck:
                    {
                        var packet = Packet<ConnectedAck>.FromBytes(eventArgs.Data);
                        try
                        {
                            var resp = await _httpHelper.PostAsync(RequestUrls.Regist, new RegistDto()
                            {
                                Port = _listeningPort,
                                EquipmentId = Id,
                                EquipmentSecret = Secret,
                                SessionId = packet.Body.SessionId
                            });

                            if (resp.IsSuccessStatusCode)
                            {
                                var result = JsonSerializer.Deserialize<RegistResponseDto>(await resp.Content.ReadAsStringAsync()
                                    , HttpRequest._jsonSerializerOptions);

                                if (result.Success)
                                {
                                    _httpHelper.SetToken(result.Token);
                                }
                            }
                            else
                            {
                                //连接服务端失败，请检查状态

                            }
                        }
                        catch (Exception ex) 
                        {
                        
                        }

                        
                    }
                    break;
            }
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
