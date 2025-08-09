namespace GameLogic
{
    internal sealed partial class EntityManager
    {
        /// <summary>
        /// 实体状态。
        /// </summary>
        private enum EntityStatus : byte
        {
            /// <summary>
            /// 未知状态。
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// 即将初始化。
            /// </summary>
            WillInit,
            /// <summary>
            /// 已经初始化。
            /// </summary>
            Inited,
            /// <summary>
            /// 即将显示。
            /// </summary>
            WillShow,
            /// <summary>
            /// 已经显示。
            /// </summary>
            Showed,
            /// <summary>
            /// 即将隐藏。
            /// </summary>
            WillHide,
            /// <summary>
            /// 已经隐藏。
            /// </summary>
            Hidden,
            /// <summary>
            /// 即将回收。
            /// </summary>
            WillRecycle,
            /// <summary>
            /// 已经回收。
            /// </summary>
            Recycled
        }
    }
}
