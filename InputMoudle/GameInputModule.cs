using System.Collections.Generic;
using System.Linq;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 游戏输入模块。负责管理输入处理器和输入层，处理输入事件分发。
    /// </summary>
    public class GameInputModule : Module, IInputModule, IUpdateModule
    {
        // 输入处理器列表
        private List<IInputHandler> _inputHandlers = new List<IInputHandler>();

        // 实体ID到输入层的映射字典
        private Dictionary<int, List<IInputLayer>> _inputLayersDict = new Dictionary<int, List<IInputLayer>>();

        // 输入轴值字典
        private readonly Dictionary<EInputAxis, float> _inputAxisDict = new Dictionary<EInputAxis, float>();

        /// <summary>
        /// 模块初始化。注册输入动作并初始化数据结构。
        /// </summary>
        public override void OnInit()
        {
            InputDefine.RegisterInputActions();
            _inputHandlers = new List<IInputHandler>();
            _inputLayersDict = new Dictionary<int, List<IInputLayer>>();
        }

        /// <summary>
        /// 添加输入处理器实例。
        /// </summary>
        /// <typeparam name="T">输入处理器类型。</typeparam>
        /// <returns>新创建的输入处理器实例。</returns>
        public T AddInputHandler<T>() where T : class, IInputHandler, new()
        {
            var inputHandler = MemoryPool.Acquire<T>();
            _inputHandlers.Add(inputHandler);
            return inputHandler;
        }

        /// <summary>
        /// 移除指定类型的输入处理器。
        /// </summary>
        /// <typeparam name="T">要移除的输入处理器类型。</typeparam>
        public void RemoveInputHandler<T>() where T : class, IInputHandler, new()
        {
            for (int i = _inputHandlers.Count - 1; i >= 0; i--)
            {
                if (_inputHandlers[i] is T)
                {
                    var handlerToRemove = _inputHandlers[i];
                    _inputHandlers.RemoveAt(i);
                    MemoryPool.Release(handlerToRemove);
                    break;
                }
            }
        }

        /// <summary>
        /// 为指定实体添加输入层。
        /// </summary>
        /// <typeparam name="T">输入层类型。</typeparam>
        /// <param name="entityID">实体ID。</param>
        /// <returns>新创建的输入层实例。</returns>
        public T AddInputLayer<T>(int entityID) where T : class, IInputLayer, new()
        {
            var layer = MemoryPool.Acquire<T>();
            if (_inputLayersDict.TryGetValue(entityID, out var layers))
            {
                layers.Add(layer);
            }
            else
            {
                _inputLayersDict.Add(entityID, new List<IInputLayer>() { layer });
            }

            return layer;
        }

        /// <summary>
        /// 移除指定实体的输入层。
        /// </summary>
        /// <typeparam name="T">要移除的输入层类型。</typeparam>
        /// <param name="entityID">实体ID。</param>
        public void RemoveInputLayer<T>(int entityID) where T : class, IInputLayer, new()
        {
            if (_inputLayersDict.ContainsKey(entityID))
            {
                var layers = _inputLayersDict[entityID];
                for (int i = layers.Count - 1; i >= 0; i--)
                {
                    if (layers[i] is T)
                    {
                        var layerToRemove = layers[i];
                        layers.RemoveAt(i);
                        MemoryPool.Release(layerToRemove);
                        break;
                    }
                }

                if (layers.Count == 0)
                {
                    _inputLayersDict.Remove(entityID);
                }
            }
        }

        /// <summary>
        /// 获取指定实体的所有输入层，按优先级排序。
        /// 对于GetLayer时的优先级排序，建议在AddLayer的业务逻辑时候就已经按照优先级去Add
        /// 避免每帧进行优先级排序的性能浪费
        /// </summary>
        /// <param name="entityID">实体ID。</param>
        /// <returns>排序后的输入层列表。</returns>
        public List<IInputLayer> GetInputLayers(int entityID)
        {
            if (_inputLayersDict.TryGetValue(entityID, out var value))
            {
                return value;
            }
            return null;
        }

        /// <summary>
        /// 每帧更新。调用所有输入处理器的后处理方法。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间(秒)。</param>
        /// <param name="realElapseSeconds">真实流逝时间(秒)。</param>
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            foreach (IInputHandler inputHandler in _inputHandlers)
            {
                inputHandler.PostProcessInput(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        /// 接收来自引擎或者UI的动作输入。
        /// </summary>
        /// <param name="action">输入动作类型。</param>
        /// <param name="state">输入状态。</param>
        /// <param name="time">事件发生时间。</param>
        public void SetInputAction(EInputAction action, EInputState state, double time)
        {
            // 遍历所有输入处理器处理输入事件
            foreach (IInputHandler inputHandler in _inputHandlers)
            {
                inputHandler.HandleInputEvent(action, state, time);
            }
        }

        /// <summary>
        /// 接收来自引擎或者UI的轴输入。
        /// </summary>
        /// <param name="axis">输入轴类型。</param>
        /// <param name="value">轴输入值。</param>
        public void SetInputAxis(EInputAxis axis, float value)
        {
            _inputAxisDict[axis] = value;

            foreach (IInputHandler inputHandler in _inputHandlers)
            {
                inputHandler.HandleInputAxis(axis, value);
            }
        }

        /// <summary>
        /// 获取指定输入轴的当前值。
        /// </summary>
        /// <param name="axis">输入轴类型。</param>
        /// <returns>轴输入值。</returns>
        public float GetInputAxis(EInputAxis axis)
        {
            return _inputAxisDict.GetValueOrDefault(axis, 0);
        }

        /// <summary>
        /// 模块关闭。清理资源。
        /// </summary>
        public override void Shutdown()
        {
        }
    }
}