#if UNITY_EDITOR
using UnityEngine;

namespace GameLogic
{
    public class ComponentView: MonoBehaviour
    {
        public Entity Component
        {
            get;
            set;
        }
    }
}
#endif