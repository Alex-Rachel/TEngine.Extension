using System.Collections.Generic;

namespace GameLogic
{
    /// <summary>
    /// 输入系统核心模块接口。负责管理输入处理器和输入层的注册与分发。
    /// </summary>
    public interface IInputModule
    {
        /// <summary>
        /// 添加输入处理器实例。泛型T必须实现IInputHandler接口。
        /// </summary>
        /// <typeparam name="T">输入处理器类型。</typeparam>
        /// <returns>新创建的输入处理器实例。</returns>
        T AddInputHandler<T>() where T : class, IInputHandler, new();

        /// <summary>
        /// 移除指定类型的输入处理器。
        /// </summary>
        /// <typeparam name="T">要移除的输入处理器类型。</typeparam>
        void RemoveInputHandler<T>() where T : class, IInputHandler, new();

        /// <summary>
        /// 为指定实体添加输入层。泛型T必须实现IInputLayer接口。
        /// </summary>
        /// <typeparam name="T">输入层类型。</typeparam>
        /// <param name="EntityID">实体ID。</param>
        /// <returns>新创建的输入层实例。</returns>
        T AddInputLayer<T>(int EntityID) where T : class, IInputLayer, new();

        /// <summary>
        /// 移除指定实体的输入层。
        /// </summary>
        /// <typeparam name="T">要移除的输入层类型。</typeparam>
        /// <param name="EntityID">实体ID。</param>
        void RemoveInputLayer<T>(int EntityID) where T : class, IInputLayer, new();

        /// <summary>
        /// 获取指定实体的所有输入层。
        /// </summary>
        /// <param name="EntityID">实体ID。</param>
        /// <returns>输入层列表。</returns>
        List<IInputLayer> GetInputLayers(int EntityID);

        /// <summary>
        /// 接收来自引擎或者UI的动作输入。
        /// </summary>
        /// <param name="action">输入动作类型。</param>
        /// <param name="state">输入状态。</param>
        /// <param name="time">事件发生时间。</param>
        void SetInputAction(EInputAction action, EInputState state, double time);

        /// <summary>
        /// 接收来自引擎或者UI的轴输入。
        /// </summary>
        /// <param name="axis">输入轴类型。</param>
        /// <param name="value">轴输入值。</param>
        void SetInputAxis(EInputAxis axis, float value);

        /// <summary>
        /// 获取指定输入轴的当前值。
        /// </summary>
        /// <param name="axis">输入轴类型。</param>
        /// <returns>轴输入值。</returns>
        float GetInputAxis(EInputAxis axis);
    }
}
