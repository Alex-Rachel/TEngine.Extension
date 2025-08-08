using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 输入层级接口。定义输入事件的分层处理机制。
    /// </summary>
    public interface IInputLayer : IMemory
    {
        /// <summary>
        /// 获取当前输入层的优先级。数值越大优先级越高。
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 处理按下输入事件。返回输入命令。
        /// </summary>
        /// <returns>处理后的输入命令。</returns>
        SInputCommand HandlePress();

        /// <summary>
        /// 处理释放输入事件。返回输入命令。
        /// </summary>
        /// <returns>处理后的输入命令。</returns>
        SInputCommand HandleRelease();

        /// <summary>
        /// 处理持续按住输入事件。返回输入命令。
        /// </summary>
        /// <returns>处理后的输入命令。</returns>
        SInputCommand HandleHold();

        /// <summary>
        /// 处理按下输入事件。不返回命令。
        /// </summary>
        void HandlePressEvent();

        /// <summary>
        /// 处理释放输入事件。不返回命令。
        /// </summary>
        void HandleReleaseEvent();

        /// <summary>
        /// 处理持续按住输入事件。不返回命令。
        /// </summary>
        void HandleHoldEvent();
    }
}