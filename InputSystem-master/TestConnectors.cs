using UnityEngine;
using System.Collections.Generic;

namespace RTS.Controlls
{
    class TestConnectors:MonoBehaviour
    {
        public List<GameObject> activateObjectList;
        public List<string> activateObjectName;
        public List<MonoBehaviour> activateComponentList;

        void Start()
        {
            for (int i = 0; i < activateObjectList.Count; i++)
            {
                MenuHandler.Instance.AddTestMenu(i < activateObjectName.Count ? activateObjectName[i] : activateObjectList[i].name, activateObjectList[i]);
            }
            
            for (int i = 0; i < activateComponentList.Count; i++)
            {
                MenuHandler.Instance.AddTestComponent(activateComponentList[i].name, activateComponentList[i]);
            }
        }
    }
}
