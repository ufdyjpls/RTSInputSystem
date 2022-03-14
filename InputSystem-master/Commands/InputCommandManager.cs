using Settings.ControllSettings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public enum InputCommand
{
    NONE = 0,
    // Камера
    //MOUSE_WHEEL scroll event - ±Высота камеры
    CameraUp = 1,
    CameraDown = 2,
    //SPACE - Перемещение камеры к выделенному объекту или общему центру.
    CameraCenter,
    //Home - Переместить камеру к месту начала игры.
    CameraHome,
    // Чат
    //Enter - Открыть чат с пометкой [Общий] для ввода сообщения. Последующее нажатие Enter отправляет сообщение всем игрокам текущей партии, либо отменяет ввод, если не было введено ни одного символа.
    ChatOpenGeneral,
    //Ctrl+Enter - Открыть чат с пометкой [Командный] для ввода сообщения. Последующее нажатие Enter отправляет сообщение команде игрока, либо отменяет ввод, если не было введено ни одного символа.
    ChatOpenTeam,
    //Enter -> TAB - Ответ на сообщение от игрока с сервера (не находится в матче). Реализовать можно по разному, либо сделать выбор ника из выпадающего списка, либо перебор с помощью стрелочек.
    ChatResponse,
    //Enter -> Esc - Закрыть чат.
    ChatClose,
    //Общие
    //F8 - Открыть подробное описание матча (взятые бонусы, рейтинг).
    GeneralOpenGameInfo,
    //F9 или Esc - Открыть меню игры.
    GeneralOpenGameMenu,
    //F10 - Открыть меню передачи ресурсов союзникам.
    GeneralOpenTeamTrade,
    //Pause/Break - Поставить паузу (работает только в одиночной игре).
    GeneralPause,
    //Alt+M - Скрыть/показать миникарту.
    GeneralToggleMinimap,
    //Alt+N - Отключить/включить звук
    GeneralToggleSound,
    //Alt+L - Скрыть все окна GUI (вид, в котором удобно сделать скриншот или посмотреть, что находится под элементами управления). Снимается нажатием любой клавиши.
    GeneralToggleInterface,
    //Alt+P - Отключить/включить отображение деревьев и камней на миникарте.
    GeneralToggleMinimapTreeAndRock,
    //Alt+O -Отключить/включить отображение шахт на миникарте.
    GeneralToggleMinimapMine,
    //Alt+U - Отключить/включить информацию об игроках и командах над миникартой.
    GeneralTogglePlayerInfo,
    //Alt+I - Сделать цвет всех союзных юнитов одним цветом, а вражеских юнитов другим.
    GeneralToggleUnitColor,
    //    Выделение и перемещение
    //Shift+ПКМ (Идти, Атаковать, Строить, Добывать) - выполнение стека действий в указанном порядке (вплоть до 12 приказов).
    SelectionAddCommand,
    //Del - Убить выбранного юнита/уничтожить здание.
    SelectionKill,
    //Ctrl+A - Выбрать все сухопутные войска, за исключением юнитов, выполняющих действия по охране объекта.'
    SelectionSelectAll,
    //Ctrl+Alt+A - Выбрать все сухопутные войска, за исключением крестьян и юнитов, выполняющих действия по охране объекта.
    SelectionSelectAllSoldier,
    //Ctrl+X - Выбрать весь флот.
    SelectionSelectAllFleet,
    //Ctrl+B - Выбрать все здания.
    SelectionSelectAllBuildings,
    //Ctrl+Z - Выбрать всех юнитов/здания данного типа (работает только в том случае, если в выделении находится один тип объекта). 
    SelectionSelectAllType,
    //Ctrl+P или ~ - Выбрать всех свободных крестьян.
    SelectionSelectAllFreeWorker,
    //Ctrl+M - Выбрать все незаполненные шахты.
    SelectionSelectAllFreeMine,
    //Тренировка юнитов и торговля
    //ЛКМ - Заказать 1 юнита для постройки.
    TrainingProduce1,
    //Shift+ЛКМ - Заказать 5 юнитов для постройки.
    TrainingProduce5,
    //Ctrl+Alt+ЛКМ - Заказать 100 юнитов для постройки.
    TrainingProduce100,
    //Ctrl+ЛКМ - Заказать бесконечное количество для постройки.
    TrainingProduceAll,
    //ПКМ - Убрать 1 юнита из очереди или снять бесконечность. 
    TrainingRemove1,
    //Shift+ПКМ - Убрать 5 юнитов из очереди или снять бесконечность. 
    TrainingRemove5,
    //Ctrl+Alt+ПКМ - Убрать 100 юнитов из очереди или снять бесконечность. 
    TrainingRemove100,
    //Ctrl+ПКМ - Убрать любое количество юнитов из очереди. 
    TrainingRemoveaAll,
    //Война
    //A+ПКМ - Идти к цели, убивая всех на своём пути.
    //Q - Запретить атаку
    WarDisableAttack,
    //W - Разрешить атаку
    WarEnableAttack,
    //S - Держать позиции
    WarHold,
    //C - Разойтись
    WarDismiss,
    //G - Охрана
    WarGuard,
    //F - Патрулировать
    WarPatrol,
    //Ctrl+[0-9] - Объединить выделенных солдат/здания/флот в группу для быстрого доступа на горячие клавиши [0-9].
    WarSetGroup1,
    //Ctrl+[0-9] - Объединить выделенных солдат/здания/флот в группу для быстрого доступа на горячие клавиши [0-9].
    WarSetGroup2,
    //Ctrl+[0-9] - Объединить выделенных солдат/здания/флот в группу для быстрого доступа на горячие клавиши [0-9].
    WarSetGroup3,
    //Ctrl+[0-9] - Объединить выделенных солдат/здания/флот в группу для быстрого доступа на горячие клавиши [0-9].
    WarSetGroup4,
    //Ctrl+[0-9] - Объединить выделенных солдат/здания/флот в группу для быстрого доступа на горячие клавиши [0-9].
    WarSetGroup5,
    //Ctrl+[0-9] - Объединить выделенных солдат/здания/флот в группу для быстрого доступа на горячие клавиши [0-9].
    WarSetGroup6,
    //Ctrl+[0-9] - Объединить выделенных солдат/здания/флот в группу для быстрого доступа на горячие клавиши [0-9].
    WarSetGroup7,
    //Ctrl+[0-9] - Объединить выделенных солдат/здания/флот в группу для быстрого доступа на горячие клавиши [0-9].
    WarSetGroup8,
    //Ctrl+[0-9] - Объединить выделенных солдат/здания/флот в группу для быстрого доступа на горячие клавиши [0-9].
    WarSetGroup9,
    //Ctrl+Shift+[0-9] - Создать/Дополнить существующую группу.

    //Строительство
    //Q - Построить здание [1,1] (Дом).
    BuildHome,
    //W - Построить здание [2,1] (Мельница.
    BuildMill,
    //E - Построить здание [3,1] (Склад).
    BuildWarehouse,
    //R - Построить здание [4,1] ((Шахта).
    BuildGoldMine,
    //???? - Построить здание [4,1] ((Шахта).
    BuildIronMine,
    //T - Построить здание [5,1] (Храм).
    BuildTemple,
    //Y - Построить здание [6,1] (Порт).
    BuildPort,
    //S - Построить здание [1,2] (Рынок).
    BuildMarket,
    //D - Построить здание [2,2] (Казарма 17 века).
    BuildBarracks17,
    //X - Построить здание [2,3] (Казарма 18 века).
    BuildBarracks18,
    //F - Построить здание [3,2] (Кузница).
    BuildForge,
    //G - Построить здание [4,2] (Конюшня).
    BuildStable,
    //H - Построить здание [5,2] (Артиллерийское депо).
    BuildArtilleryDepot,
    //J - Построить здание [6,2] (Академия).
    BuildAcademy,
    //Z - Построить здание [1,3] (Дипломатический центр).
    BuildDiplomaticCenter,
    //C - Построить здание [3,3] (Городской центр).
    BuildCityCenter,
    //N - Построить здание [6,3] (Каменная стена).
    BuildRockWall,
    //v - построить здание [4,3] (У Пруссии нет этого здания).

    //B - Построить здание [5,3] (У Пруссии нет этого здания).

    //Ctrl+[0-9] - Объединить выделенных солдат/здания/флот в группу для быстрого доступа на горячие клавиши [0-9].
    WarGetGroup0,
    //Ctrl+[0-9] - Объединить выделенных солдат/здания/флот в группу для быстрого доступа на горячие клавиши [0-9].
    WarGetGroup1,
    //Ctrl+[0-9] - Объединить выделенных солдат/здания/флот в группу для быстрого доступа на горячие клавиши [0-9].
    WarGetGroup2,
    //Ctrl+[0-9] - Объединить выделенных солдат/здания/флот в группу для быстрого доступа на горячие клавиши [0-9].
    WarGetGroup3,
    //Ctrl+[0-9] - Объединить выделенных солдат/здания/флот в группу для быстрого доступа на горячие клавиши [0-9].
    WarGetGroup4,
    //Ctrl+[0-9] - Объединить выделенных солдат/здания/флот в группу для быстрого доступа на горячие клавиши [0-9].
    WarGetGroup5,
    //Ctrl+[0-9] - Объединить выделенных солдат/здания/флот в группу для быстрого доступа на горячие клавиши [0-9].
    WarGetGroup6,
    //Ctrl+[0-9] - Объединить выделенных солдат/здания/флот в группу для быстрого доступа на горячие клавиши [0-9].
    WarGetGroup7,
    //Ctrl+[0-9] - Объединить выделенных солдат/здания/флот в группу для быстрого доступа на горячие клавиши [0-9].
    WarGetGroup8,
    //Ctrl+[0-9] - Объединить выделенных солдат/здания/флот в группу для быстрого доступа на горячие клавиши [0-9].
    WarGetGroup9,
    //Ctrl+Shift+[0-9] - Создать/Дополнить существующую группу.
    //Ctrl+[0-9] - Объединить выделенных солдат/здания/флот в группу для быстрого доступа на горячие клавиши [0-9].
    WarSetGroup0,
}

public class InputCommandAttribute : Attribute
{
    public InputCommand[] inputCommands;
    public InputCommandAttribute(InputCommand command) {

        inputCommands = new InputCommand[] { command };
    }
    public InputCommandAttribute(InputCommand[] commands)
    {
        inputCommands = commands;
    }
}

public partial class InputCommandManager : MonoBehaviour
{
    public Dictionary<InputCommand, Action> CommandEvents = new Dictionary<InputCommand, Action>();
    public Action<InputCommand> CommandStarted;
    
    public static InputCommandManager instance;
    [SerializeField]
    private InputManager inputManager = new InputManager();

    // Use this for initialization
    void Awake ()
    {
        instance = this;
        inputManager.Init();

        MethodInfo[] definedMethods = typeof(InputCommandManager).GetMethods();

        definedMethods = definedMethods.Where(x => InputCommandAttribute.IsDefined(x,typeof(InputCommandAttribute))).ToArray();

        foreach (InputCommand command in Enum.GetValues(typeof(InputCommand)))
        {
            CommandEvents[command] = () => { };
        }

        Array inputCommands = Enum.GetValues(typeof(InputCommand));

        foreach (MethodInfo methodInfo in definedMethods)
        {
            InputCommandAttribute attrebute = methodInfo.GetCustomAttribute<InputCommandAttribute>();
            foreach (var command in attrebute.inputCommands)
            {
                CommandEvents[command] += ()=> { methodInfo.Invoke(this,null); };
            }
           
        }
    }
    
    public void TryInvokeCommand(List<KeyCode> keyCodes)
    {
        InputCommand command = inputManager.GetInputCommand(keyCodes);
        if(command != InputCommand.NONE)
        {
            InvokeCommand(command);
        }
    }
    
    public void OnEnable()
    {
        
    }
    
    public void OnDisable()
    {

    }
    
    public void InvokeCommand(InputCommand command)
    {
        if(CommandEvents[command]!= null)
            CommandEvents[command].Invoke();
        if(CommandStarted != null)
            CommandStarted.Invoke(command);
    }

}

