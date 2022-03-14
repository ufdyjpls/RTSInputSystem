using UnityEngine;
using UnityEngine.UI;
using Game.CommanderController;
using Assets.Scripts.Game.EventSystem;
using Assets.Scripts.Game.EventSystem.CommandEvents;
using System.Linq;
using System.Collections.Generic;
using Game.Objects;
using CMS.Config;
using Assets.Scripts.Game;
using Game.Objects.Logic;
using UI.MainGame.Additional;

namespace RTS.Controlls
{
    class MenuHandler : Singleton<MenuHandler>
    {
        public GameObject gameMenu;

        private IEventManager eventManager;
        private Dictionary<Command,List<MenuCommand>> menuCommands;
        private List<MenuCommand> activeCommands = new List<MenuCommand>();
        
        #region [ TestOnly ]
        public GameObject testMenu;
        public GameObject testButtonPrefab;
        public RectTransform testButtonRect;
        private List<GameObject> testObjects = new List<GameObject>();
        private List<MonoBehaviour> testComponents = new List<MonoBehaviour>();
        
        public void AddTestMenu(string buttonText, GameObject activatedObject)
        {
            GameObject itemView = GameObject.Instantiate(testButtonPrefab, testButtonRect);
            TestListener(itemView.GetComponent<Button>(), activatedObject, buttonText);
            
            itemView.SetActive(true);
            gameMenu.SetActive(false);
            testMenu.SetActive(true);
            
            testObjects.Add(activatedObject);
        }

        public void AddTestComponent(string buttonText, MonoBehaviour component)
        {
            Debug.LogFormat("MenuHandler.AddTestComponent {0} count {1}", component, testComponents.Count);
            if (testComponents.Count == 0)
            {
                GameObject itemView = GameObject.Instantiate(testButtonPrefab, testButtonRect);
                
                itemView.GetComponent<Button>().onClick.AddListener(() =>
                {
                    foreach (MonoBehaviour test in testComponents)
                    {
                        test.enabled = !test.enabled;
                    }
                });
                
                itemView.SetActive(true);
                Text text = itemView.GetComponentInChildren<Text>();
                text.text = "VisualEffects";
            }
            component.enabled = false;
            testComponents.Add(component);
        }

        private void TestListener(Button button, GameObject activatedObject, string buttonText)
        {
            button.onClick.AddListener(() => {
                activatedObject.SetActive(!activatedObject.activeSelf);
            });
            activatedObject.SetActive(false);
            Text text = button.GetComponentInChildren<Text>();
            text.text = buttonText;
        }

        /// <summary>
        /// Временная активация кнопок меню
        /// TODO: переделать на словарь по группам юнитов
        /// TODO: переделать на маски команд сущностей
        /// </summary>
        public void ActivateButton(List<InGameEntity> entitys)
        {
            List<CommandRealization> commands = CommandManager.Instance.GetAvailableCommands(entitys);
            
            foreach (var activeCommand in activeCommands)
            {
                activeCommand.gameObject.transform.SetParent(BuildPanelManager.Instance.Parent.transform);
                activeCommand.gameObject.SetActive(false);
            }

            activeCommands = new List<MenuCommand>(menuCommands.Keys.Count);

            foreach (var command in commands)
            {
                if (menuCommands.ContainsKey(command.Command))
                {
                    foreach (var currentCommand in menuCommands[command.Command])
                    {
                        {
                            currentCommand.gameObject.transform.SetParent(BuildPanelManager.Instance.MainParent.transform);
                            currentCommand.gameObject.SetActive(true);
                            activeCommands.Add(currentCommand);
                        }
                    }
                }
            }

            foreach (MenuCommand currentCommand in activeCommands)
            {
                if (currentCommand.DisableIfCantBeProcessed)
                {
                    if (currentCommand.config is BuildingConfig)
                    {
                        currentCommand.gameObject.SetActive(true);
                    }
                    else
                    {
                        currentCommand.gameObject.SetActive(CommandManager.Instance.CanBeProcessed(currentCommand, entitys));
                        if (currentCommand.gameObject.activeSelf)
                        {
                            currentCommand.Interactable = CommandManager.Instance.Interactable(currentCommand);
                            currentCommand.gameObject.transform.SetParent(BuildPanelManager.Instance.MainParent.transform);
                        }
                        else
                        {
                            currentCommand.gameObject.transform.SetParent(BuildPanelManager.Instance.Parent.transform);
                        }

                    }
                }
            }
        }

        private void UpdateMenuCommandProcess()
        {
            foreach (MenuCommand menuCommand in activeCommands)
            {
                if (menuCommand.DisableIfCantBeProcessed)
                {
                    if (menuCommand.config is BuildingConfig)
                    {
                        menuCommand.Interactable = CommandManager.Instance.CanBeProcessed(menuCommand);
                    }
                    else 
                    {
                        menuCommand.gameObject.SetActive(CommandManager.Instance.CanBeProcessed(menuCommand));
                        
                        if (menuCommand.gameObject.activeInHierarchy)
                        {
                            menuCommand.Interactable = CommandManager.Instance.Interactable(menuCommand);
                        }
                    }
                }   
            }
        }
        
        public void DeactivateTest()
        {
            foreach (GameObject obj in testObjects)
            {
                obj.SetActive(false);
            }
        }

        #endregion

        public void Init(IEventManager _eventManager)
        {
            menuCommands = new Dictionary<Command,List<MenuCommand>>();
            eventManager = _eventManager;
            SelectHandler.Instance().OnSelectionListChanged += ActivateButton;
            Building.BuildingChanged += UpdateMenuCommandProcess;
            
            var menus = gameMenu?.GetComponentsInChildren<MenuCommand>();
            
            if (menus != null)
            {
                foreach (MenuCommand menuCommand in menus)
                {
                    if(!menuCommands.ContainsKey(menuCommand.command))
                    {
                        menuCommands.Add(menuCommand.command, new List<MenuCommand>());
                    }

                    switch (menuCommand.command)
                    {
                        case Command.Research:
                        
                            if (menuCommand.config == null)
                            {
                                AddButtonsFromTemplate<ResearchesProductionConfig>(menuCommand);
                            }
                            else
                            {
                                AddListener(menuCommand);
                                menuCommands[menuCommand.command].Add(menuCommand);
                            }
                            
                            break;
                            
                        case Command.RequestProduction:
                        
                            if (menuCommand.config == null)
                            {
                                AddButtonsFromTemplate<ProductionBlueprint>(menuCommand);
                            }
                            else
                            {
                                AddListener(menuCommand);
                                menuCommands[menuCommand.command].Add(menuCommand);
                            }
                            
                            break;
                            
                        case Command.SetGatherPoint:
                            AddListener(menuCommand);
                            menuCommands[menuCommand.command].Add(menuCommand);
                            Clicked clicked = menuCommand.GetComponent<Clicked>();
                            
                            if (clicked)
                            {
                                clicked.rightClick.AddListener(() =>
                                {
                                    CommandManager.Instance.MenuCommand = menuCommand;
                                    CommandManager.Instance.ProcessCurentCommand(Command.UndoSetGatherPoint);
                                });
                            }

                            break;

                        default:
                            AddListener(menuCommand);
                           menuCommands[menuCommand.command].Add(menuCommand);
                            break;
                    }
                    
                    menuCommands[menuCommand.command].Add(menuCommand);
                    menuCommand.gameObject.SetActive(false);
                }
            }
        }

        public void AddListener(MenuCommand menuCommand)
        {
            Button button = menuCommand.GetComponent<Button>();
            
            if (button == null) return;
            
            button.onClick.AddListener(() =>
            {
            
                if (menuCommand.ProcessCommandOnClick)
                {
                    CommandManager.Instance.ProcessCurentCommand(menuCommand);
                }
                else
                {
                    CommandManager.Instance.MenuCommand = menuCommand;
                    CommandManager.Instance.CurrentCommand = menuCommand.command;
                }
                
            });
            
            switch (menuCommand.command)
            {
                case Command.Build:
                    button.onClick.AddListener(() => { CommandManager.Instance.SelectBuildingPlace(menuCommand.config as BuildingConfig); });
                    break;
            }
        }

        private void AddButtonsFromTemplate<T>(MenuCommand menuCommand) where T : ProductionBlueprint
        {
            foreach(string configId in ScriptableList<T>.instance.GetIdList())
            {
                T config = ScriptableList<T>.instance.GetItemByID(configId);
                if (config.Icon)
                {
                    GameObject newButton = GameObject.Instantiate(menuCommand.gameObject, menuCommand.transform.parent);
                    newButton.name = ((config is ResearchesProductionConfig) ? "Research_":"Produce_") + configId;
                    MenuCommand newCommand = newButton.GetComponent<MenuCommand>();
                    newCommand.config = config;
                    newCommand.icon.sprite = config.Icon;
                    AddListener(newCommand);
                    menuCommands[newCommand.command].Add(newCommand);
                    
                    if (newCommand.command == Command.RequestProduction)
                    {
                        Clicked clicked = newButton.GetComponent<Clicked>();
                        if (clicked)
                        {
                            clicked.rightClick.AddListener(() =>
                            {
                                CommandManager.Instance.MenuCommand = newCommand;
                                CommandManager.Instance.ProcessCurentCommand(Command.UndoProduction);
                            });
                        }
                    }
                    
                    newCommand.gameObject.SetActive(false);
                }
            }
            
            menuCommand.gameObject.SetActive(false);
        }
    }
}
