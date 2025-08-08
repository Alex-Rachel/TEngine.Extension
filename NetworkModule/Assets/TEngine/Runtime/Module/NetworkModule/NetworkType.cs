namespace TEngine
{
    /// <summary>
    /// 网络传输协议类型枚举
    /// <para>用于配置项目使用的网络通信协议类型</para>
    /// </summary>
    public enum NetworkType
    {
        /// <summary>
        /// 传输控制协议（面向连接，可靠传输）
        /// <para>- 特点：保证数据顺序和完整性，三次握手建立连接</para>
        /// <para>- 适用场景：文件传输、网页浏览等可靠性要求高的场景</para>
        /// </summary>
        TCP,
        
        /// <summary>
        /// 用户数据报协议（无连接，高效传输）
        /// <para>- 特点：尽最大努力交付，不保证顺序和可靠性</para>
        /// <para>- 适用场景：实时游戏、视频流媒体等低延迟场景</para>
        /// </summary>
        UDP,
        
        /// <summary>
        /// 快速可靠协议（基于UDP的可靠传输）
        /// <para>- 特点：结合TCP可靠性和UDP高效性，ARQ重传机制</para>
        /// <para>- 适用场景：MOBA游戏、实时对战等需要快速响应的场景</para>
        /// </summary>
        KCP,
        
        /// <summary>
        /// WebSocket协议（全双工通信）
        /// <para>- 特点：基于TCP，HTTP握手后保持长连接</para>
        /// <para>- 适用场景：网页即时通讯、实时数据推送等场景</para>
        /// </summary>
        WebSocket
    }
}