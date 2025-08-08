namespace TEngine
{
    /// <summary>
    /// 默认网络数据包实现。
    /// 用于在网络通信中封装基础消息结构。
    /// </summary>
    /// <remarks>
    /// 实现 <see cref="INetPackage"/> 接口，提供消息ID与二进制载荷的标准容器。
    /// 通常配合编解码器（Encoder/Decoder）进行序列化与反序列化操作。
    /// </remarks>
    public class DefaultNetPackage : INetPackage
    {
        /// <summary>
        /// 消息唯一标识符。
        /// <para>整型消息ID，用于：</para>
        /// <list type="bullet">
        /// <item>1. 标识消息类型（如：1001=登录消息）。</item>
        /// <item>2. 接收方根据ID选择对应的消息序列化。</item>
        /// </list>
        /// </summary>
        public int MsgID { set; get; }

        /// <summary>
        /// 消息体原始字节数据。
        /// </summary>
        public byte[] BodyBytes { set; get; }
    }
}