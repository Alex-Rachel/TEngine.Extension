using System;
using UnityEngine;

namespace GameLogic
{
    public sealed partial class EntityModule
    {
        [Serializable]
        private sealed class EntityGroup
        {
            [SerializeField]
            private string m_Name = null;

            [SerializeField]
            private float m_InstanceAutoReleaseInterval = 60f;

            [SerializeField]
            private int m_InstanceCapacity = 16;

            [SerializeField]
            private float m_InstanceExpireTime = 60f;

            [SerializeField]
            private int m_InstancePriority = 0;

            public string Name => m_Name;

            public float InstanceAutoReleaseInterval => m_InstanceAutoReleaseInterval;

            public int InstanceCapacity => m_InstanceCapacity;

            public float InstanceExpireTime => m_InstanceExpireTime;

            public int InstancePriority => m_InstancePriority;
        }
    }
}
