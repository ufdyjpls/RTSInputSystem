using System.Collections.Generic;
using UnityEngine;
using Game.Objects;
using Assets.Scripts.Game.Pathfinding.Formation;
using System.Linq;
using Utils;
using Game.Factorys;
using Game.CommanderController;

namespace RTS.Controlls
{
    [System.Serializable]
    public struct CursoreOptions
    {
        public Command command;
        public Texture2D texture;
        public Vector2 hotspot;
    }
    
    public partial class TargetHandler : MonoBehaviour, ITargetHandler
    {
        /// <summary>
        /// Управление указателем цели
        /// </summary>
        /// 
        public event ClickDelegate OnClick;
        public event ClickDelegate OnDown;
        public float maxDistance = 1000;
        public LayerMask enemyLayerMask;
        public int maxTargetCount = 15;
        public GameObject PlacePrefab;
        public VisualUnitSelection PlaceUnitPrefab;
        public float TimeDelayDrawPlace = 0.5f;
        public TargetInfo targetInfo;
        public event TargetListComplete OnTargetListComplete;
        
        public Camera MainCamera
        {
            get
            {
                if (mainCamera == null)
                {
                    mainCamera = Camera.main;
                }
                
                return mainCamera;
            }
        }
        
        private bool selectByRect = false;
        private Rect selectionRect;
        private bool selectActiv = false;
        private Dictionary<Command, CursoreOptions> cursores = new Dictionary<Command, CursoreOptions>();
        
        [SerializeField]
        private List<CursoreOptions> cursoreOptions = new List<CursoreOptions>();

        /// <summary>
        /// Рисуем положение в конечной точке атаки
        /// TimeTarget когда нажали ПКМ
        /// TimeDelayDrawPlace через сколько времени рисовать положение юнитов
        /// </summary>
        private GameObject targetPlace;
        private float timeTarget;
        private bool targetReady;
        private List<Vector3> targetList = new List<Vector3>();
        private bool OnStartIsSelect;
        private Camera mainCamera;
        private List<VisualUnitSelection> selectionObject = new List<VisualUnitSelection>();
        private bool isWindows;
        private Command lastCursoreCommand = Command.None;
        
        #region {EVENT HANDLER}
        
        void Start()
        {
            #if UNITY_STANDALONE_WIN
                isWindows = true;
            #elif UNITY_STANDALONE_OSX
                isWindows = false;
            #endif
            
            foreach(var option in cursoreOptions)
            {
                cursores[option.command] = option;
            }
            
            if (isWindows)
            {
                Cursor.SetCursor(cursores[Command.None].texture, cursores[Command.None].hotspot, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(cursores[Command.None].texture, cursores[Command.None].hotspot, CursorMode.ForceSoftware);
            }
        }

        public void UpdateCursore(Vector2 position, bool addToExists)
        {
            if (!targetReady)
            {
                targetInfo = GetIntersected(position);
                Command command = CommandManager.Instance.FindCommandForThisPoint(targetInfo);
                SetCursor(command);
            }
        }
        
        /// <summary>
        /// Указать цель
        /// </summary>
        public void TargetShow(Vector2 position)
        {
            //TODO: Нарисовать крестик
            timeTarget = Time.time;
            OnDown?.Invoke(targetInfo);
        }
        
        public void TargetRotate(Vector2 downPosition, Vector2 upPosition)
        {
            if (((Time.time - timeTarget) > TimeDelayDrawPlace))
            {
                Ray ray = Camera.main.ScreenPointToRay(downPosition);
                RaycastHit from;
                ray = Camera.main.ScreenPointToRay(upPosition);
                RaycastHit to;
                
                if (Physics.Raycast(ray, out to))
                {
                    List<InGameEntity> inGameEntities = SelectHandler.Instance().GetSelectedEntities();
                    Vector3[] points = PositionUtils.GetWorldPoints<SquareGrid>(inGameEntities, to.point- from.point, from.point);
                    
                    while(selectionObject.Count < points.Length)
                    {
                        selectionObject.Add(GameObjectPool.Instance.Take<VisualUnitSelection>(PlaceUnitPrefab));
                    }
                    
                    while (selectionObject.Count > points.Length)
                    {
                        GameObjectPool.Instance.Put(selectionObject[0]);
                        selectionObject.RemoveAt(0);
                    }

                    for (int i = 0; i < points.Length; i++)
                    {
                        selectionObject[i].transform.position = points[i];
                        selectionObject[i].transform.rotation = Quaternion.LookRotation(to.point- from.point, Vector3.up);
                    }
                    targetInfo = new TargetInfo(from.point, null, from.point - to.point);
                    targetReady = true;
                }
            }
        }
        
        public void SetCursor(Command command)
        {
            if(command != lastCursoreCommand)
            {
                if (cursores.ContainsKey(command))
                {
                    if(isWindows)
                    {
                        Cursor.SetCursor(cursores[command].texture, cursores[command].hotspot, CursorMode.Auto);
                    }
                    else
                    {
                        Cursor.SetCursor(cursores[command].texture, cursores[command].hotspot, CursorMode.ForceSoftware);
                    }
                }
                else
                {
                    if (isWindows)
                    {
                        Cursor.SetCursor(cursores[Command.None].texture, cursores[Command.None].hotspot, CursorMode.Auto);
                    }
                    else
                    {
                        Cursor.SetCursor(cursores[Command.None].texture, cursores[Command.None].hotspot, CursorMode.ForceSoftware);
                    }
                }
                
                lastCursoreCommand = command;
            }
        }
 
        /// <summary>
        /// Обработка указания цели при отпускании ПКМ
        /// TODO: добавить обработчик target.transform.localRotation
        /// </summary>
        public void ProcessTarget(Vector2 position, bool addToExists) 
        {
            OnClick.Invoke(targetInfo);
            EndShowTarget();
        }

        public void EndShowTarget()
        {
            while (selectionObject.Count > 0)
            {
                GameObjectPool.Instance.Put(selectionObject[0]);
                selectionObject.RemoveAt(0);
            }
            
            targetReady = false;
        }

        #endregion

        #region [ Get Intersected ]

        private TargetInfo GetIntersected(Vector2 pos)
        {
            InGameEntity target = null;
            Ray ray = MainCamera.ScreenPointToRay(pos);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, maxDistance))
            {
                target = hit.transform.GetComponent<InGameEntity>();
            }
            
            TargetInfo targetInfo = new TargetInfo(hit.point.Land(), target,Vector3.zero);

            if(Game.Picking.EntityPicker.Instance != null)
            {
                InGameEntity entity = Game.Picking.EntityPicker.Instance.PickEntity(pos);
                
                if(entity != null)
                {
                    target = entity;
                    targetInfo = new TargetInfo(entity.Position.Land(), entity, Vector3.zero);
                }
            }

            return targetInfo;
        }

        private InGameEntity GetIntersectedEntity(Vector2 pos)
        {
            InGameEntity target = null;
            Ray ray = MainCamera.ScreenPointToRay(pos);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, maxDistance))
            {
                target = hit.transform.GetComponent<InGameEntity>();
            }

            return target;
        }
        
        private InGameEntity GetIntersectedEntity(Vector2 pos, LayerMask mask)
        {
            InGameEntity target = null;
            Ray ray = MainCamera.ScreenPointToRay(pos);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, maxDistance, mask))
            {
                target = hit.transform.GetComponent<InGameEntity>();
            }

            return target;
        }

        private bool GetWorldIntersect(Vector2 pos, out Vector3 intersectPos)
        {
            intersectPos = Vector3.positiveInfinity;
            Ray ray = MainCamera.ScreenPointToRay(pos);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                intersectPos = hit.point;
                return true;
            }
            
            return false;
        }
        #endregion
    }
}
