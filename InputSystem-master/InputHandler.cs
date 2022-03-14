using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using Game;
using UI.MainGame.Views.Chat;

namespace RTS.Controlls
{
    /// <summary>
    /// Перехватываем события мыши и клавиатуры и управляем клаассами SelectHandler и TargetHandler
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        public delegate void MouseDelegate(Vector2 position, KeyCode button);
        public static event MouseDelegate OnMouseDown;
        public static event MouseDelegate OnMouseUp;

        [SerializeField]
        private SelectHandler selectHandler;
        [SerializeField]
        private TargetHandler targetHandler;
        [SerializeField]
        private float timeForDoubleClick = 0.1f;

        Action<List<KeyCode>> PressedKeyChanged;

        private KeyCode addToExistsKey = KeyCode.LeftShift;
        private KeyCode similarKey = KeyCode.LeftControl;

        public static KeyCode mouseSelectKey = KeyCode.Mouse0;
        public static KeyCode mouseTargetKey = KeyCode.Mouse1;
       
        /// <summary>
        /// Параметры cостояния модуля
        /// </summary>
        private bool holdSelect=false;
        private bool holdTarget = false;
        private float lastClickTime = 0;
        private Vector2 downPosition;
        
        private readonly Array keys = Enum.GetValues(typeof(KeyCode));

        private bool PointerBlocked()
        {
            bool blocked = false;
            
            if (EventSystem.current.IsPointerOverGameObject())
            {              
                blocked = true;
            }
            
            return blocked;
        }

        void MouseTargetCheck()
        {
            targetHandler.UpdateCursore(Input.mousePosition, Input.GetKey(addToExistsKey));
            
            if (Input.GetKeyDown(mouseTargetKey))
            {
                if (CommandManager.Instance.CurrentCommand != Command.Build)
                {
                    downPosition = Input.mousePosition;
                    targetHandler.TargetShow(downPosition);
                    holdTarget = true;
                    OnMouseDown?.Invoke(downPosition, mouseTargetKey);
                }
            }

            if (Input.GetKeyUp(mouseTargetKey))
            {
                if (CommandManager.Instance.CurrentCommand == Command.Build)
                {
                    CommandManager.Instance.CancelBuildingPlacement();
                }
                else
                {
                    holdTarget = false;
                    targetHandler.ProcessTarget(Input.mousePosition, Input.GetKey(addToExistsKey));
                    OnMouseUp?.Invoke(downPosition, mouseTargetKey);
                }
            }

            if (holdTarget && !Input.GetKey(addToExistsKey) && CommandManager.Instance.CurrentCommand != Command.Build)
            {
                targetHandler.TargetRotate(downPosition, (Vector2)Input.mousePosition);
            }  
        }

        void MouseSelectCheck()
        {
            if (Input.GetKeyDown(mouseSelectKey))
            {
                downPosition = Input.mousePosition;
                
                if (!PointerBlocked())
                {
                    if (CommandManager.Instance.CurrentCommand != Command.Build)
                    {
                        selectHandler.StartSelect();
                        holdSelect = true;
                    }
                    else
                    {
                        Ray ray = Camera.main.ScreenPointToRay(downPosition);
                        RaycastHit from;
                        Physics.Raycast(ray, out from);
                        BuilderController.Instance.StartPlacement(new TargetInfo(from.point, null, from.point));
                    }

                    OnMouseDown?.Invoke(downPosition, mouseSelectKey);
                }
            }

            if (Input.GetKeyUp(mouseSelectKey))
            {
                if (holdSelect)
                    selectHandler.EndSelectByRect(Input.GetKey(addToExistsKey));

                if (!PointerBlocked())
                {
                    if (CommandManager.Instance.CurrentCommand == Command.Build)
                    {
                        Ray ray = Camera.main.ScreenPointToRay(downPosition);
                        RaycastHit from;
                        Physics.Raycast(ray, out from);
                        BuilderController.Instance.FinishPlacement(new TargetInfo(from.point, null, from.point));
                    }
                    else
                    {
                        if (downPosition == (Vector2)Input.mousePosition)
                        {
                            if (Input.GetKey(similarKey))
                            {
                                selectHandler.UnitMultiSelection(downPosition);
                            }
                            else
                            {
                                selectHandler.GetSingleTarget(downPosition, Input.GetKey(addToExistsKey));
                            }
                        }
                        
                        OnMouseUp?.Invoke((Vector2)Input.mousePosition, mouseSelectKey);
                    }
                }

                holdSelect = false;
            }
        }
        
        private void FixedUpdate()
        {
            //Обработчик удержания левой кнопки мыши
            if (holdSelect)
            {
                selectHandler.SelectByRect(downPosition, (Vector2)Input.mousePosition);
            }
        }
        
        void Update()
        {
            bool _ctr = Input.GetKey(KeyCode.LeftControl);
            bool _returnDown = Input.GetKeyDown(KeyCode.Return);
            bool _return = Input.GetKey(KeyCode.Return);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                switch (CommandManager.Instance.CurrentCommand)
                {
                    case Command.Build:
                        CommandManager.Instance.CancelBuildingPlacement();
                        break;
                }
            }

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                selectHandler.DeleteSelectedUnits();
            }
            
            if (Input.anyKeyDown)
            {
                List<KeyCode> curentPrassedKeys = new List<KeyCode>();
                curentPrassedKeys.Clear();
                
                foreach (KeyCode key in keys)
                {
                    if (Input.GetKey(key))
                    {
                        curentPrassedKeys.Add(key);
                    }    
                }
                
                if(PressedKeyChanged != null)
                {
                    PressedKeyChanged.Invoke(curentPrassedKeys);
                }
                
                InputCommandManager.instance.TryInvokeCommand(curentPrassedKeys);
            }

            // Закрываем чат
            if (_return && Input.GetKeyDown(KeyCode.Escape))
            {
                GameChatView.Instance.CloseChat();
            }

            // Открываем общий чат
            if (_ctr && _returnDown)
            {
                GameChatView.Instance.StartGeneralChat();
            }

            // Открыть чат с другом
            if (_return && Input.GetKeyDown(KeyCode.Tab))
            {
                GameChatView.Instance.StartFriendChat();
            }

            // Отправить сообщение в чат или открываем чат альянса
            if (_returnDown)
            {
                GameChatView.Instance.ReturnPressed();
            }

            // Обработчик нажатия клавиш [0..9]
            // Если есть выбранный объект UI текстовый ввод идет в него
            for (int i = 0; i < 10; i++)
            {
                if (Input.GetKeyDown($"{i}"))
                {
                  //  selectHandler.SquadInSelectionControll(i, Input.GetKey(addToExistsKey));
                }
            }
          
            MouseTargetCheck();
            
            if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject != null) return;
            
            MouseSelectCheck();
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                if (holdSelect)
                {
                    holdSelect = false;
                    selectHandler.EndSelectByRect(Input.GetKey(addToExistsKey));
                }

                if (holdTarget)
                {
                    holdTarget = false;
                    targetHandler.EndShowTarget();
                }
            }
        }
    }
}
