using UnityEngine;
using UnityEngine.InputSystem.Interactions;

namespace GameLogic
{
    /// <summary>
    /// 输入动作枚举。定义游戏中的基础输入动作类型。
    /// </summary>
    public enum EInputAction
    {
        Attack, // 攻击动作
        Skill, // 技能释放
        EnhanceSkill, // 强化技能
        Sprint, // 冲刺
        Jump, // 跳跃
    }

    /// <summary>
    /// 输入轴枚举。定义游戏中的输入轴类型。
    /// </summary>
    public enum EInputAxis
    {
        MoveForward, // 前后移动轴
        MoveRight, // 左右移动轴
        LookUp, // 视角上下轴
        LookRight, // 视角左右轴
        ZoomUp, // 镜头拉近
        ZoomDown, // 镜头拉远
    }

    /// <summary>
    /// 输入状态枚举。定义输入的不同状态。
    /// </summary>
    public enum EInputState
    {
        Press, // 按下状态
        Hold, // 按住状态
        Release, // 释放状态
    }

    /// <summary>
    /// 输入命令类型枚举。定义输入转换后的具体命令类型。
    /// </summary>
    public enum EInputCommandType
    {
        None, // 无命令
        Skill, // 技能命令
        Jump, // 跳跃命令
        Sprint, // 冲刺命令
        // 其他具体行为...
    }

    /// <summary>
    /// 输入事件结构体。表示最上游的原始输入事件。
    /// </summary>
    public struct InputEvent
    {
        public InputEvent(EInputAction action, EInputState state, double time)
        {
            Action = action;
            State = state;
            Time = time;
        }

        public EInputAction Action; // 输入动作类型
        public EInputState State; // 输入状态
        public double Time; // 事件发生时间
    }

    /// <summary>
    /// 输入命令结构体。表示最下游的具体执行命令。
    /// </summary>
    public struct SInputCommand
    {
        public SInputCommand(EInputCommandType commandType, int intValue, int tagValue)
        {
            CommandType = commandType;
            IntValue = intValue;
            TagValue = tagValue;
        }

        public EInputCommandType CommandType; // 命令类型

        public int IntValue; // 整数值，如技能ID等
        public int TagValue; // 标签值，如玩家状态标签等
    }

    /// <summary>
    /// 输入命令结构体。表示由输入层转换后的中间命令。
    /// </summary>
    public struct InputCommand
    {
        public InputCommand(EInputAction action, EInputState state, SInputCommand command, double time, int index)
        {
            Action = action;
            State = state;
            Command = command;
            Time = time;
            Index = index;
        }

        public EInputAction Action; // 原始输入动作
        public EInputState State; // 输入状态
        public double Time; // 事件时间
        public SInputCommand Command; // 转换后的命令
        public int Index; // 命令索引
    }

    /// <summary>
    /// 输入缓存结构体。用于临时存储输入事件。
    /// </summary>
    public struct InputCache
    {
        public InputCache(EInputAction action, EInputState state, double eventTime, double time, double accumulateTime)
        {
            Action = action;
            State = state;
            EventTime = eventTime;
            Time = time;
            AccumulateTime = accumulateTime;
        }

        public EInputAction Action; // 缓存的动作类型
        public EInputState State; // 缓存的状态
        public double EventTime; // 原始事件时间
        public double Time; // 缓存创建时间
        public double AccumulateTime; // 缓存累计时间
    }

    /// <summary>
    /// 输入定义静态类。负责注册和管理输入系统。
    /// </summary>
    public static class InputDefine
    {
        private static GameInputActions _inputActions; // Unity输入系统实例

        /// <summary>
        /// 注册所有输入动作的回调。
        /// </summary>
        public static void RegisterInputActions()
        {
            _inputActions = new GameInputActions();
            _inputActions.Enable();

            // 攻击输入处理
            _inputActions.GamePlay.Attack.started += (ctx) => { GameModule.Input.SetInputAction(EInputAction.Attack, EInputState.Press, ctx.time); };
            _inputActions.GamePlay.Attack.performed += (ctx) =>
            {
                if (ctx.interaction is HoldInteraction)
                {
                    GameModule.Input.SetInputAction(EInputAction.Attack, EInputState.Hold, ctx.time);
                }
            };
            _inputActions.GamePlay.Attack.canceled += (ctx) => { GameModule.Input.SetInputAction(EInputAction.Attack, EInputState.Release, ctx.time); };

            // 技能输入处理
            _inputActions.GamePlay.Skill.started += (ctx) => { GameModule.Input.SetInputAction(EInputAction.Skill, EInputState.Press, ctx.time); };
            _inputActions.GamePlay.Skill.performed += (ctx) =>
            {
                if (ctx.interaction is HoldInteraction)
                {
                    GameModule.Input.SetInputAction(EInputAction.Skill, EInputState.Hold, ctx.time);
                }
            };
            _inputActions.GamePlay.Skill.canceled += (ctx) => { GameModule.Input.SetInputAction(EInputAction.Skill, EInputState.Release, ctx.time); };

            // 强化技能输入处理
            _inputActions.GamePlay.EnhanceSkill.started += (ctx) => { GameModule.Input.SetInputAction(EInputAction.EnhanceSkill, EInputState.Press, ctx.time); };
            _inputActions.GamePlay.EnhanceSkill.canceled += (ctx) => { GameModule.Input.SetInputAction(EInputAction.EnhanceSkill, EInputState.Release, ctx.time); };
            _inputActions.GamePlay.EnhanceSkill.performed += (ctx) =>
            {
                if (ctx.interaction is HoldInteraction)
                {
                    GameModule.Input.SetInputAction(EInputAction.EnhanceSkill, EInputState.Hold, ctx.time);
                }
            };

            // 移动输入处理
            _inputActions.GamePlay.Move.performed += (ctx) =>
            {
                GameModule.Input.SetInputAxis(EInputAxis.MoveForward, ctx.ReadValue<Vector2>().y);
                GameModule.Input.SetInputAxis(EInputAxis.MoveRight, ctx.ReadValue<Vector2>().x);
            };
            _inputActions.GamePlay.Move.canceled += (ctx) =>
            {
                GameModule.Input.SetInputAxis(EInputAxis.MoveForward, 0);
                GameModule.Input.SetInputAxis(EInputAxis.MoveRight, 0);
            };

            // 视角输入处理
            _inputActions.GamePlay.Look.performed += (ctx) =>
            {
                GameModule.Input.SetInputAxis(EInputAxis.LookUp, ctx.ReadValue<Vector2>().y);
                GameModule.Input.SetInputAxis(EInputAxis.LookRight, ctx.ReadValue<Vector2>().x);
            };
            _inputActions.GamePlay.Look.canceled += (ctx) =>
            {
                GameModule.Input.SetInputAxis(EInputAxis.LookUp, 0);
                GameModule.Input.SetInputAxis(EInputAxis.LookRight, 0);
            };
        }
    }
}