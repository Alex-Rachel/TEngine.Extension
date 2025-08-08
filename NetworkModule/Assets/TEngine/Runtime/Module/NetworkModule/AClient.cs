using System;
using System.Net.Sockets;

namespace TEngine
{
    /// <summary>
    /// 网络客户端抽象基类。定义网络连接的基本操作和生命周期管理。
    /// 继承IDisposable接口实现资源释放模式。
    /// </summary>
    public abstract class AClient : IDisposable
    {
        /// <summary>
        /// 当前网络的传输类型。
        /// </summary>
        public abstract NetworkType NetworkType { get; }

        /// <summary>
        /// 释放网络连接资源，断开连接并清理所有相关资源。
        /// 该方法在对象不再使用时由系统自动调用，也可手动调用以立即释放资源。
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// 内部销毁网络连接的方法。执行实际的Socket关闭和资源回收操作。
        /// </summary>
        internal abstract void Destroy();

        /// <summary>
        /// 网络模块的每帧更新方法。处理网络事件轮询、数据接收等实时逻辑。
        /// </summary>
        internal abstract void Update();

        /// <summary>
        /// 发送网络数据包。将协议对象序列化后通过网络传输。
        /// </summary>
        /// <param name="package">实现了INetPackage接口的网络包对象。</param>
        public abstract void SendPackage(INetPackage package);

        /// <summary>
        /// 获取网络包。
        /// </summary>
        public abstract INetPackage PickPackage();

        /// <summary>
        /// 检测底层Socket连接状态。返回当前是否保持有效连接。
        /// </summary>
        /// <returns>true表示连接有效，false表示已断开。</returns>
        public abstract bool IsConnected();

        /// <summary>
        /// 异步连接到指定服务器。采用回调机制通知连接结果。
        /// </summary>
        /// <param name="ip">目标服务器IP地址。</param>
        /// <param name="port">目标服务器监听端口。</param>
        /// <param name="callback">连接结果回调，参数为Socket错误码。</param>
        public abstract void Connect(string ip, int port, Action<SocketError> callback);
    }
}