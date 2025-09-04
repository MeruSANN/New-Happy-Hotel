using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Grid;
using HappyHotel.Core.Grid.Components;
using UnityEngine;

namespace HappyHotel.Prop
{
    // 跳跃道具：被触发时尝试将触发者移动到前方第二格
    public class JumpProp : ActivePlaceablePropBase
    {
        public override void OnTriggerInternal(BehaviorComponentContainer triggerer)
        {
            var gridObject = triggerer.GetBehaviorComponent<GridObjectComponent>();
            var directionComponent = triggerer.GetBehaviorComponent<DirectionComponent>();
            if (gridObject == null || directionComponent == null) return;

            var current = gridObject.GetGridPosition();
            var dir = directionComponent.GetDirectionVector();
            var target = current + dir * 2;

            // 仅当第二格可通过时执行跳跃，否则无效果
            if (GridObjectManager.Instance != null && GridObjectManager.Instance.IsValidMove(triggerer, target))
                GridObjectManager.Instance.MoveObject(triggerer, target);
        }
    }
}

