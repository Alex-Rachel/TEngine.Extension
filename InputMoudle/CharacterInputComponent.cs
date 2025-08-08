using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 角色输入组件，负责处理输入事件。
    /// </summary>
    public class CharacterInputComponent : IInputHandler
    {
        /// <summary>
        /// TODO: 临时用，到时候请换上自己的Entity的ID
        /// </summary>
        public int EntityID;

        /// <summary>
        /// TODO: 临时用，每个角色身上的输入行为过期时间配置，到时候换上自己的
        /// </summary>
        public readonly Dictionary<(EInputAction Action, EInputState State), double> PlayerInputConfig = new Dictionary<(EInputAction, EInputState), double>();

        /// <summary>
        /// TODO: 临时用，每个角色身上的输入行为优先级配置，到时候换上自己的
        /// </summary>
        public readonly Dictionary<(EInputAction Action, EInputState State), int> PlayerInputPriorityConfig = new Dictionary<(EInputAction, EInputState), int>();

        private readonly List<InputEvent> _inputEvents = new List<InputEvent>();
        private readonly List<InputCache> _inputCaches = new List<InputCache>();

        private readonly List<InputCommand> _inputCommands = new List<InputCommand>();

        /// <summary>
        /// 轴输入缓存。
        /// <remarks>关于轴输入很重要的一点是，轴缓存机制与行为缓存机制不同，一般来说3C中直接接收轴的值来驱动，不需要经过layer，轴输入和行为输入是一个分界线。</remarks>
        /// </summary>
        private readonly Dictionary<EInputAxis, float> _inputAxisDict = new Dictionary<EInputAxis, float>();

        /// <summary>
        /// 轴输入缓存。
        /// </summary>
        private struct AxisCache
        {
            /// <summary>
            /// 最后非零值。
            /// </summary>
            public float lastNonZeroValue;
            
            /// <summary>
            /// 累计时间。
            /// </summary>
            public float accumulateTime;

            public AxisCache(float value, float time)
            {
                lastNonZeroValue = value;
                accumulateTime = time;
            }
        }

        private readonly Dictionary<EInputAxis, AxisCache> _inputAxisCacheDict = new Dictionary<EInputAxis, AxisCache>();
        private const float AXIS_CACHE_DURATION = 0.1f; // 100毫秒缓存时间

        /// <summary>
        /// 更新轴输入缓存。
        /// </summary>
        /// <param name="action">输入动作枚举。</param>
        /// <param name="state">输入状态枚举。</param>
        /// <param name="time">输入时长。</param>
        public void HandleInputEvent(EInputAction action, EInputState state, double time)
        {
            _inputEvents.Add(new InputEvent(action, state, time));
            var layers = GameModule.Input.GetInputLayers(EntityID);
            switch (state)
            {
                case EInputState.Press:
                    for (int i = 0; i < layers.Count; i++)
                    {
                        layers[i].HandlePressEvent();
                    }

                    break;
                case EInputState.Hold:
                    for (int i = 0; i < layers.Count; i++)
                    {
                        layers[i].HandleHoldEvent();
                    }

                    break;
                case EInputState.Release:
                    for (int i = 0; i < layers.Count; i++)
                    {
                        layers[i].HandleReleaseEvent();
                    }

                    break;
            }
        }

        /// <summary>
        /// 更新轴输入缓存。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间(秒)。</param>
        /// <param name="realElapseSeconds">真实流逝时间(秒)。</param>
        public void PostProcessInput(float elapseSeconds, float realElapseSeconds)
        {
            //更新轴输入缓存
            UpdateMoveCache(realElapseSeconds);

            ValidateInputCaches();

            //为输入缓存累积时间
            for (int i = 0; i < _inputCaches.Count; i++)
            {
                var cache = _inputCaches[i];
                cache.AccumulateTime += realElapseSeconds;
                _inputCaches[i] = cache;
            }

            _inputCommands.Clear();

            //先处理输入缓存
            for (int i = 0; i < _inputCaches.Count; i++)
            {
                var inputCache = _inputCaches[i];
                var command = GetCommand(inputCache.Action, inputCache.State, inputCache.Time);
                if (command.Command.CommandType != EInputCommandType.None)
                {
                    _inputCommands.Add(command);
                }
            }

            //处理输入Event
            for (int i = 0; i < _inputEvents.Count; i++)
            {
                var inputEvent = _inputEvents[i];
                var command = GetCommand(inputEvent.Action, inputEvent.State, inputEvent.Time);
                if (command.Command.CommandType != EInputCommandType.None)
                {
                    _inputCommands.Add(command);
                }
            }

            var bestCommand = GetBestCommand(_inputCommands);
            _inputCommands.Remove(bestCommand);
            _inputEvents.Clear();

            CacheInput(_inputCommands);

            ExecuteInputCommand(bestCommand);
        }

        // TODO : Remark
        //每个Entity可以有多个layer来处理相同按键但是不同行为，并且可以配置layer优先级，比如按键E可以是技能可以是交互，
        //处理交互layer的优先级大于战斗layer时，一旦识别到附近有可交互物体，交互layer返回command，并跳出循环不再进行遍历
        //一个Event只能对应一个Command
        private InputCommand GetCommand(EInputAction action, EInputState state, double time)
        {
            var layers = GameModule.Input.GetInputLayers(EntityID);
            for (int i = 0; i < layers.Count; i++)
            {
                switch (state)
                {
                    case EInputState.Press:
                        var commandPress = layers[i].HandlePress();
                        if (commandPress.CommandType != EInputCommandType.None)
                        {
                            return new InputCommand(action, state, commandPress, time, i);
                        }

                        break;
                    case EInputState.Hold:
                        var commandHold = layers[i].HandleHold();
                        if (commandHold.CommandType != EInputCommandType.None)
                        {
                            return new InputCommand(action, state, commandHold, time, i);
                        }

                        break;
                    case EInputState.Release:
                        var commandRelease = layers[i].HandleRelease();
                        if (commandRelease.CommandType != EInputCommandType.None)
                        {
                            return new InputCommand(action, state, commandRelease, time, i);
                        }

                        break;
                }
            }

            return new InputCommand(action, state, new SInputCommand(EInputCommandType.None, 0, 0), time, 0);
        }

        //清理过期缓存
        private void ValidateInputCaches()
        {
            for (int i = _inputCaches.Count - 1; i >= 0; i--)
            {
                var cache = _inputCaches[i];
                if (cache.AccumulateTime > PlayerInputConfig[(cache.Action, cache.State)])
                {
                    _inputCaches.RemoveAt(i);
                }
            }
        }

        private InputCommand GetBestCommand(List<InputCommand> commands)
        {
            var bestCommand = commands[0];
            for (int i = 1; i < commands.Count; i++)
            {
                if (PlayerInputPriorityConfig[(commands[i].Action, commands[i].State)] >
                    PlayerInputPriorityConfig[(bestCommand.Action, bestCommand.State)])
                {
                    bestCommand = commands[i];
                }
            }

            return bestCommand;
        }

        public void CacheInput(List<InputCommand> inputCommands)
        {
            foreach (var command in inputCommands)
            {
                //第二个Time为当前世界时间戳
                _inputCaches.Add(new InputCache(command.Action, command.State, command.Time, DateTime.UtcNow.Ticks, 0));
            }
        }

        public void HandleInputAxis(EInputAxis axis, float value)
        {
            _inputAxisDict[axis] = value;

            // 如果输入值非零，记录到缓存中
            if (Mathf.Abs(value) > 0.001f) // 使用小阈值避免浮点精度问题
            {
                if (_inputAxisCacheDict.ContainsKey(axis))
                {
                    var cache = _inputAxisCacheDict[axis];
                    _inputAxisCacheDict[axis] = new AxisCache(value, cache.accumulateTime);
                }
                else
                {
                    _inputAxisCacheDict.Add(axis, new AxisCache(value, 0f));
                }
            }
        }

        /// <summary>
        /// 更新轴缓存并维护_inputAxisDict
        /// </summary>
        private void UpdateMoveCache(float deltaTime)
        {
            var keysToRemove = new List<EInputAxis>();

            foreach (var kvp in _inputAxisCacheDict)
            {
                var axis = kvp.Key;
                var cache = kvp.Value;

                // 累积时间
                var newCache = new AxisCache(cache.lastNonZeroValue, cache.accumulateTime + deltaTime);
                _inputAxisCacheDict[axis] = newCache;

                // 检查当前轴的实际输入值
                float currentValue = 0;
                if (_inputAxisDict.TryGetValue(axis, out var value))
                {
                    currentValue = value;
                }

                // 如果当前值为零且缓存未过期，在字典中保持方向值
                if (Mathf.Abs(currentValue) < 0.001f && newCache.accumulateTime <= AXIS_CACHE_DURATION)
                {
                    _inputAxisDict[axis] = Mathf.Sign(cache.lastNonZeroValue);
                }
                // 如果缓存过期，清理
                else if (newCache.accumulateTime > AXIS_CACHE_DURATION)
                {
                    keysToRemove.Add(axis);
                    if (_inputAxisDict.ContainsKey(axis) && Mathf.Abs(_inputAxisDict[axis]) < 0.001f)
                    {
                        _inputAxisDict[axis] = 0;
                    }
                }
            }

            // 清理过期缓存
            foreach (var key in keysToRemove)
            {
                _inputAxisCacheDict.Remove(key);
            }
        }

        public float GetInputAxis(EInputAxis axis)
        {
            return _inputAxisDict.GetValueOrDefault(axis, 0);
        }

        public void ExecuteInputCommand(InputCommand command)
        {
            switch (command.Command.CommandType)
            {
                case EInputCommandType.Skill:
                    //根据返回的技能ID执行技能
                    //SkillComponent.StartSkill(command.Command.IntValue);
                    break;
                case EInputCommandType.Jump:
                    //MoveComponent.Jump();
                    break;
                case EInputCommandType.Sprint:
                    //MoveComponent.Sprint();
                    break;
                case EInputCommandType.None:
                    break;
            }
        }

        public void Clear()
        {
            _inputEvents.Clear();
            _inputCaches.Clear();
            _inputAxisCacheDict.Clear();
        }
    }
}