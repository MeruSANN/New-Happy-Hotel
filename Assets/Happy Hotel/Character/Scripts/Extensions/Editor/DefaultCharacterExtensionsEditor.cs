#if UNITY_EDITOR
using HappyHotel.Action;
using HappyHotel.Action.Settings;
using HappyHotel.Core;
using HappyHotel.Core.Registry;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace HappyHotel.Character.Extension
{
    [CustomEditor(typeof(DefaultCharacter))]
    public class DefaultCharacterExtensionsEditor : OdinEditor
    {
        // 用于存储方向改变行动的临时变量
        private Direction actionDirection = Direction.Right;

        // 用于存储移动目标位置的临时变量
        private Vector2Int moveTargetPosition;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var character = target as DefaultCharacter;
            if (character == null) return;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("扩展方法", EditorStyles.boldLabel);

            // 移动功能
            EditorGUILayout.LabelField("移动控制", EditorStyles.boldLabel);
            moveTargetPosition = EditorGUILayout.Vector2IntField("移动目标位置", moveTargetPosition);
            if (GUILayout.Button("移动到指定位置")) character.MoveTo(moveTargetPosition);

            EditorGUILayout.Space(10);

            // Action队列控制
            EditorGUILayout.LabelField("行动队列控制", EditorStyles.boldLabel);

            // 添加方向改变行动
            EditorGUILayout.LabelField("添加方向改变行动", EditorStyles.miniBoldLabel);
            actionDirection = (Direction)EditorGUILayout.EnumPopup("目标方向", actionDirection);
            if (GUILayout.Button("添加方向改变行动"))
            {
                var typeId = TypeId.Create<ActionTypeId>("ChangeDirection");
                var setting = new ChangeDirectionActionSetting(actionDirection);
                var action = ActionManager.Instance.Create(typeId, setting);
                character.AddAction(action);
            }

            EditorGUILayout.Space(5);

            // 添加攻击行动
            EditorGUILayout.LabelField("添加攻击行动", EditorStyles.miniBoldLabel);
            if (GUILayout.Button("添加攻击行动"))
            {
                var typeId = TypeId.Create<ActionTypeId>("Attack");
                var setting = new AttackActionSetting(1);
                var action = ActionManager.Instance.Create(typeId, setting);
                character.AddAction(action);
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("清空行动队列")) character.ClearActionQueue();
        }
    }
}
#endif