using TEngine;
using UnityEngine;

namespace GameLogic
{
    internal sealed class AttachEntityInfo : IMemory
    {
        private Transform _parentTransform;
        private object _userData;

        public AttachEntityInfo()
        {
            _parentTransform = null;
            _userData = null;
        }

        public Transform ParentTransform => _parentTransform;

        public object UserData => _userData;

        public static AttachEntityInfo Create(Transform parentTransform, object userData)
        {
            AttachEntityInfo attachEntityInfo = MemoryPool.Acquire<AttachEntityInfo>();
            attachEntityInfo._parentTransform = parentTransform;
            attachEntityInfo._userData = userData;
            return attachEntityInfo;
        }

        public void Clear()
        {
            _parentTransform = null;
            _userData = null;
        }
    }
}
