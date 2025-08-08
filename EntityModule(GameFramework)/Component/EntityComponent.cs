using System;
using System.Diagnostics;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 实体组件基类。
    /// </summary>
    public abstract class EntityComponent : IMemory
    {
        private EntityLogic _owner;

        public virtual int Priority => 0;

        public virtual bool NeedUpdate => false;

        /// <summary>
        /// 组件持有者。
        /// </summary>
        public EntityLogic Owner => _owner;

        protected internal void OnAwake(EntityLogic owner)
        {
            _owner = owner;
            OnAttach(_owner);
            AddDebug();
        }

        public void Clear()
        {
            OnDetach(_owner);
            ClearAllListener();
            RmvDebug();
            _owner = null;
        }

        /// <summary>
        /// 附加到实体上。
        /// <remarks>After Awake()</remarks>
        /// </summary>
        /// <param name="entityLogic">实体逻辑类。</param>
        protected virtual void OnAttach(EntityLogic entityLogic)
        {
        }

        /// <summary>
        /// 从实体上移除。
        /// <remarks>Before OnDestroy()</remarks>
        /// </summary>
        /// <param name="entityLogic">实体逻辑类。</param>
        protected virtual void OnDetach(EntityLogic entityLogic)
        {
        }

        /// <summary>
        /// 实体组件轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        protected internal virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        #region 事件相关。

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        /// <param name="eventCallback">事件回调。</param>
        public void AddEventListener(int eventId, Action eventCallback)
        {
            _owner.Event.AddEventListener(eventId, eventCallback, this);
        }

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        /// <param name="eventCallback">事件回调。</param>
        /// <typeparam name="TArg1">事件参数类型1。</typeparam>
        public void AddEventListener<TArg1>(int eventId, Action<TArg1> eventCallback)
        {
            _owner.Event.AddEventListener(eventId, eventCallback, this);
        }

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        /// <param name="eventCallback">事件回调。</param>
        /// <typeparam name="TArg1">事件参数类型1。</typeparam>
        /// <typeparam name="TArg2">事件参数类型2。</typeparam>
        public void AddEventListener<TArg1, TArg2>(int eventId, Action<TArg1, TArg2> eventCallback)
        {
            _owner.Event.AddEventListener(eventId, eventCallback, this);
        }

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        /// <param name="eventCallback">事件回调。</param>
        /// <typeparam name="TArg1">事件参数类型1。</typeparam>
        /// <typeparam name="TArg2">事件参数类型2。</typeparam>
        /// <typeparam name="TArg3">事件参数类型3。</typeparam>
        public void AddEventListener<TArg1, TArg2, TArg3>(int eventId, Action<TArg1, TArg2, TArg3> eventCallback)
        {
            _owner.Event.AddEventListener(eventId, eventCallback, this);
        }

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        /// <param name="eventCallback">事件回调。</param>
        /// <typeparam name="TArg1">事件参数类型1。</typeparam>
        /// <typeparam name="TArg2">事件参数类型2。</typeparam>
        /// <typeparam name="TArg3">事件参数类型3。</typeparam>
        /// <typeparam name="TArg4">事件参数类型4。</typeparam>
        public void AddEventListener<TArg1, TArg2, TArg3, TArg4>(int eventId, Action<TArg1, TArg2, TArg3, TArg4> eventCallback)
        {
            _owner.Event.AddEventListener(eventId, eventCallback, this);
        }

        /// <summary>
        /// 清除本组件所有事件监听。
        /// </summary>
        public void ClearAllListener()
        {
            if (_owner != null && _owner.Event != null)
            {
                _owner.Event.RemoveAllListenerByOwner(this);
            }
        }

        #endregion

        #region 编辑器Debug相关

        [Conditional("UNITY_EDITOR")]
        protected void AddDebug()
        {
#if UNITY_EDITOR
            if (_owner == null)
            {
                return;
            }

            var debugData = _owner.gameObject.GetComponent<EntityDebugBehaviour>();
            if (debugData == null)
            {
                debugData = _owner.gameObject.AddComponent<EntityDebugBehaviour>();
            }
            debugData.AddDebugCmpt(GetType().Name);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        protected void RmvDebug()
        {
#if UNITY_EDITOR
            if (_owner == null)
            {
                return;
            }

            var debugData = _owner.gameObject.GetComponent<EntityDebugBehaviour>();
            if (debugData == null)
            {
                debugData = _owner.gameObject.AddComponent<EntityDebugBehaviour>();
            }
            debugData.RmvDebugCmpt(GetType().Name);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public void SetDebugInfo(string key, string val)
        {
#if UNITY_EDITOR
            if (_owner == null)
            {
                return;
            }

            var debugData = _owner.gameObject.GetComponent<EntityDebugBehaviour>();
            if (debugData == null)
            {
                debugData = _owner.gameObject.AddComponent<EntityDebugBehaviour>();
            }
            debugData.SetDebugInfo(GetType().Name, key, val);
#endif
        }

        #endregion
    }
}