using Game.CommanderController;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Game.Pathfinding.Formation;
using CMS.Config;

namespace RTS.Controlls
{
    [RequireComponent(typeof(Button))]
    [ExecuteInEditMode]
    public class MenuCommand : MonoBehaviour
    {
        public Command command;
        public TypeOfFormation formation;
        public bool boolParam;
        public int entityType;
        public int SortingPriority = 0;
        public int intParam;
        public bool DisableIfCantBeProcessed = false;
        public bool ProcessCommandOnClick = true;
        public bool CanUseForMiltiplyChoose = false;
        public BaseScriptableDrowableItem config;
        public Image icon;
        public InputCommand inputCommand;

        private Image img;
        private Button btn;

        public bool Interactable{
            get
            {
                return btn.interactable;
            }
            set
            {
                btn.interactable = value;
                if (img)
                {
                    Color temp = img.color;
                    temp.a = !value ? 0.2f : 1f;
                    img.color = temp;
                }  
            }
        }

        void OnCommandInvoke()
        {
            if(btn.onClick != null && btn.interactable)
            {
                btn.onClick.Invoke();
            }
        }

        void OnEnable()
        {
            if(btn == null)
            {
                img = transform.GetChild(0).GetComponent<Image>();
                btn = GetComponent<Button>();
            }
               
            if(InputCommandManager.instance != null)
            {
                InputCommandManager.instance.CommandEvents[inputCommand] += OnCommandInvoke;
            }
        }

        void OnDisable()
        {
            if (InputCommandManager.instance != null)
            {
                InputCommandManager.instance.CommandEvents[inputCommand] -= OnCommandInvoke;
            }
        }
    }
}
