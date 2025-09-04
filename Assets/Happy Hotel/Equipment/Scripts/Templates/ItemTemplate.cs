using System.Collections.Generic;
using System.Linq;
using HappyHotel.Character;
using HappyHotel.Core.Registry;
using HappyHotel.Core.Rarity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HappyHotel.Equipment.Templates
{
    [CreateAssetMenu(fileName = "New Item Template", menuName = "Happy Hotel/Item/Item Template")]
    public class ItemTemplate : ScriptableObject
    {
        [Header("基本信息")] public string itemName;

        [Tooltip("物品图标，用于UI显示和Prop的SpriteRenderer")] [PreviewField]
        public Sprite icon;

        public bool isPurchasable = true;

        [Header("描述")] [TextArea(2, 4)] public string description;

        [Header("稀有度")] public Rarity rarity = Rarity.Common;

        [Header("唯一性设置")] [Tooltip("如果为true，该物品只能被获得一次，获得后商店和奖励盒中不会再出现")]
        public bool isUnique;

        [Header("角色限制设置")]
        [Tooltip("允许使用该物品的角色类型ID（选择'Public'表示所有角色都可用）")]
        [ValueDropdown("GetAvailableCharacterOptions")]
        public string allowedCharacterTypeId = "Public";

        // 获取可用的角色选项列表（用于ValueDropdown）
        private IEnumerable<string> GetAvailableCharacterOptions()
        {
            var options = new List<string>();

            // 添加公共选项
            options.Add("Public");

            // 直接从通用工具读取已注册的角色类型
            var registeredTypes = RegistryTypeIdUtility.GetRegisteredTypeIdsByRegistrationAttribute<CharacterRegistrationAttribute>();

            var availableCharacterTypes = registeredTypes as string[] ?? registeredTypes.ToArray();
            if (!availableCharacterTypes.Any())
            {
                Debug.LogWarning("ItemTemplate: 未找到注册的角色类型");
                return options;
            }

            // 添加所有角色类型
            options.AddRange(availableCharacterTypes);

            return options;
        }

        // 检查指定角色是否可以使用该物品
        public bool IsCharacterAllowed(string characterTypeId)
        {
            // 如果是公共物品，所有角色都可以使用
            if (allowedCharacterTypeId == "Public")
                return true;

            // 否则检查角色类型ID是否匹配
            return allowedCharacterTypeId == characterTypeId;
        }
    }
}