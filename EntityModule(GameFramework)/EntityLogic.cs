using System;
using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 实体逻辑基类。
    /// </summary>
    public abstract class EntityLogic : MonoBehaviour
    {
        private bool _available = false;
        private bool _visible = false;
        private Entity _entity = null;
        private Transform _cachedTransform = null;
        private int _originalLayer = 0;
        private Transform _originalTransform = null;

        /// <summary>
        /// 获取实体。
        /// </summary>
        public Entity Entity => _entity;

        /// <summary>
        /// 获取或设置实体名称。
        /// </summary>
        public string Name
        {
            get => gameObject.name;
            set => gameObject.name = value;
        }

        /// <summary>
        /// 获取实体是否可用。
        /// </summary>
        public bool Available => _available;

        /// <summary>
        /// 获取或设置实体是否可见。
        /// </summary>
        public bool Visible
        {
            get => _available && _visible;
            set
            {
                if (!_available)
                {
                    Log.Warning("Entity '{0}' is not available.", Name);
                    return;
                }

                if (_visible == value)
                {
                    return;
                }

                _visible = value;
                InternalSetVisible(value);
            }
        }

        /// <summary>
        /// 获取已缓存的 Transform。
        /// </summary>
        public Transform CachedTransform => _cachedTransform;
        
        private ActorEventDispatcher _event;

        /// <summary>
        /// 事件分发器。
        /// </summary>
        public ActorEventDispatcher Event => _event ??= MemoryPool.Acquire<ActorEventDispatcher>();

        /// <summary>
        /// 实体初始化。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnInit(object userData)
        {
            if (_cachedTransform == null)
            {
                _cachedTransform = transform;
            }

            _entity = GetComponent<Entity>();
            _originalLayer = gameObject.layer;
            _originalTransform = CachedTransform.parent;
        }

        /// <summary>
        /// 实体回收。
        /// </summary>
        protected internal virtual void OnRecycle()
        {
        }

        /// <summary>
        /// 实体显示。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnShow(object userData)
        {
            _available = true;
            Visible = true;
        }

        /// <summary>
        /// 实体隐藏。
        /// </summary>
        /// <param name="isShutdown">是否是关闭实体管理器时触发。</param>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnHide(bool isShutdown, object userData)
        {
            if (_event != null)
            {
                MemoryPool.Release(_event);
                _event = null;
            }
            if (gameObject != null)
            {
                // gameObject.SetLayerRecursively(_originalLayer);
            }
            Visible = false;
            _available = false;
            var iter = _componentMap.GetEnumerator();
            while (iter.MoveNext())
            {
                EntityComponent component = iter.Current.Value;
                if (component == null)
                {
                    continue;
                }

                if (gameObject != null)
                {
                    MemoryPool.Release(component);
                }
            }
            iter.Dispose();
            _componentMap.Clear();
            _updateComponents.Clear();
        }

        /// <summary>
        /// 实体附加子实体。
        /// </summary>
        /// <param name="childEntity">附加的子实体。</param>
        /// <param name="parentTransform">被附加父实体的位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnAttached(EntityLogic childEntity, Transform parentTransform, object userData)
        {
        }

        /// <summary>
        /// 实体解除子实体。
        /// </summary>
        /// <param name="childEntity">解除的子实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnDetached(EntityLogic childEntity, object userData)
        {
        }

        /// <summary>
        /// 实体附加子实体。
        /// </summary>
        /// <param name="parentEntity">被附加的父实体。</param>
        /// <param name="parentTransform">被附加父实体的位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnAttachTo(EntityLogic parentEntity, Transform parentTransform, object userData)
        {
            CachedTransform.SetParent(parentTransform);
        }

        /// <summary>
        /// 实体解除子实体。
        /// </summary>
        /// <param name="parentEntity">被解除的父实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnDetachFrom(EntityLogic parentEntity, object userData)
        {
            CachedTransform.SetParent(_originalTransform);
        }

        /// <summary>
        /// 实体轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        protected virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 实体轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal void OnExecuteUpdate(float elapseSeconds, float realElapseSeconds)
        {
            OnUpdate(elapseSeconds, realElapseSeconds);
            var iter = _updateComponents.GetEnumerator();
            while (iter.MoveNext())
            {
                var current = iter.Current;
                if (current != null)
                {
                    current.OnUpdate(elapseSeconds, realElapseSeconds);
                }
            }
            iter.Dispose();
        }

        /// <summary>
        /// 设置实体的可见性。
        /// </summary>
        /// <param name="visible">实体的可见性。</param>
        protected virtual void InternalSetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private readonly Dictionary<Type,EntityComponent> _componentMap = new Dictionary<Type,EntityComponent>();
        private static readonly GameFrameworkLinkedList<EntityComponent> _updateComponents = new GameFrameworkLinkedList<EntityComponent>();

        /// <summary>
        /// 添加实体组件。
        /// </summary>
        /// <typeparam name="T">实体组件类型。</typeparam>
        /// <returns>实体组件。</returns>
        public T AddEntityComponent<T>() where T : EntityComponent
        {
            T ret = MemoryPool.Acquire(typeof(T)) as T;

            if (ret == null)
            {
                Log.Fatal($"add entityComponent failed");
                return null;
            }
            
            _componentMap.Add(typeof(T),ret);

            if (ret.NeedUpdate)
            {
                LinkedListNode<EntityComponent> current = _updateComponents.First;
                while (current != null)
                {
                    if (ret.Priority > current.Value.Priority)
                    {
                        break;
                    }

                    current = current.Next;
                }

                if (current != null)
                {
                    _updateComponents.AddBefore(current, ret);
                }
                else
                {
                    _updateComponents.AddLast(ret);
                }
            }
            
            ret?.OnAwake(this);
            
            return ret;
        }

        /// <summary>
        /// 获取实体组件。
        /// </summary>
        /// <typeparam name="T">实体组件类型。</typeparam>
        /// <returns>实体组件。</returns>
        public T GetEntityComponent<T>() where T : EntityComponent
        {
            _componentMap.TryGetValue(typeof(T), out EntityComponent component);
            return component as T;
        }
        
        /// <summary>
        /// 获取或者添加实体组件。
        /// </summary>
        /// <typeparam name="T">实体组件类型。</typeparam>
        /// <returns>实体组件。</returns>
        public T GetOrAddEntityComponent<T>() where T : EntityComponent
        {
            T ret = GetEntityComponent<T>() ?? AddEntityComponent<T>();
            return ret;
        }
        
        /// <summary>
        /// 移除实体组件。
        /// </summary>
        /// <typeparam name="T">实体组件类型。</typeparam>
        /// <returns>实体组件。</returns>
        public void RemoveEntityComponent<T>() where T : EntityComponent
        {
            Type key = typeof(T);
            RemoveEntityComponentImp(key);
        }
        
        /// <summary>
        /// 移除实体组件。
        /// </summary>
        /// <param name="component">实体组件。</param>
        /// <returns>实体组件。</returns>
        public void RemoveEntityComponent(EntityComponent component)
        {
            Type key = component.GetType();
            RemoveEntityComponentImp(key);
        }

        private void RemoveEntityComponentImp(Type type)
        {
            if (_componentMap.ContainsKey(type))
            {
                EntityComponent component = _componentMap[type];
                if (component.NeedUpdate)
                {
                    _updateComponents.Remove(component);
                }
                MemoryPool.Release(component);
                _componentMap.Remove(type);
            }
        }
    }
}
