using TEngine;

namespace GameLogic
{
    public static class EntityExtension
    {
        private static int _serialId = 0;

        /// <summary>
        /// 生成客户端序列化ID/EntityID，服务器默认不用
        /// </summary>
        /// <param name="entitySystem"></param>
        /// <returns></returns>
        public static int GenerateSerialId(this EntityModule entityModule)
        {
            return ++_serialId;
        }

        public static void CreateEntity<T>(this EntityModule entityModule, string assetPath,string entityGroup,
            EntityData userData = null, float autoReleaseInterval = 60f,
            int capacity = 60, float expireTime = 60f, int priority = 0)
        {
            entityModule.CreateEntity(typeof(T), assetPath,entityGroup, userData, autoReleaseInterval, capacity, expireTime,
                priority);
        }
        public static void CreateEntity<T>(this EntityModule entityModule, string assetPath,string entityGroup,
            EntityUnitData userData = null, float autoReleaseInterval = 60f,
            int capacity = 60, float expireTime = 60f, int priority = 0)
        {
            entityModule.CreateEntity(typeof(T), assetPath,entityGroup, userData, autoReleaseInterval, capacity, expireTime,
                priority);
        }
        
        /// <summary>
        /// 创建实体
        /// </summary>
        /// <param name="entitySystem">EntitySystem</param>
        /// <param name="assetPath">实体资源路径</param>
        /// <param name="userData">实体数据</param>
        /// <param name="autoReleaseInterval">实体实例对象池自动释放可释放对象的间隔秒数</param>
        /// <param name="capacity">实体实例对象池容量</param>
        /// <param name="expireTime">实体实例对象池对象过期秒数</param>
        /// <param name="priority">实体实例对象池的优先级</param>
        /// <typeparam name="T">实体类型</typeparam>
        public static void CreateEntity<T>(this EntityModule entityModule, string assetPath,
            object userData, float autoReleaseInterval = 60f,
            int capacity = 60, float expireTime = 60f, int priority = 0)
        {
            entityModule.CreateEntity(typeof(T), assetPath, userData, autoReleaseInterval, capacity, expireTime,
                priority);
        }

        /// <summary>
        /// 创建实体
        /// </summary>
        /// <param name="entitySystem">EntitySystem</param>
        /// <param name="assetPath">实体资源路径</param>
        /// <param name="userData">EntityData实体数据</param>
        /// <param name="autoReleaseInterval">实体实例对象池自动释放可释放对象的间隔秒数</param>
        /// <param name="capacity">实体实例对象池容量</param>
        /// <param name="expireTime">实体实例对象池对象过期秒数</param>
        /// <param name="priority">实体实例对象池的优先级</param>
        /// <typeparam name="T">实体类型</typeparam>
        public static void CreateEntity<T>(this EntityModule entitySystem, string assetPath,
            EntityData userData = null, float autoReleaseInterval = 60f,
            int capacity = 60, float expireTime = 60f, int priority = 0)
        {
            if (userData == null)
            {
                userData = EntityData.Create();
            }

            entitySystem.CreateEntity(typeof(T), assetPath, userData, autoReleaseInterval, capacity, expireTime,
                priority);
        }

        /// <summary>
        /// 创建实体
        /// </summary>
        /// <param name="entitySystem"></param>
        /// <param name="logicType"></param>
        /// <param name="assetPath"></param>
        /// <param name="userData"></param>
        /// <param name="autoReleaseInterval"></param>
        /// <param name="capacity"></param>
        /// <param name="expireTime"></param>
        /// <param name="priority"></param>
        public static void CreateEntity(this EntityModule entitySystem, System.Type logicType, string assetPath,
            object userData = null, float autoReleaseInterval = 60f,
            int capacity = 60,
            float expireTime = 60f, int priority = 0)
        {
            var entityId = entitySystem.GenerateSerialId();
            entitySystem.CreateEntity(entityId, logicType, assetPath, userData, autoReleaseInterval, capacity,
                expireTime, priority);
        }
        public static void CreateEntity(this EntityModule entitySystem, System.Type logicType, string assetPath,
            string entityGroup,object userData = null, float autoReleaseInterval = 60f,
            int capacity = 60,
            float expireTime = 60f, int priority = 0)
        {
            var entityId = entitySystem.GenerateSerialId();
            entitySystem.CreateEntity(entityId, logicType, assetPath,entityGroup, userData, autoReleaseInterval, capacity,
                expireTime, priority);
        }

        /// <summary>
        /// 创建实体
        /// </summary>
        /// <param name="entitySystem"></param>
        /// <param name="entityId"></param>
        /// <param name="logicType"></param>
        /// <param name="assetPath"></param>
        /// <param name="userData"></param>
        /// <param name="autoReleaseInterval"></param>
        /// <param name="capacity"></param>
        /// <param name="expireTime"></param>
        /// <param name="priority"></param>
        public static void CreateEntity(this EntityModule entitySystem, int entityId,
            System.Type logicType, string assetPath, object userData = null, float autoReleaseInterval = 60f,
            int capacity = 60,
            float expireTime = 60f, int priority = 0)
        {
            if (entityId < 0)
            {
                Log.Error("Can not load entity id '{0}'.", entityId.ToString());
                return;
            }

            if (!entitySystem.HasEntityGroup(logicType.Name))
            {
                entitySystem.AddEntityGroup(logicType.Name,
                    autoReleaseInterval, capacity,
                    expireTime, priority);
            }

            entitySystem.ShowEntity(entityId, logicType, assetPath, logicType.Name,
                priority, userData);
        }
        public static void CreateEntity(this EntityModule entitySystem, int entityId,
            System.Type logicType, string assetPath,string entityGroup, object userData = null, float autoReleaseInterval = 60f,
            int capacity = 60,
            float expireTime = 60f, int priority = 0)
        {
            if (entityId < 0)
            {
                Log.Error("Can not load entity id '{0}'.", entityId.ToString());
                return;
            }

            if (!entitySystem.HasEntityGroup(entityGroup))
            {
                entitySystem.AddEntityGroup(entityGroup,
                    autoReleaseInterval, capacity,
                    expireTime, priority);
            }

            entitySystem.ShowEntity(entityId, logicType, assetPath, entityGroup,
                priority, userData);
        }
    }
}