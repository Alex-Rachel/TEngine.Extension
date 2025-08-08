using TEngine;
using YooAsset;

namespace GameLogic
{
    internal sealed partial class EntityManager
    {
        /// <summary>
        /// 实体实例对象。
        /// </summary>
        private sealed class EntityInstanceObject : ObjectBase
        {
            private object _entityAsset;
            private IEntityHelper _entityHelper;

            public EntityInstanceObject()
            {
                _entityAsset = null;
                _entityHelper = null;
            }

            public static EntityInstanceObject Create(string name, object entityAsset, object entityInstance, IEntityHelper entityHelper)
            {
                if (entityAsset == null)
                {
                    throw new GameFrameworkException("Entity asset is invalid.");
                }

                if (entityHelper == null)
                {
                    throw new GameFrameworkException("Entity helper is invalid.");
                }

                EntityInstanceObject entityInstanceObject = MemoryPool.Acquire<EntityInstanceObject>();
                entityInstanceObject.Initialize(name, entityInstance);
                entityInstanceObject._entityAsset = entityAsset;
                entityInstanceObject._entityHelper = entityHelper;
                return entityInstanceObject;
            }

            public override void Clear()
            {
                base.Clear();
                _entityAsset = null;
                _entityHelper = null;
            }

            protected override void Release(bool isShutdown)
            {
                _entityHelper.ReleaseEntity(_entityAsset, Target);
            }
        }
    }
}
