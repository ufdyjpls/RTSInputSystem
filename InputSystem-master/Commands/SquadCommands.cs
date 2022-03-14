using Assets.Scripts.Game.Pathfinding.Formation;
using Game.CommanderController;
using Game.Objects;
using Game.Objects.Logic;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CMS.Config;

namespace RTS.Controlls
{
    [CommandRealization(Command.CreateSherenga, BehaviourType.CreateSquad, 0, 7)]
    public class CreateKolonSquadCommand : CreateSquadCommand
    {
        protected override TypeOfFormation TypeOfFormation
        {
            get
            {
                return TypeOfFormation.Sherenga;
            }
        }
    }

    [CommandRealization(Command.CreateKolon, BehaviourType.CreateSquad, 0, 8)]
    public class CreateKolonColonnaCommand : CreateSquadCommand
    {
        protected override TypeOfFormation TypeOfFormation
        {
            get
            {
                return TypeOfFormation.Kolonna;
            }
        }
    }

    [CommandRealization(Command.CreateCare, BehaviourType.CreateSquad, 0, 9)]
    public class CreateCareSquadCommand : CreateSquadCommand
    {
        protected override TypeOfFormation TypeOfFormation
        {
            get
            {
                return TypeOfFormation.Kare;
            }
        }
    }

    [CommandRealization(Command.CreateSquad, BehaviourType.CreateSquad)]
    public class CreateSquadCommand : GroupCommand
    {
        protected virtual TypeOfFormation TypeOfFormation
        {
            get
            {
                return TypeOfFormation.Kare;
            }
        }

        public static new bool CanBeProcessedMenuCommand(List<InGameEntity> behaviours, MenuCommand menuCommand, TargetInfo targetInfo)
        {
            foreach (var beh in behaviours)
            {
                Unit unit = beh as Unit;

                if (unit)
                {
                    FormationConfig config = (unit.config as UnitConfig).FormationConfig;

                    if (config)
                    {
                        switch (menuCommand.command)
                        {
                            case Command.CreateKolon:
                                if (config.kolonna.Count > 0) return true;
                                break;

                            case Command.CreateSherenga:
                                if (config.sherenga.Count > 0) return true;
                                break;

                            case Command.CreateCare:
                                if (config.kare.Count > 0) return true;
                                break;
                        }
                    }
                }
            }

            return false;
        }

        protected override void OnStart()
        {
            List<ISquadAgent> findSquads = new List<ISquadAgent>();
            List<DynamicEntity> commanders = null;
            List<DynamicEntity> drummers = null;
            List<DynamicEntity> bannerBearers = null;
            Dictionary<string, List<DynamicEntity>> unitTypes = new Dictionary<string, List<DynamicEntity>>();

            foreach (var besh in logicBehaviours)
            {
                if (besh is SquadNavAgent)
                {
                    findSquads.Add((SquadNavAgent)besh);
                }
                else
                {
                    if (!unitTypes.ContainsKey(besh.Behaviour.Context.Unit.ConfigId))
                    {
                        unitTypes[besh.Behaviour.Context.Unit.ConfigId] = new List<DynamicEntity>();
                    }

                    unitTypes[besh.Behaviour.Context.Unit.ConfigId].Add(besh as DynamicEntity);
                }
            }

            //Для существующих отрядов поменяем формацию
            foreach (ISquadAgent squad in findSquads)
            {
                if (squad.typeOfFormation != TypeOfFormation)
                {
                    squad.ChangeFormationType(TypeOfFormation);
                }
            }

            //Создадим новые отряды
            foreach (var entityType in unitTypes.Keys)
            {
                List<DynamicEntity> group = unitTypes[entityType];

                if (group.Count == 0) continue;

                Unit oneUnit = null;

                if (group[0] is Unit)
                {
                    oneUnit = (Unit)group[0];
                }

                if (!oneUnit) return;

                FormationGroup formation = new FormationGroup(oneUnit.FormationConfig, TypeOfFormation, group.Count);

                //Нет описания формации или недостаточно количества
                if (formation.FormationDescription == null || formation.GetCount() == 0) continue;

                if (formation.NeedCommander)
                {
                    // Отрядов из коммандиров... не существует
                    if (entityType == formation.FormationConfig.commander?.ConfigId
                        || entityType == formation.FormationConfig.drummer?.ConfigId
                        || entityType == formation.FormationConfig.bannerBearer?.ConfigId)
                        continue;

                    if (unitTypes.ContainsKey(formation.FormationConfig.commander?.ConfigId)
                        && unitTypes.ContainsKey(formation.FormationConfig.drummer?.ConfigId)
                        && unitTypes.ContainsKey(formation.FormationConfig.bannerBearer?.ConfigId))
                    {
                        commanders = unitTypes[formation.FormationConfig.commander.ConfigId];
                        drummers = unitTypes[formation.FormationConfig.drummer.ConfigId];
                        bannerBearers = unitTypes[formation.FormationConfig.bannerBearer.ConfigId];

                        if (commanders.Count == 0 || drummers.Count == 0 || bannerBearers.Count == 0)
                            continue;

                        group.Insert(0, commanders[0]);
                        commanders.RemoveAt(0);
                        group.Insert(1, drummers[0]);
                        drummers.RemoveAt(0);
                        group.Insert(2, bannerBearers[0]);
                        bannerBearers.RemoveAt(0);
                    }
                    else
		    {
                        continue;
		    }
                }

                var squad = SquadManager.Instance.CreateSquadAgent() as SquadNavAgent;
                squad.Create(formation, group.Cast<Unit>().ToList());
	            squad.Behaviour.SelectBehaviour(BehaviourType.Idle);
	    }

            End();
        }
    }

    [CommandRealization(Command.DismissSquad, BehaviourType.CreateSquad, 0, 13)]
    public class DismisSquadCommand : GroupCommand
    {
        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return behaviours.Find(x => x is SquadNavAgent) != null;
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            if (behaviour is SquadNavAgent)
            {
                SquadManager.Instance.DismissSquad(((SquadNavAgent)behaviour));
            }
        }
    }

    [CommandRealization(Command.FillSquad, BehaviourType.CreateSquad)]
    public class FillSquadCommand : GroupCommand
    {
        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return behaviours.Find(x => x is SquadNavAgent) != null;
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            if (behaviour is SquadNavAgent)
            {
                SquadManager.Instance.FillSquad(((SquadNavAgent)behaviour));
            }
        }
    }

    [CommandRealization(Command.ResizeFormation, BehaviourType.CreateSquad, 0, 10)]
    public class ResizeSquadCommand : GroupCommand
    {
        public static new bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return behaviours.Find(x => x is SquadNavAgent) != null;
        }

        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            if (behaviour is SquadNavAgent)
            {
                ((SquadNavAgent)behaviour).ChangeFormationSize(menuCommand.boolParam);
            }
        }
    }
}
