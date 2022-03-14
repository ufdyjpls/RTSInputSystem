using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;


namespace RTS.Controlls
{
    /// <summary>
    /// Перехватываем события мыши и отправляем тем кто на них подписан
    /// Должен располагаться на Канве, на прозрачной панели накрывающей весь экран
    /// </summary>

    public class MouseHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public delegate void MouseDelegate(Vector2 position, PointerEventData.InputButton button);

        public static event MouseDelegate OnMouseDown;
        public static event MouseDelegate OnMouseUp;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnMouseDown?.Invoke(eventData.position, eventData.button);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnMouseUp?.Invoke(eventData.position, eventData.button);
        }
    }
}
