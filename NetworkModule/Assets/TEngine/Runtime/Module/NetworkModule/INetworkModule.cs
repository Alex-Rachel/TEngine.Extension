namespace TEngine
{
    public interface INetworkModule
    {
        /// <summary>
        /// 创建指定类型的网络客户端。
        /// </summary>
        /// <param name="networkType">网络类型。</param>
        /// <param name="packageBodyMaxSize">数据包最大尺寸。</param>
        /// <param name="encoder">网络包编码器。</param>
        /// <param name="decoder">网络包解码器。</param>
        /// <returns>创建的客户端实例。</returns>
        /// <exception cref="Exception">模块未初始化或非法网络类型时抛出异常。</exception>
        public AClient CreateNetworkClient(NetworkType networkType, int packageBodyMaxSize = 4096, INetPackageEncoder encoder = null, INetPackageDecoder decoder = null);

        /// <summary>
        /// 销毁指定的网络客户端。
        /// </summary>
        /// <param name="client">要销毁的客户端实例。</param>
        public void DestroyNetworkClient(AClient client);
    }
}