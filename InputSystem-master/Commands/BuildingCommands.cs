using Assets.Scripts.Game.Pathfinding.Formation;
using Game.Actions;
using Game.Objects;
using Game.Objects.Logic;
using RTS.Controlls;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CMS.Config;

namespace RTS.Controlls
{
    [CommandRealization(Command.RequestProduction, BehaviourType.BuildingProduction, 0, 0, true)]
    public class RequestProduction : GroupCommand
    {
        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return true;
        }

        public static new bool CanBeProcessedMenuCommand(List<InGameEntity> behaviours, MenuCommand menuCommand, TargetInfo targetInfo)
        {
            if (behaviours.Count > 1)
                return false;

            foreach (var beh in behaviours)
            {
                if (!(beh is Building))
                {
                    return (false);
                }
                else
                {
                    Building building = (Building)beh;
                    
                    return building.CanProduct(menuCommand.config as ProductionBlueprint);
                }
            }

            return true;
        }

        public static new bool InteractableMenuCommand(List<InGameEntity> behaviours, MenuCommand menuCommand)
        {
            foreach (var beh in behaviours)
            {
                if (!(beh is Building))
                {
                    return true;
                }
                else
                {
                    Building building = (Building)beh;
                    return building.InteractableButton(menuCommand.config as ProductionBlueprint);
                }
            }
            
            return true;
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            base.OnBeshProcess(behaviour, step);

            if (menuCommand != null)
            {
                Building building = behaviour.Behaviour.Context.Owner as Building;
                building.Production.RequestProduction(menuCommand.config as ProductionBlueprint, 1);
            }
        }
    }

    [CommandRealization(Command.UndoProduction, BehaviourType.BuildingProduction, 0, 0, true)]
    public class UndoProduction : GroupCommand
    {
        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return true;
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            base.OnBeshProcess(behaviour, step);
            
            if (menuCommand != null)
            {
                Building building = behaviour.Behaviour.Context.Owner as Building;
                building.Production.UndoProduction(menuCommand.config as ProductionBlueprint, 1);
            }
        }
    }

    [CommandRealization(Command.UpgradeBuilding, BehaviourType.BuildingUpgrade, 0, 0, true)]
    public class UpgradeBuilding : GroupCommand
    {
        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return true;
        }

        public static new bool CanBeProcessedMenuCommand(List<InGameEntity> behaviours, MenuCommand menuCommand, TargetInfo targetInfo)
        {
            if (behaviours.Count > 1) return false;
                
            foreach (var beh in behaviours)
            {
                if (!(beh is Building))
                {
                    return (false);
                }
                else
                {
                    Building building = (Building)beh;

                    if (menuCommand.intParam != building.UpgradeLevel || !building.CanUpgrade())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            base.OnBeshProcess(behaviour, step);

            Building building = behaviour.Behaviour.Context.Owner as Building;
            building.UpgradeAction.Upgrade(menuCommand.intParam);
        }
    }


    [CommandRealization(Command.SetGatherPoint, BehaviourType.BuildingGatherPoint, 0, 0, true)]
    public class SetGatherPoint : GroupCommand
    {
        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return true;
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            base.OnBeshProcess(behaviour, step);

            Building building = behaviour.Behaviour.Context.Owner as Building;
            building.SetGatherPoint.Start();
        }
    }

    [CommandRealization(Command.UndoSetGatherPoint, BehaviourType.BuildingGatherPoint, 0, 0, true)]
    public class UndoSetGatherPoint : GroupCommand
    {
        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return true;
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            base.OnBeshProcess(behaviour, step);

            Building building = behaviour.Behaviour.Context.Owner as Building;
            building.SetGatherPoint.Undo();
        }
    }

    [CommandRealization(Command.SelectFreeWorkers, BehaviourType.BuidingFreeWorkers, 0, 0, true)]
    public class SelectFreeWorkers : GroupCommand
    {
        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return true;
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            base.OnBeshProcess(behaviour, step);

            var player = Assets.Scripts.Game.GameBootstrap.Instance.GameScene.MyPlayer;
            bool didDropSelection = false;
            
            foreach (var unit in player.GetOwnedInGameObjects(typeof(WorkerUnit)))
            {
                WorkerUnit worker = (WorkerUnit)unit;
                
                if (worker.Behaviour.CurrentBehaviourType != BehaviourType.Move2Work  &&
                    worker.Behaviour.CurrentBehaviourType != BehaviourType.Move2Mine  &&
                    worker.Behaviour.CurrentBehaviourType != BehaviourType.Move2Build &&
                    worker.Behaviour.CurrentBehaviourType != BehaviourType.Move2Store &&
                    !worker.BuildBuilding.InProgress && !worker.MineAction.InProgress && 
                    !worker.WorkAction.InProgress && worker.CollectedResource.Count == 0)
                {
                    if(!didDropSelection)
                    {
                        didDropSelection = true;
                        SelectHandler.Instance().ClearSelectedUnits();
                    }

                    SelectHandler.Instance().Sellect(worker, true, false,false);
                }
            }
            
            if (didDropSelection)
            {
                SelectHandler.Instance().InvokeSelectionListChanged();
            }
        }
    }

    [CommandRealization(Command.SelectFreeMines, BehaviourType.BuildingFreeMines, 0, 0, true)]
    public class SelectFreeMines : GroupCommand
    {
        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return true;
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            base.OnBeshProcess(behaviour, step);
            
            var player = Assets.Scripts.Game.GameBootstrap.Instance.GameScene.MyPlayer;
            bool didDropSelection = false;
            
            foreach (var entity in player.GetOwnedInGameObjects(typeof(Building)))
            {
                Building building = entity as Building;
                if (building.ConfigId.Contains("Mine") && building.UnitsInBuilding < building.MaxUnitsInBuilding && building.IsBuild && building.Alive)
                {
                    if (!didDropSelection)
                    {
                        didDropSelection = true;
                        SelectHandler.Instance().ClearSelectedUnits();
                    }
                    
                    SelectHandler.Instance().Sellect(building, true, false, false);
                }
            }
            
            if (didDropSelection)
            {
                SelectHandler.Instance().InvokeSelectionListChanged();
            }
        }
    }

    [CommandRealization(Command.Research, BehaviourType.BuildingReserach, 0, 0, true)]
    public class ResearchCommand : GroupCommand
    {
        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return true;
        }

        public static new bool CanBeProcessedMenuCommand(List<InGameEntity> behaviours, MenuCommand menuCommand, TargetInfo targetInfo)
        {
            if (behaviours.Count > 1) return false;
            
            foreach (var beh in behaviours)
            {
                if (!(beh is Building))
                {
                    return (false);
                }
                else
                {
                    Building building = (Building)beh;
                    
                    return building.CanResearch(menuCommand.config as ResearchesProductionConfig);
                }
            }

            return true;
        }

        public static new bool InteractableMenuCommand(List<InGameEntity> behaviours, MenuCommand menuCommand)
        {
            foreach (var beh in behaviours)
            {
                if (!(beh is Building))
                {
                    return true;
                }
                else
                {
                    Building building = (Building)beh;

                    return building.InteractableButton(menuCommand.config as ResearchesProductionConfig);
                }
            }

            return true;
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            base.OnBeshProcess(behaviour, step);

            Building building = behaviour.Behaviour.Context.Owner as Building;
            building.Research.Research(menuCommand.config as ResearchesProductionConfig);
        }
    }

    [CommandRealization(Command.UnloadWorker, BehaviourType.BuildingUnload, 0, 13, true)]
    public class UnloadWorkerCommand : GroupCommand
    {
        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return true;
        }

        public static new bool CanBeProcessedMenuCommand(List<InGameEntity> behaviours, MenuCommand menuCommand, TargetInfo targetInfo)
        {
            foreach (var beh in behaviours)
            {
                if (!(beh is Building))
                {
                    return false;
                }
                else
                {
                    Building building = (Building)beh;

                    if (building.IsBuild && building.MaxUnitsInBuilding > building.GetWorkingPlaces(null).Length) return true;
                }
            }
            
            return false;
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            base.OnBeshProcess(behaviour, step);

            Building building = behaviour.Behaviour.Context.Owner as Building;
            WorkerUnit worker = building.WorkingPlace.GetInWorkers().FirstOrDefault() as WorkerUnit;
            worker?.WorkAction.Stop();
        }
    }

    [CommandRealization(Command.OpenDoor, BehaviourType.OpenClose, 0, 0, true)]
    public class OpenDoorCommand : GroupCommand
    {
        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return true;
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            base.OnBeshProcess(behaviour, step);

            Building building = behaviour.Behaviour.Context.Owner as Building;
            building.OpenClose.ChangeState(true);
        }
    }

    [CommandRealization(Command.CloseDoor, BehaviourType.OpenClose, 0, 0, true)]
    public class CloseDoorCommand : GroupCommand
    {
        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return true;
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            base.OnBeshProcess(behaviour, step);

            Building building = behaviour.Behaviour.Context.Owner as Building;
            building.OpenClose.ChangeState(false);
        }
    }
}
