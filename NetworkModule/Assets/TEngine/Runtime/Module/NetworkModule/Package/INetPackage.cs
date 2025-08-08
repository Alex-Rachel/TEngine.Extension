namespace TEngine
{
    /// <summary>
    /// 网络错误处理委托。
    /// </summary>
    /// <param name="isDispose">是否需要释放。</param>
    /// <param name="error">错误描述信息。</param>
    public delegate void HandleErrorDelegate(bool isDispose, string error);

    /// <summary>
    /// 网络数据包基础接口。
    /// <para>功能说明：</para>
    /// <list type="bullet">
    /// <item>定义网络通信包的基础规范。</item>
    /// <item>通常用于协议解析和封包处理。</item>
    /// </list>
    /// </summary>
    public interface INetPackage
    {
    }
}