# if UNITY_EDITOR
using RTS.Controlls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Game.Selection;
using CMS.Editor;
using Game.Objects;
using Game.Objects.Logic;

[CustomEditor(typeof(SelectHandler))]
[CanEditMultipleObjects]
public class SelectHandlerEditor : Editor {

    private SelectHandler selectHandler;
    private bool showContent = false;

    public override void OnInspectorGUI()
    {

        base.OnInspectorGUI();
        showContent = GUILayout.Toggle(showContent, "Show sellection group");
        if (showContent)
        {
            List<Type> allTypes = new List<Type>(Assembly.GetAssembly(typeof(SelectHandler)).GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ISelectable))).ToArray());
            List<String> typesName = new List<String>(allTypes.Count);
            
            for (int i = 0; i < allTypes.Count; i++)
            {
                typesName.Add(allTypes[i].Name);
            }

            for (int j = 0; j < selectHandler.selectGroup.Count; j++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.Label("Group " + j);
                
                for (int i = 0; i < selectHandler.selectGroup[j].types.Count; i++)
                {
                    GUILayout.BeginHorizontal();

                    int typeIndex = Mathf.Clamp(typesName.IndexOf(selectHandler.selectGroup[j].types[i]), 0, int.MaxValue);
                    selectHandler.selectGroup[j].types[i] = typesName[EditorGUILayout.Popup(typeIndex, typesName.ToArray())];
                    selectHandler.selectGroup[j].activeBehaviours[i] = (BehaviourType)EditorGUILayout.EnumPopup(selectHandler.selectGroup[j].activeBehaviours[i]);

                    if (GUILayout.Button("-"))
                    {
                        selectHandler.selectGroup[j].types.RemoveAt(i);
                        selectHandler.selectGroup[j].activeBehaviours.RemoveAt(i);

                        i--;
                    }

                    GUILayout.EndHorizontal();
                }
                
                if (GUILayout.Button("+"))
                {
                    selectHandler.selectGroup[j].types.Add(typeof(Unit).Name);
                    selectHandler.selectGroup[j].activeBehaviours.Add(BehaviourType.None);
                }
                
                GUILayout.EndVertical();

                if (GUILayout.Button("-"))
                {
                    selectHandler.selectGroup.RemoveAt(j);
                    j--;
                }
                
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("+"))
            {
                selectHandler.selectGroup.Add(new SelectGroup());
            }

            GUILayout.EndHorizontal();
        }
    }
    
    void OnEnable()
    {
        selectHandler = target as SelectHandler;
    }
}
#endif
