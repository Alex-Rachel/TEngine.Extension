using System;
using System.Collections.Generic;

namespace TEngine
{
    /// <summary>
    /// 网络模块管理类。
    /// </summary>
    public class NetworkModule : Module, IUpdateModule, INetworkModule
    {
        // 模块初始化标志。
        private bool _isInitialize = false;

        // 存储所有网络客户端实例的列表。
        private readonly List<AClient> _networkClients = new List<AClient>();

        /// <summary>
        /// 初始化网络模块。创建网络驱动器并设置为场景切换不销毁。
        /// </summary>
        /// <exception cref="Exception">重复初始化时会抛出异常</exception>
        private void Initialize()
        {
            if (_isInitialize)
            {
                throw new Exception($"{nameof(NetworkModule)} is initialized !");
            }

            if (_isInitialize == false)
            {
                _isInitialize = true;
                Log.Info($"{nameof(NetworkModule)} initialize !");
            }
        }

        /// <summary>
        /// 销毁网络模块。释放所有网络客户端并清理资源。
        /// </summary>
        private void Destroy()
        {
            if (_isInitialize)
            {
                foreach (var client in _networkClients)
                {
                    client.Destroy();
                }

                _networkClients.Clear();

                _isInitialize = false;

                Log.Info($"{nameof(NetworkModule)} destroy all !");
            }
        }

        /// <summary>
        /// 创建指定类型的网络客户端。
        /// </summary>
        /// <param name="networkType">网络类型。</param>
        /// <param name="packageBodyMaxSize">数据包最大尺寸。</param>
        /// <param name="encoder">网络包编码器。</param>
        /// <param name="decoder">网络包解码器。</param>
        /// <returns>创建的客户端实例。</returns>
        /// <exception cref="Exception">模块未初始化或非法网络类型时抛出异常。</exception>
        public AClient CreateNetworkClient(NetworkType networkType, int packageBodyMaxSize = 4096, INetPackageEncoder encoder = null, INetPackageDecoder decoder = null)
        {
            if (_isInitialize == false)
            {
                throw new Exception($"{nameof(NetworkModule)} not initialized !");
            }

            encoder ??= new DefaultNetPackageEncoder();
            decoder ??= new DefaultNetPackageDecoder();

            AClient client = null;
            client = networkType switch
            {
                NetworkType.TCP => new TcpClient(packageBodyMaxSize, encoder, decoder),
                NetworkType.WebSocket => new WebSocketClient(packageBodyMaxSize, encoder, decoder),
                _ => throw new Exception($"Does not support network type : {networkType} !"),
            };
            _networkClients.Add(client);
            return client;
        }

        /// <summary>
        /// 销毁指定的网络客户端。
        /// </summary>
        /// <param name="client">要销毁的客户端实例。</param>
        public void DestroyNetworkClient(AClient client)
        {
            if (client == null)
            {
                return;
            }

            client.Dispose();
            _networkClients.Remove(client);
        }

        public override void OnInit()
        {
            Initialize();
        }

        public override void Shutdown()
        {
            Destroy();
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (_isInitialize)
            {
                foreach (var client in _networkClients)
                {
                    client.Update();
                }
            }
        }
    }
}