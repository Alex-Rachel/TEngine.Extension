using System.Collections.Generic;
using TEngine;

namespace GameLogic
{
    internal sealed partial class EntityManager
    {
        /// <summary>
        /// 实体信息。
        /// </summary>
        private sealed class EntityInfo : IMemory
        {
            private IEntity _entity;
            private EntityStatus _status;
            private IEntity _parentEntity;
            private readonly List<IEntity> _childEntities;

            public EntityInfo()
            {
                _entity = null;
                _status = EntityStatus.Unknown;
                _parentEntity = null;
                _childEntities = new List<IEntity>();
            }

            public IEntity Entity => _entity;

            public EntityStatus Status
            {
                get => _status;
                set => _status = value;
            }

            public IEntity ParentEntity
            {
                get => _parentEntity;
                set => _parentEntity = value;
            }

            public int ChildEntityCount => _childEntities.Count;

            public static EntityInfo Create(IEntity entity)
            {
                if (entity == null)
                {
                    throw new GameFrameworkException("Entity is invalid.");
                }

                EntityInfo entityInfo = MemoryPool.Acquire<EntityInfo>();
                entityInfo._entity = entity;
                entityInfo._status = EntityStatus.WillInit;
                return entityInfo;
            }

            public void Clear()
            {
                _entity = null;
                _status = EntityStatus.Unknown;
                _parentEntity = null;
                _childEntities.Clear();
            }

            public IEntity GetChildEntity()
            {
                return _childEntities.Count > 0 ? _childEntities[0] : null;
            }

            public IEntity[] GetChildEntities()
            {
                return _childEntities.ToArray();
            }

            public void GetChildEntities(List<IEntity> results)
            {
                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                foreach (IEntity childEntity in _childEntities)
                {
                    results.Add(childEntity);
                }
            }

            public void AddChildEntity(IEntity childEntity)
            {
                if (_childEntities.Contains(childEntity))
                {
                    throw new GameFrameworkException("Can not add child entity which is already exist.");
                }

                _childEntities.Add(childEntity);
            }

            public void RemoveChildEntity(IEntity childEntity)
            {
                if (!_childEntities.Remove(childEntity))
                {
                    throw new GameFrameworkException("Can not remove child entity which is not exist.");
                }
            }
        }
    }
}
