using System.Collections.Generic;
using TEngine;

namespace GameLogic
{
    internal sealed partial class EntityManager
    {
        /// <summary>
        /// 实体组。
        /// </summary>
        private sealed class EntityGroup : IEntityGroup
        {
            private readonly string _name;
            private readonly IEntityGroupHelper _entityGroupHelper;
            private readonly IObjectPool<EntityInstanceObject> _instancePool;
            private readonly GameFrameworkLinkedList<IEntity> _entities;
            private LinkedListNode<IEntity> _cachedNode;

            /// <summary>
            /// 初始化实体组的新实例。
            /// </summary>
            /// <param name="name">实体组名称。</param>
            /// <param name="instanceAutoReleaseInterval">实体实例对象池自动释放可释放对象的间隔秒数。</param>
            /// <param name="instanceCapacity">实体实例对象池容量。</param>
            /// <param name="instanceExpireTime">实体实例对象池对象过期秒数。</param>
            /// <param name="instancePriority">实体实例对象池的优先级。</param>
            /// <param name="entityGroupHelper">实体组辅助器。</param>
            /// <param name="objectPoolManager">对象池管理器。</param>
            public EntityGroup(string name, float instanceAutoReleaseInterval, int instanceCapacity, float instanceExpireTime, int instancePriority, IEntityGroupHelper entityGroupHelper, IObjectPoolModule objectPoolManager)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new GameFrameworkException("Entity group name is invalid.");
                }

                if (entityGroupHelper == null)
                {
                    throw new GameFrameworkException("Entity group helper is invalid.");
                }

                _name = name;
                _entityGroupHelper = entityGroupHelper;
                _instancePool = objectPoolManager.CreateSingleSpawnObjectPool<EntityInstanceObject>(Utility.Text.Format("Entity Instance Pool ({0})", name), instanceCapacity, instanceExpireTime, instancePriority);
                _instancePool.AutoReleaseInterval = instanceAutoReleaseInterval;
                _entities = new GameFrameworkLinkedList<IEntity>();
                _cachedNode = null;
            }

            /// <summary>
            /// 获取实体组名称。
            /// </summary>
            public string Name => _name;

            /// <summary>
            /// 获取实体组中实体数量。
            /// </summary>
            public int EntityCount => _entities.Count;

            /// <summary>
            /// 获取或设置实体组实例对象池自动释放可释放对象的间隔秒数。
            /// </summary>
            public float InstanceAutoReleaseInterval
            {
                get => _instancePool.AutoReleaseInterval;
                set => _instancePool.AutoReleaseInterval = value;
            }

            /// <summary>
            /// 获取或设置实体组实例对象池的容量。
            /// </summary>
            public int InstanceCapacity
            {
                get => _instancePool.Capacity;
                set => _instancePool.Capacity = value;
            }

            /// <summary>
            /// 获取或设置实体组实例对象池对象过期秒数。
            /// </summary>
            public float InstanceExpireTime
            {
                get => _instancePool.ExpireTime;
                set => _instancePool.ExpireTime = value;
            }

            /// <summary>
            /// 获取或设置实体组实例对象池的优先级。
            /// </summary>
            public int InstancePriority
            {
                get => _instancePool.Priority;
                set => _instancePool.Priority = value;
            }

            /// <summary>
            /// 获取实体组辅助器。
            /// </summary>
            public IEntityGroupHelper Helper => _entityGroupHelper;

            /// <summary>
            /// 实体组轮询。
            /// </summary>
            /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
            /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                LinkedListNode<IEntity> current = _entities.First;
                while (current != null)
                {
                    _cachedNode = current.Next;
                    current.Value.OnUpdate(elapseSeconds, realElapseSeconds);
                    current = _cachedNode;
                    _cachedNode = null;
                }
            }

            /// <summary>
            /// 实体组中是否存在实体。
            /// </summary>
            /// <param name="entityId">实体序列编号。</param>
            /// <returns>实体组中是否存在实体。</returns>
            public bool HasEntity(int entityId)
            {
                foreach (IEntity entity in _entities)
                {
                    if (entity.Id == entityId)
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// 实体组中是否存在实体。
            /// </summary>
            /// <param name="entityAssetName">实体资源名称。</param>
            /// <returns>实体组中是否存在实体。</returns>
            public bool HasEntity(string entityAssetName)
            {
                if (string.IsNullOrEmpty(entityAssetName))
                {
                    throw new GameFrameworkException("Entity asset name is invalid.");
                }

                foreach (IEntity entity in _entities)
                {
                    if (entity.EntityAssetName == entityAssetName)
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// 从实体组中获取实体。
            /// </summary>
            /// <param name="entityId">实体序列编号。</param>
            /// <returns>要获取的实体。</returns>
            public IEntity GetEntity(int entityId)
            {
                foreach (IEntity entity in _entities)
                {
                    if (entity.Id == entityId)
                    {
                        return entity;
                    }
                }

                return null;
            }

            /// <summary>
            /// 从实体组中获取实体。
            /// </summary>
            /// <param name="entityAssetName">实体资源名称。</param>
            /// <returns>要获取的实体。</returns>
            public IEntity GetEntity(string entityAssetName)
            {
                if (string.IsNullOrEmpty(entityAssetName))
                {
                    throw new GameFrameworkException("Entity asset name is invalid.");
                }

                foreach (IEntity entity in _entities)
                {
                    if (entity.EntityAssetName == entityAssetName)
                    {
                        return entity;
                    }
                }

                return null;
            }

            /// <summary>
            /// 从实体组中获取实体。
            /// </summary>
            /// <param name="entityAssetName">实体资源名称。</param>
            /// <returns>要获取的实体。</returns>
            public IEntity[] GetEntities(string entityAssetName)
            {
                if (string.IsNullOrEmpty(entityAssetName))
                {
                    throw new GameFrameworkException("Entity asset name is invalid.");
                }

                List<IEntity> results = new List<IEntity>();
                foreach (IEntity entity in _entities)
                {
                    if (entity.EntityAssetName == entityAssetName)
                    {
                        results.Add(entity);
                    }
                }

                return results.ToArray();
            }

            /// <summary>
            /// 从实体组中获取实体。
            /// </summary>
            /// <param name="entityAssetName">实体资源名称。</param>
            /// <param name="results">要获取的实体。</param>
            public void GetEntities(string entityAssetName, List<IEntity> results)
            {
                if (string.IsNullOrEmpty(entityAssetName))
                {
                    throw new GameFrameworkException("Entity asset name is invalid.");
                }

                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                foreach (IEntity entity in _entities)
                {
                    if (entity.EntityAssetName == entityAssetName)
                    {
                        results.Add(entity);
                    }
                }
            }

            /// <summary>
            /// 从实体组中获取所有实体。
            /// </summary>
            /// <returns>实体组中的所有实体。</returns>
            public IEntity[] GetAllEntities()
            {
                List<IEntity> results = new List<IEntity>();
                foreach (IEntity entity in _entities)
                {
                    results.Add(entity);
                }

                return results.ToArray();
            }

            /// <summary>
            /// 从实体组中获取所有实体。
            /// </summary>
            /// <param name="results">实体组中的所有实体。</param>
            public void GetAllEntities(List<IEntity> results)
            {
                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                foreach (IEntity entity in _entities)
                {
                    results.Add(entity);
                }
            }

            /// <summary>
            /// 往实体组增加实体。
            /// </summary>
            /// <param name="entity">要增加的实体。</param>
            public void AddEntity(IEntity entity)
            {
                _entities.AddLast(entity);
            }

            /// <summary>
            /// 从实体组移除实体。
            /// </summary>
            /// <param name="entity">要移除的实体。</param>
            public void RemoveEntity(IEntity entity)
            {
                if (_cachedNode != null && _cachedNode.Value == entity)
                {
                    _cachedNode = _cachedNode.Next;
                }

                if (!_entities.Remove(entity))
                {
                    throw new GameFrameworkException(Utility.Text.Format("Entity group '{0}' not exists specified entity '[{1}]{2}'.", _name, entity.Id, entity.EntityAssetName));
                }
            }

            public void RegisterEntityInstanceObject(EntityInstanceObject obj, bool spawned)
            {
                _instancePool.Register(obj, spawned);
            }

            public EntityInstanceObject SpawnEntityInstanceObject(string name)
            {
                return _instancePool.Spawn(name);
            }

            public void UnspawnEntity(IEntity entity)
            {
                _instancePool.Unspawn(entity.Handle);
            }

            public void SetEntityInstanceLocked(object entityInstance, bool locked)
            {
                if (entityInstance == null)
                {
                    throw new GameFrameworkException("Entity instance is invalid.");
                }

                _instancePool.SetLocked(entityInstance, locked);
            }

            public void SetEntityInstancePriority(object entityInstance, int priority)
            {
                if (entityInstance == null)
                {
                    throw new GameFrameworkException("Entity instance is invalid.");
                }

                _instancePool.SetPriority(entityInstance, priority);
            }
        }
    }
}
