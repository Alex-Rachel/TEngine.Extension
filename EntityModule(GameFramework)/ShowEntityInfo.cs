using System;
using TEngine;

namespace GameLogic
{
    internal sealed class ShowEntityInfo : IMemory
    {
        private Type _entityLogicType;
        private object _userData;
    
        public ShowEntityInfo()
        {
            _entityLogicType = null;
            _userData = null;
        }
    
        public Type EntityLogicType => _entityLogicType;
    
        public object UserData => _userData;
    
        public static ShowEntityInfo Create(Type entityLogicType, object userData)
        {
            ShowEntityInfo showEntityInfo = MemoryPool.Acquire<ShowEntityInfo>();
            showEntityInfo._entityLogicType = entityLogicType;
            showEntityInfo._userData = userData;
            return showEntityInfo;
        }
    
        public void Clear()
        {
            _entityLogicType = null;
            _userData = null;
        }
    }
}
