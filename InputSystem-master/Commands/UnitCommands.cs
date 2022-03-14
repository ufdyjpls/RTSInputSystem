using Assets.Scripts.Game.Pathfinding.Formation;
using Game.Actions;
using Game.Objects;
using Game.Objects.Logic;
using RTS.Controlls;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CMS.Config;
using Assets.Scripts.Game;
using Utils;

namespace RTS.Controlls
{
    [CommandRealization(Command.Atack, BehaviourType.Attack, 10)]
    public class AtackCommand : GroupCommand
    {
        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return targetInfo.ResourceEntity == null && targetInfo.GameEntity != null && !targetInfo.GameEntity.IsAlly;
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            behaviour.Behaviour.Attack(targetInfo.GameEntity);
        }

    }

    [CommandRealization(Command.MoveAndAttack, BehaviourType.Move2Attack, 0, 2, true)]
    public class MoveAndAttackCommand : MoveCommand
    {
        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            behaviour.Behaviour.Move2Attack(positions[step]);
        }
    }

    [CommandRealization(Command.AtackON, BehaviourType.Attack, 0, 3, true)]
    public class AttackOnCommand : GroupCommand
    {
        public static new bool CanBeProcessedMenuCommand(List<InGameEntity> behaviours, MenuCommand command, TargetInfo targetInfo)
        {
            return behaviours.Any(x => x.Behaviour.Context.AttackOn == false) && !(behaviours.FirstOrDefault().Behaviour.Context.Owner is Building);
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            behaviour.Behaviour.Context.AttackOn.Value = true;
        }
    }

    [CommandRealization(Command.AtackOff, BehaviourType.Attack, 0, 4, true)]
    public class AttackOffCommand : GroupCommand
    {
        public static new bool CanBeProcessedMenuCommand(List<InGameEntity> behaviours, MenuCommand command, TargetInfo targetInfo)
        {
            return behaviours.Any(x => x.Behaviour.Context.AttackOn == true) && !behaviours.Any(x => x.Behaviour.Context.AttackOn == false) && !(behaviours.FirstOrDefault().Behaviour.Context.Owner is Building);
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            behaviour.Behaviour.Context.AttackOn.Value = false;
        }
    }

    [CommandRealization(Command.Move, BehaviourType.Move, 5)]
    public class MoveCommand : GroupCommand
    {
        protected Vector3[] positions;

        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return true;// UnityEngine.AI.NavMesh.CalculatePath(behaviours[0].Position, targetInfo.Position, (behaviours[0] as DynamicEntity)., 1, new UnityEngine.AI.NavMeshPath());
        }

        protected override void OnStart()
        {
            var direction = targetInfo.Direction;
            if (direction.Equals(Vector3.zero))
            {
                direction = targetInfo.Position - PositionUtils.GetCenderPoint(logicBehaviours);
            }

            positions = PositionUtils.GetWorldPoints<SquareGrid>(logicBehaviours, direction, targetInfo.Position);
            base.OnStart();
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            behaviour.Behaviour.Move(positions[step], Quaternion.LookRotation(targetInfo.Direction * -1).eulerAngles);
        }
    }

    [CommandRealization(Command.Patrol, BehaviourType.Move2Attack, 0, 5)]
    public class PatrolCommand : PingPongCommand
    {
        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return true;
        }

        protected override void OnInit()
        {
            var centerPoint = PositionUtils.GetCenderPoint(logicBehaviours);
            TargetInfo info = new TargetInfo
            {
                Position = centerPoint,
                Direction = (targetInfo.Position - centerPoint).normalized
            };

            var move = new MoveAndAttackCommand();
            move.SetCommandTarget(info);

            Add(move);

            targetInfo.Direction = info.Direction;
            move = new MoveAndAttackCommand();
            move.SetCommandTarget(targetInfo);

            Add(move);
        }

        protected override void OnMoveNextAction()
        {
            (_current as GroupCommand).AddToCurentCommand(logicBehaviours);
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {

        }
    }

    [CommandRealization(Command.Guard, BehaviourType.Guard, 0, 6)]
    public class GuardCommand : GroupCommand
    {
        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return true;
        }

        protected override void OnStart()
        {
            if (targetInfo.GameEntity != null && targetInfo.GameEntity.IsAlly)
            {
                base.OnStart();
            }
            else
            {
                End();
            }
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            if (step < 2)
            {
                SelectHandler.Instance().Sellect(behaviour as InGameEntity, false,false,true);
                behaviour.Behaviour.Guard(targetInfo.GameEntity);
            }
        }
    }

    [CommandRealization(Command.Hold, BehaviourType.Hold, 0, 0, true)]
    public class HoldOnCommand : GroupCommand
    {
        public static new bool CanBeProcessedMenuCommand(List<InGameEntity> behaviours, MenuCommand command, TargetInfo targetInfo)
        {
            return behaviours.Any(x => x.Behaviour.CurrentBehaviourType != BehaviourType.Hold);
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            behaviour.Behaviour.Hold();
        }
    }

    [CommandRealization(Command.HoldCancel, BehaviourType.Hold, 0, 1, true)]
    public class HoldOffCommand : GroupCommand
    {
        public static new bool CanBeProcessedMenuCommand(List<InGameEntity> behaviours, MenuCommand command, TargetInfo targetInfo)
        {
            return behaviours.Any(x => x.Behaviour.CurrentBehaviourType == BehaviourType.Hold) && !behaviours.Any(x => x.Behaviour.CurrentBehaviourType != BehaviourType.Hold);
        }
        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            behaviour.Behaviour.Idle();
        }
    }

    [CommandRealization(Command.FireRegion, BehaviourType.AttackWorldPoint, 0, 6)]
    public class FireRegionCommand : GroupCommand
    {
	public new static bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
	{

		foreach (var behaviour in behaviours)
		{
			var squad = behaviour.Behaviour.Context.Squad;

			if (squad == null) continue;
			
                	if (!squad.units[0].Behaviour.AssignedBehaviours().Contains(BehaviourType.AttackWorldPoint)) return false;
		}

		return true;
	}

        public static new bool CanBeProcessedMenuCommand(List<InGameEntity> behaviours, MenuCommand command, TargetInfo targetInfo)
        {
            foreach (var behaviour in behaviours)
            {
                var squad = behaviour.Behaviour.Context.Squad;

                if (squad == null) continue;

                if (!squad.units[0].Behaviour.AssignedBehaviours().Contains(BehaviourType.AttackWorldPoint)) return false;
            }

            return true;
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            behaviour.Behaviour.AttackWorldPoint(targetInfo.Position);
        }	
    }

    [CommandRealization(Command.FireMode1, BehaviourType.AttackWorldPoint)]
    public class FireMode1Command : GroupCommand
    {
        public static new bool CanBeProcessedMenuCommand(List<InGameEntity> behaviours, MenuCommand command, TargetInfo targetInfo)
        {
            foreach (var behaviour in behaviours)
            {
                var squad = behaviour.Behaviour.Context.Squad;

                if (squad == null) continue;

                if (!squad.units[0].Behaviour.AssignedBehaviours().Contains(BehaviourType.AttackWorldPoint)) return false;
            }

            return behaviours.Any(x => x.Behaviour.Context.SpreadModeOn == true);
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            behaviour.Behaviour.Context.SpreadModeOn = false;
        }
    }

    [CommandRealization(Command.FireMode2, BehaviourType.AttackWorldPoint)]
    public class FireMode2Command : GroupCommand
    {
        public static new bool CanBeProcessedMenuCommand(List<InGameEntity> behaviours, MenuCommand command, TargetInfo targetInfo)
        {
            foreach (var behaviour in behaviours)
            {
                var squad = behaviour.Behaviour.Context.Squad;

                if (squad == null) continue;

                if (!squad.units[0].Behaviour.AssignedBehaviours().Contains(BehaviourType.AttackWorldPoint)) return false;
            }

            return behaviours.Any(x => x.Behaviour.Context.SpreadModeOn == false) && !behaviours.Any(x => x.Behaviour.Context.SpreadModeOn == true);
        }
        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            behaviour.Behaviour.Context.SpreadModeOn = true;
        }
    }

    [CommandRealization(Command.Unload, BehaviourType.Unload, 0, 13)]
    public class UnloadCommand : GroupCommand
    {
        public static new bool CanBeProcessedMenuCommand(List<InGameEntity> behaviours, MenuCommand menuCommand, TargetInfo targetInfo)
        {
            foreach (var behaviour in behaviours)
            {
                if (behaviour.Behaviour.CurrentBehaviourType != BehaviourType.Idle)
                {
                    return false;
                }

                IWorkingPlaceContainer workingPlace = behaviour as IWorkingPlaceContainer;

                if (workingPlace!=null)
                {
                    if (workingPlace.FreeSpace < workingPlace.MaxUnitsInBuilding && workingPlace.WorkingPlace.InOutPlace.OnLayer(LayerMask.NameToLayer("Terrain")))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override void OnStart()
        {
           foreach(var besh in logicBehaviours)
            {
                (besh as IWorkingPlaceContainer).FreeAllPlaces(null);
            }
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {

        }
    }

    [CommandRealization(Command.Load, BehaviourType.Load, 20)]
    public class LoadCommand : GroupCommand
    {
        PlaceToWork[] places;
        int placeCounter;

        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return (targetInfo.Building == null && targetInfo.WorkingPlaceContainer != null && targetInfo.WorkingPlaceContainer.ReadyForLoad && targetInfo.GameEntity.IsAlly && targetInfo.WorkingPlaceContainer.FreeSpace > 0);
        }

        protected override void OnStart()
        {
            places = targetInfo.WorkingPlaceContainer.GetWorkingPlaces(null);
            placeCounter = places.Length - 1;

            if (placeCounter >= 0) base.OnStart();
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            if (placeCounter > 0)
            {
                behaviour.Behaviour.Move2Load(places[step], targetInfo.WorkingPlaceContainer);
                placeCounter--;
            }
        }
    }

    [CommandRealization(Command.Build, BehaviourType.Move2Build, 20)]
    public class BuildCommand : GroupCommand
    {
        int SendUnitsCounter;

        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return (targetInfo.Building != null && targetInfo.GameEntity.IsAlly && (targetInfo.Building.IsBusy()|| targetInfo.Building.Damaged) && targetInfo.Building.UnitsInBuilding < targetInfo.Building.MaxUnitsForCommand(Command.Build));
        }

        public static new bool CanBeProcessedMenuCommand(List<InGameEntity> behaviours, MenuCommand menuCommand, TargetInfo targetInfo)
        {
            BuildingConfig config=menuCommand.config as BuildingConfig;

            if (config==null) return false;

            return GameBootstrap.Instance.GameScene.MyPlayer.CheckConditions(config, 0);
        }

        protected override void OnStart()
        {
            if (CanBeProcessed(logicBehaviours, targetInfo))
            {
                if (targetInfo.Building.IsWall)
                {
                    SendUnitsCounter = SelectHandler.Instance().GetSelectedEntities().Count;
                }
                else
                {
                    SendUnitsCounter = targetInfo.Building.MaxUnitsForCommand(Command.Build); // - targetInfo.Building.UnitsInBuilding;
                }

                base.OnStart();
            }
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            if (targetInfo.Building.IsWall || (targetInfo.Building.UnitsMoveToBuilding + targetInfo.Building.UnitsInBuilding < targetInfo.Building.MaxUnitsForCommand(Command.Build)))
            {
                behaviour.Behaviour.Move2Build(targetInfo.Building);
                SendUnitsCounter--;
                if (SendUnitsCounter==0)
		{
                    SelectHandler.Instance().SetSelected(behaviour.Behaviour.Context.Owner, false, true);
		}
                else
		{
                    SelectHandler.Instance().SetSelected(behaviour.Behaviour.Context.Owner, false, false);
		}
            }
        }
    }

    [CommandRealization(Command.MineRock, BehaviourType.Move2Mine, 20)]
    public class MineRockCommand : MineCommand
    {
        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return targetInfo.ResourceEntity != null && targetInfo.ResourceEntity.Resource.ResourceKey == CMS.Config.GameResources.Stone;
        }
    }

    [CommandRealization(Command.MineTree, BehaviourType.Move2Mine, 20)]
    public class MineTreeCommand : MineCommand
    {
        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return targetInfo.ResourceEntity != null && targetInfo.ResourceEntity.Resource.ResourceKey == CMS.Config.GameResources.Wood;
        }
    }

    [CommandRealization(Command.MineFood, BehaviourType.Move2Mine, 20)]
    public class MineFoodCommand : MineCommand
    {
        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return targetInfo.ResourceEntity != null && targetInfo.ResourceEntity.Resource.ResourceKey == CMS.Config.GameResources.Food;
        }
    }

    [CommandRealization(Command.Mine, BehaviourType.Move2Mine, 19)]
    public class MineCommand : GroupCommand
    {
        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return (targetInfo.ResourceEntity != null && targetInfo.ResourceEntity.PurposeType == ResourceEntityType.ForUnits);
        }

        protected override void OnStart()
        {
            if (CanBeProcessed(logicBehaviours, targetInfo))
            {
                base.OnStart();
            }
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            behaviour.Behaviour.Move2Mine(targetInfo.ResourceEntity);
        }
    }

    [CommandRealization(Command.Work, BehaviourType.Move2Work, 19)]
    public class WorkCommand : GroupCommand
    {
        int SendUnitsCounter;

        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return (targetInfo.Building != null && targetInfo.Building.IsAlly && targetInfo.Building.MineTarget && targetInfo.Building.IsBuild && targetInfo.Building.UnitsInBuilding <= targetInfo.Building.MaxUnitsForCommand(Command.Work));
        }

        protected override void OnStart()
        {
            if (CanBeProcessed(logicBehaviours, targetInfo))
            {
                SendUnitsCounter = targetInfo.Building.MaxUnitsForCommand(Command.Work);

                base.OnStart();
            }
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            if (SendUnitsCounter > 0)
            {
                behaviour.Behaviour.Move2Work(targetInfo.Building);
                SendUnitsCounter--;
            }
        }
    }
}
