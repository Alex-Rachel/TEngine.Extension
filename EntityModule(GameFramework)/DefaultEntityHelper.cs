using TEngine;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 默认实体辅助器。
    /// </summary>
    public class DefaultEntityHelper : EntityHelperBase
    {
        private IResourceModule _resourceModule = null;

        /// <summary>
        /// 实例化实体。
        /// </summary>
        /// <param name="entityAsset">要实例化的实体资源。</param>
        /// <returns>实例化后的实体。</returns>
        public override object InstantiateEntity(object entityAsset)
        {
            return Instantiate((Object)entityAsset);
        }

        /// <summary>
        /// 创建实体。
        /// </summary>
        /// <param name="entityInstance">实体实例。</param>
        /// <param name="entityGroup">实体所属的实体组。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>实体。</returns>
        public override IEntity CreateEntity(object entityInstance, IEntityGroup entityGroup, object userData)
        {
            GameObject entityGameObject = entityInstance as GameObject;
            if (entityGameObject == null)
            {
                Log.Error("Entity instance is invalid.");
                return null;
            }

            Transform entityTransform = entityGameObject.transform;
            entityTransform.SetParent(((MonoBehaviour)entityGroup.Helper).transform);

            var entity = entityGameObject.GetComponent<Entity>();
            if (entity == null)
            {
                entity = entityGameObject.AddComponent<Entity>();
            }
            return entity;
        }

        /// <summary>
        /// 释放实体。
        /// </summary>
        /// <param name="entityAsset">要释放的实体资源。</param>
        /// <param name="entityInstance">要释放的实体实例。</param>
        public override void ReleaseEntity(object entityAsset, object entityInstance)
        {
            Destroy((Object)entityInstance);
        }

        private void Start()
        {
            _resourceModule = ModuleSystem.GetModule<IResourceModule>();
            if (_resourceModule == null)
            {
                Log.Fatal("Resource component is invalid.");
                return;
            }
        }
    }
}
