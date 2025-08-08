using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 输入处理接口。
    /// </summary>
    public interface IInputHandler : IMemory
    {
        /// <summary>
        /// 处理输入事件
        /// </summary>
        /// <param name="action">输入动作类型。</param>
        /// <param name="state">输入状态(按下/抬起等)。</param>
        /// <param name="time">事件发生时间。</param>
        void HandleInputEvent(EInputAction action, EInputState state, double time);

        /// <summary>
        /// 处理输入轴值变化。
        /// </summary>
        /// <param name="axis">输入轴类型。</param>
        /// <param name="value">轴值。</param>
        void HandleInputAxis(EInputAxis axis, float value);

        /// <summary>
        /// 获取输入轴当前值。
        /// </summary>
        /// <param name="axis">输入轴类型。</param>
        /// <returns>轴值。</returns>
        float GetInputAxis(EInputAxis axis);

        /// <summary>
        /// 输入后处理。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间(秒)。</param>
        /// <param name="realElapseSeconds">真实流逝时间(秒)。</param>
        void PostProcessInput(float elapseSeconds, float realElapseSeconds);
    }
}