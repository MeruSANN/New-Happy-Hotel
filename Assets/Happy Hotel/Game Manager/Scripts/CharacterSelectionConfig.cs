using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using HappyHotel.Core.Registry;
using HappyHotel.Equipment;
using HappyHotel.Card;

namespace HappyHotel.GameManager
{
    [CreateAssetMenu(fileName = "CharacterSelectionConfig",
        menuName = "Happy Hotel/Game Manager/Character Selection Config")]
    public class CharacterSelectionConfig : SerializedScriptableObject
    {
        [Header("角色信息")] [SerializeField] [Tooltip("角色显示名称")]
        private string characterName = "默认角色";

        [SerializeField] [Tooltip("角色描述")] private string characterDescription = "这是一个默认角色";

        [SerializeField] [Tooltip("角色类型ID（字符串形式）")]
        private string characterTypeId = "Default";

        [Header("初始装备配置")] [SerializeField] [Tooltip("新游戏时玩家初始拥有的装备列表，每个字符串代表一个装备的TypeId")]
        [ValueDropdown(nameof(GetAvailableEquipmentTypeIds))]
        private List<string> initialEquipments = new List<string>();

        [Header("初始卡牌配置")] [SerializeField] [Tooltip("新游戏时玩家初始拥有的卡牌列表，每个字符串代表一个卡牌的TypeId")]
        [ValueDropdown(nameof(GetAvailableCardTypeIds))]
        private List<string> initialCards = new List<string>();

        [Header("初始金币配置")] [SerializeField] [Tooltip("新游戏时玩家的初始金币数量")]
        private int initialMoney = 1000;

        [Header("初始敏捷配置")] [SerializeField] [Tooltip("新游戏时玩家每回合可免费放置次数（敏捷上限）")]
        private int initialAgilityMax = 1;

        [Header("角色图标")] [SerializeField] [Tooltip("角色选择界面显示的图标")]
        private Sprite characterIcon;

        // 获取角色显示名称
        public string CharacterName => characterName;

        // 获取角色描述
        public string CharacterDescription => characterDescription;

        // 获取角色类型ID
        public string CharacterTypeId => characterTypeId;

        // 获取初始装备列表的只读访问
        public string[] InitialEquipments => initialEquipments?.ToArray() ?? new string[0];

        // 获取初始卡牌列表的只读访问
        public string[] InitialCards => initialCards?.ToArray() ?? new string[0];

        // 获取初始金币数量
        public int InitialMoney => initialMoney;

        // 获取初始敏捷上限
        public int InitialAgilityMax => Mathf.Max(0, initialAgilityMax);

        // 获取角色图标
        public Sprite CharacterIcon => characterIcon;

        // 检查配置是否有效
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(characterName))
                return false;

            if (string.IsNullOrEmpty(characterTypeId))
                return false;

            if (initialEquipments == null)
                return false;

            if (initialCards == null)
                return false;

            // 检查初始金币是否为负数
            if (initialMoney < 0)
                return false;

            // 检查是否有空字符串或null
            foreach (var equipment in initialEquipments)
                if (string.IsNullOrEmpty(equipment))
                    return false;

            foreach (var card in initialCards)
                if (string.IsNullOrEmpty(card))
                    return false;
            
            return true;
        }

        private IEnumerable<string> GetAvailableEquipmentTypeIds()
        {
            return RegistryTypeIdUtility.GetRegisteredTypeIdsByRegistrationAttribute<EquipmentRegistrationAttribute>();
        }

        private IEnumerable<string> GetAvailableCardTypeIds()
        {
            return RegistryTypeIdUtility.GetRegisteredTypeIdsByRegistrationAttribute<CardRegistrationAttribute>();
        }

        // 获取初始装备数量
        public int GetEquipmentCount()
        {
            return initialEquipments?.Count ?? 0;
        }

        // 获取初始卡牌数量
        public int GetCardCount()
        {
            return initialCards?.Count ?? 0;
        }
    }
}