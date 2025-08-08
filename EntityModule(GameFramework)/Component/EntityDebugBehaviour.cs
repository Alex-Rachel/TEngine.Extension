using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    [Serializable]
    public class ComponentDebugKeyInfo
    {
        public string name;
        public string val;
    }

    [Serializable]
    public class ComponentDebugInfo
    {
        public string name;
        public List<ComponentDebugKeyInfo> infoList = new List<ComponentDebugKeyInfo>();
    }

    /// <summary>
    /// 用来调试查看Entity信息。
    /// </summary>
    public class EntityDebugBehaviour : MonoBehaviour
    {
        public List<ComponentDebugInfo> cmptInfoList = new List<ComponentDebugInfo>();

        public event Action OnGizmosSelect;
        public event Action OnGizmos;

        public ComponentDebugInfo AddDebugCmpt(string cmptName)
        {
            var cmptInfo = cmptInfoList.Find(item => item.name == cmptName);
            if (cmptInfo == null)
            {
                cmptInfo = new ComponentDebugInfo();
                cmptInfo.name = cmptName;
                this.cmptInfoList.Add(cmptInfo);
            }

            return cmptInfo;
        }

        public void RmvDebugCmpt(string cmptName)
        {
            cmptInfoList.RemoveAll(item => item.name == cmptName);
        }

        public void SetDebugInfo(string cmptName, string key, string val)
        {
            var cmptInfo = AddDebugCmpt(cmptName);
            var entry = cmptInfo.infoList.Find(t => t.name == key);
            if (entry == null)
            {
                entry = new ComponentDebugKeyInfo();
                entry.name = key;
                cmptInfo.infoList.Add(entry);
            }

            entry.val = val;
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (OnGizmosSelect != null)
            {
                OnGizmosSelect();
            }
        }

        void OnDrawGizmos()
        {
            if (OnGizmos != null)
            {
                OnGizmos();
            }
        }
#endif
    }
}