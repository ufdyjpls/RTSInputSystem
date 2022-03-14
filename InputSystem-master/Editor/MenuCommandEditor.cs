using UnityEngine.UI;
using System.Collections.Generic;
using UnityEditor;
using Game.CommanderController;
using Assets.Scripts.Game.Pathfinding.Formation;
using CMS.Config;
using CMS.Editor;

namespace RTS.Controlls
{
    [CanEditMultipleObjects()]
    [CustomEditor(typeof(MenuCommand))]
    public class MenuCommandEditor : UnityEditor.Editor
    {
        private MenuCommand menuCommand { get { return target as MenuCommand; } }
        
        public override void OnInspectorGUI()
        {
            menuCommand.command = (Command)EditorGUILayout.EnumPopup("Command:", menuCommand.command);
            
            switch (menuCommand.command)
            {
                case Command.CreateSquad:
                    menuCommand.formation = (TypeOfFormation)EditorGUILayout.EnumFlagsField("Formation:", menuCommand.formation);
                    break;
                    
                case Command.ResizeFormation:
                    menuCommand.boolParam = EditorGUILayout.Toggle("Expand:", menuCommand.boolParam);
                    break;
                    
                case Command.Build:
                    menuCommand.config = ScriptableGUIUtils.DrawObjectField<BuildingConfig>("Config:", menuCommand.config as BuildingConfig);
                    // menuCommand.configId = config?.ConfigId;
                    break;
                    
                case Command.Recruit:
                    menuCommand.entityType = EditorGUILayout.IntField("EntityType:", menuCommand.entityType);
                    break;
                    
                case Command.RequestProduction:
                    menuCommand.config = ScriptableGUIUtils.DrawObjectField<ProductionBlueprint>("Config:", menuCommand.config as ProductionBlueprint);
                    EditorGUILayout.LabelField("Image to replace:");
                    menuCommand.icon = (Image)EditorGUILayout.ObjectField(menuCommand.icon, typeof(Image), true);
                    break;
                    
                case Command.UpgradeBuilding:
                    menuCommand.intParam = EditorGUILayout.IntField("Upgrade Level", menuCommand.intParam);
                    break;
                    
                case Command.SelectFreeWorkers:
                    menuCommand.config = ScriptableGUIUtils.DrawObjectField<BuildingConfig>("Building Config:", menuCommand.config as BuildingConfig);
                    // menuCommand.configId = config?.ConfigId;
                    break;
                    
                case Command.Research:
                    menuCommand.config = ScriptableGUIUtils.DrawObjectField<ResearchesProductionConfig>("Config:", menuCommand.config as ResearchesProductionConfig);
                    EditorGUILayout.LabelField("Image to replace:");
                    menuCommand.icon = (Image)EditorGUILayout.ObjectField(menuCommand.icon,typeof(Image),true);
                    break;
            }

            menuCommand.inputCommand = (InputCommand)EditorGUILayout.EnumPopup("inputCommand",menuCommand.inputCommand);
            menuCommand.DisableIfCantBeProcessed = EditorGUILayout.Toggle("DisableIfCantBeProcessed", menuCommand.DisableIfCantBeProcessed);
            menuCommand.ProcessCommandOnClick = EditorGUILayout.Toggle("ProcessCommandOnClick", menuCommand.ProcessCommandOnClick);
            menuCommand.CanUseForMiltiplyChoose = EditorGUILayout.Toggle("CanUseForMiltiplyChoose", menuCommand.CanUseForMiltiplyChoose);
            menuCommand.SortingPriority = EditorGUILayout.IntField("Sorting priority", menuCommand.SortingPriority);
        }

    }
}
