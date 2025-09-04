using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using HappyHotel.Core.Registry;
using HappyHotel.Intent;
using HappyHotel.Intent.Settings;
using UnityEngine;

namespace HappyHotel.Enemy.Templates
{
    [CreateAssetMenu(fileName = "New Enemy Template", menuName = "Happy Hotel/Enemies/Enemy Template")]
    public class EnemyTemplate : SerializedScriptableObject
    {
        [Header("基本信息")] public string enemyName = "敌人";

        [Header("外观")] [PreviewField] public Sprite enemySprite;

        [Header("属性")] public int baseHealth = 3;
        
        [Header("攻击设置")] 
        [SerializeField] [Min(0)] [Tooltip("每回合结束后对主角色造成的伤害值")]
        public int attackPower = 1;
        
        [Title("意图配置")]
        [LabelText("回合结束意图序列")] [Tooltip("敌人每个回合结束时按顺序执行的意图列表")]
        [ListDrawerSettings(ShowFoldout = true, DraggableItems = true, HideAddButton = false, HideRemoveButton = false)]
        [HideReferenceObjectPicker]
        [OdinSerialize]
        public List<EnemyIntentSequenceItem> intentSequence = new();

        [InlineProperty]
        public class EnemyIntentSequenceItem
        {
            [LabelText("意图类型")]
            [ValueDropdown(nameof(GetAvailableIntentTypeIds))]
            [OnValueChanged(nameof(OnTypeChanged))]
            public string typeId;

            [ShowIf(nameof(HasSetting))]
            [InlineProperty, HideLabel]
            [OdinSerialize]
            [HideReferenceObjectPicker]
            public IIntentSetting setting;

            private bool HasSetting()
            {
                return IntentSettingTypeLookup.GetSettingTypeFor(typeId) != null;
            }

            private void OnTypeChanged()
            {
                var t = IntentSettingTypeLookup.GetSettingTypeFor(typeId);
                setting = t != null ? (IIntentSetting)System.Activator.CreateInstance(t) : null;
            }

            [OnInspectorInit]
            private void EnsureSettingInstance()
            {
                var t = IntentSettingTypeLookup.GetSettingTypeFor(typeId);
                if (t != null && setting == null)
                {
                    setting = (IIntentSetting)System.Activator.CreateInstance(t);
                }
            }

            private static IEnumerable<string> GetAvailableIntentTypeIds()
            {
                return RegistryTypeIdUtility.GetRegisteredTypeIdsByRegistry<IntentRegistry>();
            }
        }
    }
}