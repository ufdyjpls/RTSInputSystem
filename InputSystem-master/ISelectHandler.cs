using Game.Objects;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Game.InputSystem
{
	public delegate void SelectDelegate(InGameEntity entity, bool selected);

    public delegate void SelectListDelegate(List<InGameEntity> entities);

	public interface ISelectHandler
	{
		event SelectDelegate OnSelectionChanged;
		void SetSelected(InGameEntity entity, bool selected, bool InvokeListChange);
		List<InGameEntity> GetSelectedEntities();
	}
}
