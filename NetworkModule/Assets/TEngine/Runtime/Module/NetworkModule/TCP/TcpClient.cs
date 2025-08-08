using System;
using System.Net;
using System.Net.Sockets;

namespace TEngine
{
    public class TcpClient : AClient
    {
        public override NetworkType NetworkType => NetworkType.TCP;

        private class UserToken
        {
            public Action<SocketError> Callback;
        }

        private TcpChannel _channel;
        private readonly int _packageBodyMaxSize;
        private readonly INetPackageEncoder _encoder;
        private readonly INetPackageDecoder _decoder;
        private readonly ThreadSyncContext _syncContext;

        private TcpClient()
        {
        }

        internal TcpClient(int packageBodyMaxSize, INetPackageEncoder encoder, INetPackageDecoder decoder)
        {
            _packageBodyMaxSize = packageBodyMaxSize;
            _encoder = encoder;
            _decoder = decoder;
            _syncContext = new ThreadSyncContext();
        }

        /// <summary>
        /// 每帧更新网络状态。
        /// 处理线程同步上下文的消息队列，并驱动Tcp频道进行网络更新。
        /// </summary>
        internal override void Update()
        {
            _syncContext.Update();

            _channel?.Update();
        }

        /// <summary>
        /// 销毁网络
        /// </summary>
        internal override void Destroy()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose()
        {
            if (_channel == null)
            {
                return;
            }

            _channel.Dispose();
            _channel = null;
        }

        /// <summary>
        /// 发送网络包
        /// </summary>
        public override void SendPackage(INetPackage package)
        {
            _channel?.SendPackage(package);
        }

        /// <summary>
        /// 获取网络包
        /// </summary>
        public override INetPackage PickPackage()
        {
            return _channel?.PickPackage();
        }

        /// <summary>
        /// 检测Socket是否已连接
        /// </summary>
        public override bool IsConnected()
        {
            return _channel != null && _channel.IsConnected();
        }

        /// <summary>
        /// 异步连接
        /// </summary>
        public override void Connect(string ip, int port, Action<SocketError> callback)
        {
            UserToken token = new UserToken()
            {
                Callback = callback,
            };

            IPEndPoint remote = new IPEndPoint(IPAddress.Parse(ip), port);
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = remote;
            args.Completed += AcceptEventArg_Completed;
            args.UserToken = token;

            Socket clientSock = new Socket(remote.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            bool willRaiseEvent = clientSock.ConnectAsync(args);
            if (!willRaiseEvent)
            {
                ProcessConnected(args);
            }
        }

        /// <summary>
        /// 处理连接请求
        /// </summary>
        private void ProcessConnected(object obj)
        {
            SocketAsyncEventArgs e = obj as SocketAsyncEventArgs;
            UserToken token = (UserToken)e.UserToken;
            if (e.SocketError == SocketError.Success)
            {
                if (_channel != null)
                {
                    throw new Exception("TcpClient channel is created.");
                }

                // 创建频道
                _channel = new TcpChannel();
                _channel.InitChannel(_syncContext, e.ConnectSocket, _packageBodyMaxSize, _encoder, _decoder);
            }
            else
            {
                Log.Error($"Network connect error : {e.SocketError}");
            }

            // 回调函数		
            token.Callback?.Invoke(e.SocketError);
        }

        private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    _syncContext.Post(ProcessConnected, e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a connect");
            }
        }
    }
}