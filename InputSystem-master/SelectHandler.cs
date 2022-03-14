using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Objects;
using Assets.Scripts.Game.InputSystem;
using Game.CommanderController;
using Assets.Scripts.Game.Pathfinding.Formation;
using Assets.Scripts.Game;
using Game.Picking;
using System.Linq;
using Game.Objects.Logic;
using System;
using Game.World;

namespace RTS.Controlls
{
	[Serializable]
	public class SelectGroup
	{
		public List<String> types = new List<String>();
		public List<BehaviourType> activeBehaviours = new List<BehaviourType>();
	}

	/// <summary>
	/// Класс отвечающий за выбор Юнитов
	/// Получение списка всех сущностей текущего игрока через инициализацию.
	/// Init(List<InGameEntity> lEntities)  
	/// Выбирает юниты по одному и областью
	/// Выбранным юнитам отправляет gameObject.SendMessage("SelectEntity", true)
	/// Если выделение снято отправляет gameObject.SendMessage("SelectEntity", false)
	/// Всем кто подписан отправляется событие OnSelectionChanged 
	/// left shift+[0..9] Сохранить выделенных для последущего выбора нажатием клавиш [0..9]
	/// left strl + мышь выбираются подобные
	/// Получить список выбранных на экране List<InGameEntity> GetSelectedEntities() 
	/// </summary>

	public class SelectHandler : MonoBehaviour, ISelectHandler
	{
	/// <summary>
	/// selectActiv- Активация, деактивация режима выбора мышкой
	/// Выбор отряда по номеру активен всегда
	/// </summary>

	public TargetHandler targetHandler;
	public float maxDistance = 1000;
	public LayerMask selectableLayerMask;
	public Color borderColor = Color.green;
	public EntityPicker entityPicker;
	public List<SelectGroup> selectGroup = new List<SelectGroup>();

	public event SelectDelegate OnSelectionChanged;
	public event SelectListDelegate OnSelectionListChanged;

	public readonly Dictionary<int, HashSet<InGameEntity>> squads = new Dictionary<int, HashSet<InGameEntity>>();

	private bool selectByRect = false;
	private Rect selectionRect;
	private bool SelectActiv = false;
	private Color rectColor;
	private static SelectHandler instance;
	private readonly List<InGameEntity> selectedObjects = new List<InGameEntity>();
	private HashSet<InGameEntity> list;
	private bool OnStartIsSelect;
	private Vector2 downPosition;
	private Vector3 downWorldPosition;
	private Vector3 upWorldPosition;
	private HashSet<SquadNavAgent> squad = new HashSet<SquadNavAgent>();
	private List<HashSet<InGameEntity>> selectedObjectByGroup;

	[SerializeField]
	private Camera mainCamera;

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

	#region {INIT}

	public static SelectHandler Instance()
	{
		if (instance == null)
		{
			instance = FindObjectOfType<SelectHandler>();
		}

		return instance;
	}

	private void Awake()
	{
		rectColor = new Color(borderColor.r, borderColor.g, borderColor.b, .1f);
		instance = this

		if (targetHandler == null)
		{
			targetHandler = FindObjectOfType<TargetHandler>();
		}
	}

	public void Init()
	{
		///TODO Сделать последовательное создание обьектов с единной точкой входа - гаме бутстрап, без кучи синглетонов
		list = GameBootstrap.Instance.GameScene.MyPlayer.GetOwnedInGameObjects();
		SelectActiv = true;
		SquadDictionary_Init();
	}

	void OnEnable()
	{
		SelectActiv = true;
	}

	void OnDisable()
	{
		SelectActiv = false;
		selectByRect = false;
		//InputHandler.OnKey -= onKey;
	}

	#endregion

	#region {DRAW RECT}

	void OnGUI()
	{
		if (SelectActiv && selectByRect)
		{
			SelectingUtil.DrawScreenRect(selectionRect, rectColor);
			SelectingUtil.DrawScreenRectBorder(selectionRect, 1, borderColor);
		}

		if (Input.GetKey(KeyCode.H))
		{
			foreach (InGameEntity target in GameBootstrap.Instance.GameScene.MyPlayer.GetOwnedInGameObjects())
			{
				GUI.Label(new Rect(ScreenPos(target).x, ScreenPos(target).y, 300, 30), "Health " + target.Health);
			}

			foreach (InGameEntity target in GameBootstrap.Instance.GameScene.NeutralPlayer.GetOwnedInGameObjects())
			{
				GUI.Label(new Rect(ScreenPos(target).x, ScreenPos(target).y, 300, 30), "Health " + target.Health);
			}
		}
	}

	#endregion

	#region {SELECT}

	public void StartSelect()
	{
		OnStartIsSelect = (selectedObjects.Count > 0);
	}

	public InGameEntity GetSingleTarget(Vector2 pos, bool addToExists)
	{
		//Ищем здания используя entityPicker
		InGameEntity target = entityPicker.GetEntityUnderCursor();

		if ((target != null) && ((selectableLayerMask & (1 << target.gameObject.layer)) != 0))
		{
			if (!addToExists)
			{
				ClearSelectedUnits();
				Sellect(target, true, true);
			}
			else
			{
				if (CanAdd(target))
				{
					Sellect(target, true, true);
				}
			}

			OnSelectionListChanged?.Invoke(selectedObjects);

			return target;
		}

	    	//Поиск лучом
		Ray ray = MainCamera.ScreenPointToRay(pos);
		RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, selectableLayerMask);

		for(int i = 0; i < hits.Length; ++i)
		{
			var t = hits[i].transform.GetComponent<InGameEntity>();

			if (!t ) continue;

			target = t;

			if (!addToExists)
			{
			    ClearSelectedUnits();
			    Sellect(target, true, true);
			}
			else
			{
			    if (CanAdd(target))
			    {
				Sellect(target, true);
			    }
			}

			break;
		}

		if (!addToExists && target == null)
		{
			ClearSelectedUnits();
		}

		OnSelectionListChanged?.Invoke(selectedObjects);

		return target;
	}

	public void UnitMultiSelection(Vector2 pos)
	{
	    InGameEntity target = GetSingleTarget(pos, false);

	    if (target != null && target is DynamicEntity)
	    {
		Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);

		foreach (var item in list)
		{
			if (item.ConfigId == target.ConfigId && screenRect.Contains(ScreenPos(item)))
			{
				Sellect(item, true);
			}
		}
	    }
	}

	#endregion

	#region [ Select by Rect ]

	public Rect GetWorldRect(Vector3 WorldDownPosition, Vector3 WorldUpPosition)
	{
		return new Rect(Mathf.Min(WorldUpPosition.x, WorldDownPosition.x),
			Mathf.Min(WorldUpPosition.z, WorldDownPosition.z),
			Mathf.Max(WorldUpPosition.x, WorldDownPosition.x) - Mathf.Min(WorldUpPosition.x, WorldDownPosition.x),
			Mathf.Max(WorldUpPosition.z, WorldDownPosition.z) - Mathf.Min(WorldUpPosition.z, WorldDownPosition.z));
	}

	public void SelectByRect(Vector2 downPosition, Vector2 upPosition)
	{
		RaycastHit hit;

		if (Physics.Raycast(mainCamera.ScreenPointToRay(new Vector3(downPosition.x, downPosition.y)), out hit))
		{
		downWorldPosition = hit.point;
		}

		if (Physics.Raycast(mainCamera.ScreenPointToRay(new Vector3(upPosition.x, upPosition.y)), out hit))
		{
			upWorldPosition = hit.point;
		}

		selectByRect = true;
		selectionRect = new Rect(Mathf.Min(upPosition.x, downPosition.x),
			Screen.height - Mathf.Max(upPosition.y, downPosition.y),
			Mathf.Max(upPosition.x, downPosition.x) - Mathf.Min(upPosition.x, downPosition.x),
			Mathf.Max(upPosition.y, downPosition.y) - Mathf.Min(upPosition.y, downPosition.y));

	    return;
	}

	public void EndSelectByRect(bool addToExists)
	{
		if (selectionRect.size.magnitude > 1)
		{
			if (!addToExists)
			{
				ClearSelectedUnits();
			}

			selectedObjectByGroup = new List<HashSet<InGameEntity>>(selectGroup.Count);

			for (int i = 0; i < selectGroup.Count; i++)
			{
			    selectedObjectByGroup.Add(new HashSet<InGameEntity>());
			}

			Rect worldRect = GetWorldRect(downWorldPosition, upWorldPosition);

			Collider[] colliders = Physics.OverlapBox(new Vector3(worldRect.center.x, 0, worldRect.center.y), new Vector3(worldRect.width / 2, 100, worldRect.height / 2),Quaternion.identity, selectableLayerMask);

			foreach (var obj in colliders)
			{
			    InGameEntity target = obj.GetComponent<InGameEntity>();
			    //if (target == null || target.gameObject == null || (selectableLayerMask & (1 << target.gameObject.layer)) == 0) continue;
			    //if (worldRect.Contains(new Vector2(target.transform.position.x, target.transform.position.z)))
			    {
				for (int i=0; i< selectGroup.Count; i++)
				{
				    for (int j = 0; j < selectGroup[i].types.Count; j++)
				    {
					if (selectGroup[i].types[j] == target.GetType().Name && selectGroup[i].activeBehaviours[j] == target.Behaviour.CurrentBehaviourType)
					{
						selectedObjectByGroup[i].Add(target);
					}
				    }
				}
			    }
			}

			if(selectedObjectByGroup.FirstOrDefault(x => { return x.Count > 0; }) == null)
			{
			    foreach (var obj in colliders)
			    {
				InGameEntity target = obj.GetComponent<InGameEntity>();

				if (worldRect.Contains(new Vector2(target.transform.position.x, target.transform.position.z)))
				{
				    selectedObjectByGroup[0].Add(target);
				}
			    }
			}

			HashSet<InGameEntity> mostPriority = selectedObjectByGroup.FirstOrDefault(x => { return x.Count > 0; });

			if(mostPriority != null)
			{
				foreach (InGameEntity target in mostPriority)
				{
					Sellect(target, true, mostPriority.Count == 1);
				}

				OnSelectionListChanged?.Invoke(selectedObjects);
			}   
		}

		downPosition = Vector3.zero;
		downWorldPosition = Vector3.zero;
		upWorldPosition = Vector3.zero;
		selectByRect = false;
	}

	#endregion

	#region {SELECT UTILS}

	/// <summary>
	///  Можно ли добавлять сущность к списку выбранных
	/// </summary>
	private bool CanAdd(InGameEntity target)
	{
		if (!selectedObjects.Any() && target.isActiveAndEnabled)
		{
			return true;
		}

		foreach (InGameEntity entity in selectedObjects)
		{
			if ((entity is DynamicEntity && target is DynamicEntity)|| (entity is Building && target is Building))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		return true;
	}

	public void ClearSelectedUnits()
	{
	    foreach (InGameEntity target in selectedObjects)
	    {
		target.IsSelected = false;
		OnSelectionChanged?.Invoke(target, false);
	    }

	    squad.Clear();
	    selectedObjects.Clear();
	    OnSelectionChanged?.Invoke(null, false);
	    OnSelectionListChanged?.Invoke(selectedObjects);
	}

	public void RemoveUnit(ILogicBehaviour target)
	{
		Sellect(target.Context.Owner, false, true);
	}

	public void Sellect(InGameEntity target, bool selected, bool singleTarget = false, bool InvokeListChange = false)
	{
	    if (!target.isActiveAndEnabled)
	    {
		selected = false;
	    }

	    if (target is Unit)
	    {
		Unit unit = ((Unit)target);

		if (unit.OwnerSquad != null)
		{
		    if (!squad.Contains(unit.OwnerSquad))
		    {
			squad.Add(unit.OwnerSquad);
			SetSelected(unit.OwnerSquad, selected, InvokeListChange);
		    }
		}
		else
		{
		    SetSelected(target, selected, InvokeListChange);
		}
	    }
	    else
	    {
		SetSelected(target, selected, InvokeListChange);
	    }
	}

	public void SetSelected(InGameEntity target, bool selected, bool InvokeListChange=false)
	{
		if (selected)
		{
			if (!target.IsSelected && !target.IsInternal)
			{
				selectedObjects.Add(target);
				target.Behaviour.OnClear += RemoveUnit;
				target.IsSelected = true;
	#if UNITY_EDITOR
				if (Input.GetKey(KeyCode.Tab))
				{
					UnityEditor.Selection.activeTransform = target.transform;
				}
	#endif
				OnSelectionChanged?.Invoke(target, true);

				if (InvokeListChange)
				{
					OnSelectionListChanged?.Invoke(selectedObjects);
				}
			}
	    }
	    else
	    {
		if (target != null && target.IsSelected)
		{
			selectedObjects.Remove(target);
			target.Behaviour.OnClear -= RemoveUnit;
			target.IsSelected = false;
			OnSelectionChanged?.Invoke(target, false);

			if (InvokeListChange)
			{
				OnSelectionListChanged?.Invoke(selectedObjects);
			}
		}
	    }
	}

	private Vector2 ScreenPos(InGameEntity target)
	{
		Vector2 pos = MainCamera.WorldToScreenPoint(target.transform.position);
		return pos = new Vector2(pos.x, Screen.height - pos.y);
	}

	#endregion

	#region SET_UNIT_SQUADS : [ HOTKEY + NUMBERS_(0-9) ]

	public void SquadInSelectionControll(int numSquad, bool addToSquad)
	{
		//TODO: переделать для hashset
		//if (!list.ExistAndNotEmpty()) return;
		if (addToSquad)
		{
			SetUnitSquad(numSquad);
		}
		else
		{
			GetUnitSquadFrom(numSquad);
		}
	}

	private void SquadDictionary_Init()
	{
		if (squads.Count < 1)
		{
			for (int i = 0; i < 10; squads.Add(i++, new HashSet<InGameEntity>()));
		}
	}

	private void SetUnitSquad(int squadNum)
	{
		if (squads[squadNum].Any())
		{
			squads[squadNum].Clear();
		}

		foreach (var x in selectedObjects)
		{
			squads[squadNum].UnionWith(selectedObjects);
		}
	}

	private void GetUnitSquadFrom(int squadNum)
	{
		if (!squads[squadNum].Any()) return;

		ClearSelectedUnits();

		foreach (InGameEntity entity in squads[squadNum])
		{
			Sellect(entity, true);
		}

		OnSelectionListChanged?.Invoke(selectedObjects);
	}

	private void RemoveUnitFromSquad(InGameEntity entity)
	{
		foreach (int key in squads.Keys)
		{
			HashSet<InGameEntity> squad = squads[key];
			squads[key].Remove(entity);
		}
	}

	#endregion

	public List<InGameEntity> GetSelectedEntities()
	{
		return selectedObjects;
	}

	public void SelectAllFreeWorkers(HashSet<InGameEntity> workers)
	{
		ClearSelectedUnits();

		foreach (InGameEntity target in workers)
		{
			Sellect(target, true, workers.Count == 1);
		}

		OnSelectionListChanged?.Invoke(selectedObjects);
	}

	/// <summary>
	/// Какой смысл в множественном выделении шахт
	/// Sellect(target, true, mines.Count == 1); запрещает выделять много шахт
	/// </summary>
	/// <param name="mines"></param>
	public void SelectAllFreeMines(HashSet<InGameEntity> mines)
	{
		ClearSelectedUnits();

		foreach (InGameEntity target in mines)
		{
			Sellect(target, true, mines.Count == 1);
		}

		OnSelectionListChanged?.Invoke(selectedObjects);
	}

	public void InvokeSelectionListChanged()
	{
		OnSelectionListChanged?.Invoke(selectedObjects);
	}

	public void DeleteSelectedUnits()
	{
		while (selectedObjects.Count > 0)
		{
			InGameEntity entity = selectedObjects.First();
			SetSelected(entity, false);
			Building building = entity as Building;

			if (building)
			{
				if (building.ConfigId == "MineIron" || building.ConfigId == "MineGold")
				{
					building.Health = 0;
					building.DeathAction.Start();
				}
				else
				{
					building.MaxDamage();
				}
			}
			else
			{
				entity.Health = 0;
				entity.DeathAction.Start();
			}
		}

		OnSelectionListChanged?.Invoke(selectedObjects);
	}
	}
}
