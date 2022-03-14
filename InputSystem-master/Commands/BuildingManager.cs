using System;
using UnityEngine;
using Assets.Scripts.Game.Pathfinding.Formation;
using System.Collections.Generic;
using Game.Objects;
using Assets.Scripts.Game.EventSystem;
using Assets.Scripts.Game.EventSystem.CommandEvents;
using Assets.Scripts.Game.EventSystem.Events;
using RTS.Controlls;
using System.Linq;
using Game.Objects.Logic;
using Game;
using Game.CommanderController;
using CMS.Config;

namespace RTS.Controlls
{
    public partial class CommandManager 
    {
        public void SelectBuildingPlace(int EntityType)
        {
            BuilderController.Instance.NewBuilding(EntityType);
        }

        public void SelectBuildingPlace(string configId)
        {
            BuilderController.Instance.NewBuilding(configId);
        }
        
        public void SelectBuildingPlace(BuildingConfig config)
        {
            if (config == null)
            {
                return;
            }

            BuilderController.Instance.NewBuilding(config.ConfigId);
        }
        
        public void CancelBuildingPlacement()
        {
            if (BuilderController.Instance.IsPlacingBuilding())
            {
                BuilderController.Instance.CancelPlacement();
            }
            
            SetDefaultCommand();
        }
    }
}
