using System;
using UnityEngine;
using Assets.Scripts.Game.Pathfinding.Formation;
using System.Collections.Generic;
using Game.Objects;
using Assets.Scripts.Game.EventSystem;
using Assets.Scripts.Game.EventSystem.Events;
using RTS.Controlls;
using System.Linq;
using Assets.Scripts.Game;
using CMS.Config;
using Game;
using Game.CommanderController;
using Game.Objects.Logic;
using Game.Actions;
using Utils;
using Game.Damage;
using CMS.Configs;
using System.Reflection;
using Assets.Scripts.Game.EventSystem.CommandEvents;

namespace RTS.Controlls
{
    public enum Command
    {
        None = 0,
        //	    CreateUnit,
        /// <summary>
        ///     Команды Юнита
        /// “Двигаться и атаковать” 1) стр. 14
        /// “Охрана” 1) стр. 14
        /// “Держать позицию” 1) стр. 14
        /// “Отмена удержания позиции”  1) стр. 14
        /// “Патрулировать” 1) стр. 14. В качестве параметра передается путь патрулирования.
        /// “Включить атаку”. 1) стр. 15.
        /// “Отключить атаку”. 1) стр. 15.
        /// “Выгрузить” 1) стр. 15. Для сущности корабль
        /// “Режим стрельбы”. 1) стр. 15.
        /// “Обстрелять область”.1) стр. 15.
        /// “Строить здание” 1) стр. 34
        /// Искать ресурс возле точки сбора 1)90
        /// </summary>
        Atack = 1, Guard = 2, Hold = 3, HoldCancel = 4, Patrol = 5, AtackON = 6, AtackOff = 7, Unload = 8, FireMode = 9, FireRegion = 10, Build = 11,
        /// <summary>
        /// “Пополнить отряд” 1) стр. 13
        /// “Распустить отряд”. 1) стр. 14
        /// “Сформировать шеренгу”, “Сформировать колонну” и “Сформировать каре” 1)52  1) стр.34
        /// “Сжать построение” 1) стр. 18
        /// “Расширить построение”  1) стр. 18
        /// </summary>
        FillSquad = 12, DismissSquad = 13, CreateSquad = 14, ResizeFormation = 15,
        /// <summary>
        /// “Нанять юнита” 1) стр. 27
        /// “Строить здание”. Подписать на юнитов, которые идут строить, начинать строительство после перехода подписанных юнитов в состоянии строить это здание. 1) стр. 34
        /// Установки “точки сбора”. 1)89
        /// “Улучшение здания”
        /// Рынок. Обмен ресурсов. 1)99
        /// “Улучшение оружия”. 1)102
        /// </summary>
        Recruit = 16, TargetPlace = 17, UpgradeBuilding = 18, UpgradeWeapon = 19, Exchange = 20, Move = 21, Work = 22, Mine = 23, CreateCare, CreateKolon, CreateSherenga, MoveAndAttack, Load, MineRock, MineTree, MineFood,
        RequestProduction = 32, Research = 33, SetGatherPoint = 34, SelectFreeWorkers = 35, 
        UnloadWorker = 36, FireMode1 = 37, FireMode2 = 38,
        OpenDoor = 39, CloseDoor =40 ,
        SelectFreeMines = 41,
        CreateKiln=42,
        UndoProduction=43,
        UndoSetGatherPoint=44
    }

    public partial class CommandManager : Singleton<CommandManager>
    {

        public Command CurrentCommand { get { return currentCommand; } set { currentCommand = value; CommandChange?.Invoke(currentCommand); } }
        public MenuCommand MenuCommand;
        public static event Action<Command> CommandChange;
        
        [SerializeField]
        private List<GroupCommand> ActiveCommands = new List<GroupCommand>();
        private Dictionary<Command, MethodInfo> commandComporationInteractableMenuMethod = new Dictionary<Command, MethodInfo>();
        private Dictionary<Command, MethodInfo> commandComporationMenuMethod = new Dictionary<Command, MethodInfo>();
        private List<InGameEntity> selectedBehaviourList = new List<InGameEntity>();
        private Dictionary<Command, CommandRealization> commandRealizations = new Dictionary<Command, CommandRealization>();
        private Dictionary<Command, MethodInfo> commandComporationMethod = new Dictionary<Command, MethodInfo>();
        private Dictionary<Command, Type> commandRealizationTypes = new Dictionary<Command, Type>();
        private TargetInfo targetInfo;
        private Command command;
        private Command currentCommand;

        /// <summary>
        /// Установить на команду действующую по умолчанию. Может зависеть от текущих выделенных сущностей
        /// </summary>
        public void SetDefaultCommand()
        {
            CurrentCommand = Command.None;
        }

        public void Init(IEventManager _eventManager)
        {
            RegisterEvents();
            SetDefaultCommand();
            selectedBehaviourList = SelectHandler.Instance().GetSelectedEntities();
            
            var classes = new List<Type>(Assembly.GetAssembly(typeof(GroupCommand))
               .GetTypes()
               .Where(t => t.IsSubclassOf(typeof(GroupCommand))).ToArray()).ToArray();

            foreach (var cClass in classes)
            {
                var attr = cClass.GetCustomAttribute<CommandRealization>();
                if (attr == null)
                {
                    Debug.LogWarning("Plese add CommandRealization attribute to " + cClass.Name);
                }
                else
                {
                    commandRealizations.Add(attr.Command, attr);
                    MethodInfo info;
                    Type type = cClass;
                    
                    do
                    {
                        info = type.GetMethod(attr.CanBeProcessedFunctionName);
                        type = type.BaseType;
                    }
                    while (info == null);
                    
                    commandComporationMethod.Add(attr.Command, info);
                    info = null;
                    type = cClass;
                    
                    do
                    {
                        info = type.GetMethod(attr.CanBeProcessedMenuCommandFunctionName);
                        type = type.BaseType;

                    }
                    while (info == null);
                    
                    commandComporationMenuMethod.Add(attr.Command, info);
                    info = null;
                    type = cClass;
                    
                    do
                    {
                        info = type.GetMethod(attr.InteractableMenuCommandFunctionName);
                        type = type.BaseType;
                    }
                    while (info == null);
                    
                    commandComporationInteractableMenuMethod.Add(attr.Command, info);
                    commandRealizationTypes.Add(attr.Command, cClass);
                }
            }
        }

        private void RegisterEvents()
        {
            SelectHandler.Instance().OnSelectionListChanged += SetBehaviourList;
            SelectHandler.Instance().targetHandler.OnClick += ProcessCurentCommand;
        }

        /// <summary>
        /// Самая подходящая комманда которая выполниться по клику если не задана комманда кнопками
        /// </summary>
        /// <param name="targetInfo"> Информация о точке на которую кликнули </param>
        /// <returns></returns>
        public Command FindCommandForThisPoint(TargetInfo targetInfo)
        {
            return FindCommandForThisPoint(GetAvailableCommands(this.selectedBehaviourList), this.selectedBehaviourList, targetInfo);
        }

        /// <summary>
        /// Самая подходящая комманда которая выполниться по клику если не задана комманда кнопками
        /// </summary>
        /// <param name="behaviourList"> Юниты для которых вычисляем подходящую команду </param>
        /// <param name="targetInfo"> Информация о точке на которую кликнули </param>
        /// <returns></returns>
        public Command FindCommandForThisPoint(IEnumerable<InGameEntity> behaviourList, TargetInfo targetInfo)
        {
            return FindCommandForThisPoint(GetAvailableCommands(behaviourList), behaviourList, targetInfo);
        }
        protected Command FindCommandForThisPoint(List<CommandRealization> commands, IEnumerable<InGameEntity> behaviourList, TargetInfo targetInfo)
        {
            if (CurrentCommand != Command.None)
            {
                //Debug.Log(CurrentCommand+ " "+ commandComporationMethod[CurrentCommand]);
                if (CanBeProcessed(CurrentCommand, behaviourList, targetInfo))
                {
                    return CurrentCommand;
                }
            }
            commands.Sort();
            foreach (var command in commands)
            {
                if (CanBeProcessed(command.Command, behaviourList, targetInfo))
                {
                    return command.Command;
                }
            }
            return Command.None;
        }

        public bool CanBeProcessed(Command command)
        {
            return (bool)commandComporationMethod[command].Invoke(null, new object[] { selectedBehaviourList, targetInfo });
        }

        public bool CanBeProcessed(Command command, IEnumerable<InGameEntity> behaviourList)
        {
            return (bool)commandComporationMethod[command].Invoke(null, new object[] { behaviourList, targetInfo });
        }

        public bool CanBeProcessed(Command command, IEnumerable<InGameEntity> behaviourList, TargetInfo targetInfo)
        {
            return (bool)commandComporationMethod[command].Invoke(null, new object[] { behaviourList, targetInfo });
        }

        public bool CanBeProcessed(Command command, TargetInfo targetInfo)
        {
            return (bool)commandComporationMethod[command].Invoke(null, new object[] { selectedBehaviourList, targetInfo });
        }

        public bool CanBeProcessed(MenuCommand command)
        {
            return (bool)commandComporationMenuMethod[command.command].Invoke(null, new object[] { selectedBehaviourList, command, targetInfo });
        }

        public bool CanBeProcessed(MenuCommand command, IEnumerable<InGameEntity> behaviourList)
        {
            return (bool)commandComporationMenuMethod[command.command].Invoke(null, new object[] { behaviourList, command, targetInfo });
        }

        public bool CanBeProcessed(MenuCommand command, IEnumerable<InGameEntity> behaviourList, TargetInfo targetInfo)
        {
            return (bool)commandComporationMenuMethod[command.command].Invoke(null, new object[] { behaviourList, command, targetInfo });
        }

        public bool Interactable(MenuCommand command)
        {
            return (bool)commandComporationInteractableMenuMethod[command.command].Invoke(null, new object[] { selectedBehaviourList, command });
        }

        public bool Interactable(MenuCommand command, IEnumerable<InGameEntity> behaviourList)
        {
            return (bool)commandComporationInteractableMenuMethod[command.command].Invoke(null, new object[] { behaviourList, command });
        }

        /// <summary>
        /// Старт выбранной команды в выбранном месте
        /// </summary>
        public void ProcessCurentCommand()
        {
            StartCommand(CurrentCommand, selectedBehaviourList, targetInfo, false);
        }

        /// <summary>
        /// Старт команды в выбранном заранее месте
        /// </summary>
        public void ProcessCurentCommand(Command command)
        {
            StartCommand(command, selectedBehaviourList, targetInfo, false);
        }

        public void ProcessCurentCommand(MenuCommand command)
        {
            MenuCommand = command;
            StartCommand(command.command, selectedBehaviourList, targetInfo, false);
        }

        /// <summary>
        /// Старт подходящей команды для точки. Если комманда задана ранее выполниться она.
        /// </summary>
        /// <param name="targetInfo">Информация о точке на которую кликнули</param>
        public void ProcessCurentCommand(TargetInfo targetInfo)
        {
            this.targetInfo = targetInfo;

            if (selectedBehaviourList.Count > 0 && !(selectedBehaviourList.FirstOrDefault().Behaviour.Context.Owner is Building))
            {
                currentCommand = FindCommandForThisPoint(GetAvailableCommands(selectedBehaviourList), this.selectedBehaviourList, targetInfo);
                StartCommand(CurrentCommand, selectedBehaviourList, targetInfo, false);
                CurrentCommand = Command.None;
                MenuCommand = null;
            }
        }
        
        /// <summary>
        /// Список доступных комманд для обьектов
        /// </summary>
        /// <param name="behaviourList">>Выбранные обьекты</param>
        /// <returns></returns>
        public List<CommandRealization> GetAvailableCommands(IEnumerable<InGameEntity> behaviourList)
        {
            HashSet<BehaviourType> intersect = new HashSet<BehaviourType>();

            foreach (var besh in behaviourList)
            {
                if (intersect.Count > 0)
                {
                    intersect.IntersectWith(besh.Behaviour.AssignedBehaviours());
                }
                else
                {
                    intersect.UnionWith(besh.Behaviour.AssignedBehaviours());
                }
            }

            List<CommandRealization> availableCommands = new List<CommandRealization>(intersect.Count);

            foreach (var commands in commandRealizations)
            {
                foreach (var behaviourType in intersect)
                {
                    if (commands.Value.BehaviourType == behaviourType)
                    {
                        availableCommands.Add(commands.Value);
                    }

                }
            }

            return availableCommands;
        }
        
        /// <summary>
        /// Установка выбранных обьектов
        /// </summary>
        /// <param name="behaviourList">Выбранные обьекты</param>
        public void SetBehaviourList(IEnumerable<InGameEntity> behaviourList)
        {
           // this.selectedBehaviourList = new List<InGameEntity>(behaviourList.Count());
           // this.selectedBehaviourList.AddRange(behaviourList.ToArray());
        }

        /// <summary>
        /// Старт коммманы
        /// </summary>
        /// <param name="command">Сомманда котораяя выполниться</param>
        /// <param name="behaviourList">Выбранные обьекты</param>
        /// <param name="targetInfo">Информация о точке на которую кликнули</param>
        /// <param name="addToQueue"> Добавить к текущей в очередь</param>
        public void StartCommand(Command command, IEnumerable<InGameEntity> behaviourList, TargetInfo targetInfo, bool addToQueue = false)
        {
            GroupCommand commandRealization = null;
            if (!commandRealizationTypes.ContainsKey(command))
            {
                commandRealization = Activator.CreateInstance(commandRealizationTypes[Command.None]) as GroupCommand;
            }
            else
            {
                commandRealization = Activator.CreateInstance(commandRealizationTypes[command]) as GroupCommand;
            }

            commandRealization.menuCommand = MenuCommand;
            commandRealization.SetCommandTarget(targetInfo);
            commandRealization.AddToCurentCommand(behaviourList.ToArray());
            commandRealization.Ended += OnCommandEnded;
            commandRealization.Stoped += OnCommandEnded;
            ActiveCommands.Add(commandRealization);
            commandRealization.Start();
        }

        protected void OnCommandEnded(IGameActionExt baseActionExt)
        {
            //Debug.Log("OnCommandEnded");
            baseActionExt.Ended -= OnCommandEnded;
            baseActionExt.Stoped -= OnCommandEnded;
            baseActionExt.Stop();
            ActiveCommands.Remove(baseActionExt as GroupCommand);
        }
    }

    /// <summary>
    /// Унаследованные команды будут выполняться пока не остановишь
    /// </summary>
    [System.Serializable]
    public partial class LoopCommand : GroupCommand
    {
        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override bool MoveNextAction()
        {
            if (childs.Count == 0)
            {
                return false;
            }

            _currentIndex++;

            if (_currentIndex >= childs.Count)
            {
                _currentIndex = 0;
            }

            _current = childs[_currentIndex];
            if (!_current.InProgress) return true;

            return MoveNextAction();
        }
    }

    /// <summary>
    /// Унаследованные команды будут выполняться от первой к последней затем в обратную сторону
    /// </summary>
    [System.Serializable]
    public partial class PingPongCommand : GroupCommand
    {
        private int increment = 1;

        protected override void OnStart()
        {
            base.OnStart();

            _manualStop = false;
        }

        protected override bool MoveNextAction()
        {
            if (childs.Count == 0)
            {
                return true;
            }

            _currentIndex += increment;

            if (_currentIndex >= childs.Count || _currentIndex < 0)
            {
                increment *= -1;
                _currentIndex += increment;
            }

            _current = childs[_currentIndex];

            if (!_current.InProgress) return true;

            return MoveNextAction();
        }

        public override void BehaviourReady(ILogicBehaviour behaviour)
        {

        }
    }
    
    /// <summary>
    /// Базовый класс для всех комманд
    /// </summary>
    [System.Serializable]
    public partial class GroupCommand : SequenceAction
    {
        public MenuCommand menuCommand;
        private int readyBeshCount = 0;
        private bool removeFromList;

        protected List<InGameEntity> logicBehaviours = new List<InGameEntity>();
        protected TargetInfo targetInfo;
        
        public static bool CanBeProcessed(List<InGameEntity> behaviours, TargetInfo targetInfo)
        {
            return true;
        }

        public static bool CanBeProcessedMenuCommand(List<InGameEntity> behaviours, MenuCommand menuCommand, TargetInfo targetInfo)
        {
            return true;
        }

        public static bool InteractableMenuCommand(List<InGameEntity> behaviours, MenuCommand menuCommand)
        {
            return true;
        }

        public GroupCommand()
        {
        }

        public GroupCommand(IEnumerable<InGameEntity> behaviours, TargetInfo targetInfo, MenuCommand menuCommand = null) : base()
        {
            this.targetInfo = targetInfo;
            AddToCurentCommand(behaviours);
            this.menuCommand = menuCommand;
        }

        /// <summary>
        /// Установить цель команды
        /// </summary>
        /// <param name="targetInfo">Информация о точке</param>
        public virtual void SetCommandTarget(TargetInfo targetInfo)
        {
            this.targetInfo = targetInfo;
        }

        public virtual void BehaviourReady(ILogicBehaviour behaviour)
        {
            RemoveFromCurentCommand(behaviour.Context.Owner);
        }

        public virtual void RemoveFromCurentCommand(ILogicBehaviour behaviour)
        {
            RemoveFromCurentCommand(behaviour.Context.Owner);
        }

        public virtual void RemoveFromCurentCommand(InGameEntity behaviour)
        {
            logicBehaviours.Remove(behaviour);
            behaviour.Behaviour.OnClear -= BehaviourReady;
            behaviour.Behaviour.OnReady -= BehaviourReady;

            foreach (var child in childs)
            {
                (child as GroupCommand).RemoveFromCurentCommand(behaviour);
            }

            if (logicBehaviours.Count == 0)
            {
                End();
            }
        }

        /// <summary>
        /// Добавить обьекты которые выполняют команды
        /// </summary>
        /// <param name="behaviour">Обьекты содержащие логику</param>
        public virtual void AddToCurentCommand(IEnumerable<InGameEntity> behaviour)
        {
            foreach (var besh in behaviour)
            {
                AddToCurentCommand(besh);

            }
        }

        /// <summary>
        /// Добавить 1н обьект
        /// </summary>
        /// <param name="behaviour">Обьект содержащие логику</param>
        public virtual void AddToCurentCommand(InGameEntity behaviour)
        {
            logicBehaviours.Add(behaviour);

            if (behaviour.Behaviour.Context.ActiveCommand == null)
                behaviour.Behaviour.Context.ActiveCommand = this;
        }

        /// <summary>
        /// Добавить другую команду которая выполниться после этой
        /// </summary>
        /// <param name="action">Комманда</param>
        public virtual void AddToCurentCommand(IGameActionExt action)
        {
            Add(action);
        }

        protected override void OnStart()
        {
            if (CanBeProcessed(logicBehaviours, targetInfo))
            {
                for (int i = 0; i < logicBehaviours.Count; i++)
                {
                    if (logicBehaviours[i].Behaviour.Context.ActiveCommand == null)
                    {
                        logicBehaviours[i].Behaviour.Context.ActiveCommand = this;
                    }
                    else
                    {
                        if (logicBehaviours[i].Behaviour.Context.ActiveCommand.Parent != this.Parent)
                        {
                            (logicBehaviours[i].Behaviour.Context.ActiveCommand.Parent as GroupCommand).RemoveFromCurentCommand(logicBehaviours[i]);
                        }

                        logicBehaviours[i].Behaviour.Context.ActiveCommand = this;
                    }
                    
                    logicBehaviours[i].Behaviour.OnClear += RemoveFromCurentCommand;
                    logicBehaviours[i].Behaviour.OnReady += BehaviourReady;
                    OnBeshProcess(logicBehaviours[i], i);
                }

                _manualStop = true;
                base.OnStart();
            }
            else
            {
                End();
            }
        }

        protected virtual void OnBeshProcess(InGameEntity behaviour, int step)
        {

        }
        
        protected override void OnStop()
        {
            foreach (var besh in logicBehaviours)
            {
                besh.Behaviour.Context.ActiveCommand = null;
            }
            //logicBehaviours.Clear();
            base.OnStop();
        }

        protected override void OnEnd()
        {
            foreach (var besh in logicBehaviours)
            {
                besh.Behaviour.Context.ActiveCommand = null;
            }
            //logicBehaviours.Clear();
            base.OnEnd();
        }
    }

    //public 
    /// <summary>
    /// Комманда - заглушка
    /// </summary>
    [CommandRealization(Command.None, BehaviourType.None, 1)]
    public class NoneCommand : GroupCommand
    {
        protected override void OnBeshProcess(InGameEntity behaviour, int step)
        {
            behaviour.Behaviour.SelectBehaviour(BehaviourType.Idle);
        }
    }
}
