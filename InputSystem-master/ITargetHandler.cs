using Game.Objects;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.Controlls
{
    public delegate void ClickDelegate(TargetInfo entity);

    /// <summary>
    /// Сформирован список целей
    /// </summary>
    public delegate void TargetListComplete(List<Vector3> targetList);
    
    public struct TargetInfo
    {
        public Vector3 Position;
        public InGameEntity GameEntity;
        public Building Building;
        public IWorkingPlaceContainer WorkingPlaceContainer;
        public Unit Unit;
        public ResourceEntity ResourceEntity;
        public Vector3 Direction;

        public TargetInfo(Vector3 position, InGameEntity entity, Vector3 direction)
        {
            Position = position;
            GameEntity = entity;
            Building = entity as Building;
            Unit = entity as Unit;
            ResourceEntity = entity as ResourceEntity;
            Direction = direction;
            WorkingPlaceContainer = entity as IWorkingPlaceContainer;
        }
    }
    
    public interface ITargetHandler
    {
        event ClickDelegate OnClick;
    }
}
