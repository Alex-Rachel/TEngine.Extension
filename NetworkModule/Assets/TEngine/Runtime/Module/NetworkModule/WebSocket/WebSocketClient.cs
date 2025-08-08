using System;
using System.Net.Sockets;
using UnityWebSocket;

namespace TEngine
{
    /// <summary>
    /// WebSocket客户端。
    /// </summary>
    public class WebSocketClient : AClient
    {
        public override NetworkType NetworkType => NetworkType.WebSocket;

        private IWebSocket _socket; // UnityWebSocket库的WebSocket实例。
        
        // 网络包处理相关配置。
        private readonly int _packageBodyMaxSize;  // 数据包体最大尺寸限制。
        private readonly INetPackageEncoder _encoder;  // 网络包编码器。
        private readonly INetPackageDecoder _decoder;  // 网络包解码器。
        private WebSocketChannel _channel;  // WebSocket通信通道管理实例。

        /// <summary>
        /// 内部构造函数，初始化网络包处理配置。
        /// </summary>
        internal WebSocketClient(int packageBodyMaxSize, INetPackageEncoder encoder, INetPackageDecoder decoder)
        {
            _packageBodyMaxSize = packageBodyMaxSize;
            _encoder = encoder;
            _decoder = decoder;
        }

        /// <summary>
        /// 释放资源，关闭WebSocket连接并清理通道。
        /// </summary>
        public override void Dispose()
        {
            // 确保连接关闭。
            if (_socket != null && _socket.ReadyState != WebSocketState.Closed)
            {
                _socket.CloseAsync();
            }
            
            // 释放通道资源。
            if (_channel != null)
            {
                _channel.Dispose();
                _channel = null;
            }
        }

        /// <summary>
        /// 销毁客户端时的清理操作。
        /// </summary>
        internal override void Destroy()
        {
            Dispose();
        }

        /// <summary>
        /// 每帧更新通道状态，用于处理网络消息。
        /// </summary>
        internal override void Update()
        {
            _channel?.Update();
        }

        /// <summary>
        /// 发送网络数据包。
        /// </summary>
        public override void SendPackage(INetPackage package)
        {
            if (_channel == null)
            {
                Log.Error("Channel is null.");
                return;
            }
            _channel.SendPackage(package);
        }
        
        /// <summary>
        /// 获取网络包
        /// </summary>
        public override INetPackage PickPackage()
        {
            return _channel?.PickPackage();
        }

        /// <summary>
        /// 检查当前是否处于已连接状态。
        /// </summary>
        public override bool IsConnected()
        {
            return _socket is { ReadyState: WebSocketState.Open };
        }

        /// <summary>
        /// 连接指定地址和端口的WebSocket服务。
        /// </summary>
        public override void Connect(string ip, int port, Action<SocketError> callback)
        {
            var address = port <= 0 ? $"{ip}" : $"{ip}:{port}";
            _socket = new WebSocket(address);
            // 绑定WebSocket事件回调
            _socket.OnOpen += Socket_OnOpen;
            _socket.OnMessage += Socket_OnMessage;
            _socket.OnClose += Socket_OnClose;
            _socket.OnError += Socket_OnError;
            Log.Info("Connecting...");
            _socket.ConnectAsync();
        }
        
        /// <summary>
        /// WebSocket连接成功回调，初始化通信通道。
        /// </summary>
        private void Socket_OnOpen(object sender, OpenEventArgs e)
        {
            if (_channel != null)
            {
                throw new Exception("Client channel is created.");
            }
            _channel = new WebSocketChannel();
            _channel.InitChannel(_socket, _packageBodyMaxSize, _encoder, _decoder);
            Log.Info("Web Socket Connected.");
        }

        /// <summary>
        /// 收到二进制消息时的处理，将数据交给通道处理。
        /// </summary>
        private void Socket_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.IsBinary)
            {
                _channel.OnReceive(e);
            }
        }

        /// <summary>
        /// 连接关闭事件处理，记录日志。
        /// </summary>
        private void Socket_OnClose(object sender, CloseEventArgs e)
        {
            Log.Info("Socket closed.");
        }

        /// <summary>
        /// 错误事件处理，记录错误信息。
        /// </summary>
        private void Socket_OnError(object sender, ErrorEventArgs e)
        {
           Log.Error(e.Message);
        }
    }
}
