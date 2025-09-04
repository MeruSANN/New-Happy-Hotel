using HappyHotel.Action;
using HappyHotel.Action.Components;
using HappyHotel.Core.Grid.Components;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HappyHotel.Character.Extension
{
    public static class CharacterExtensions
    {
        [Button]
        public static void MoveTo(this DefaultCharacter character, Vector2Int position)
        {
            character.GetBehaviorComponent<GridObjectComponent>().MoveTo(position);
        }

        // 添加行动到队列
        [Button]
        public static bool AddAction(this DefaultCharacter character, IAction action)
        {
            var actionQueue = character.GetBehaviorComponent<ActionQueueComponent>();
            if (actionQueue != null) return actionQueue.AddAction(action);
            return false;
        }

        // 清空行动队列
        [Button]
        public static void ClearActionQueue(this DefaultCharacter character)
        {
            var actionQueue = character.GetBehaviorComponent<ActionQueueComponent>();
            if (actionQueue != null) actionQueue.Clear();
        }
    }
}