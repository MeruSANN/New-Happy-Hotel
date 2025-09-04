using System.Collections.Generic;
using UnityEngine;

namespace HappyHotel.Equipment.Templates
{
    // 装备类物品的配置模板
    [CreateAssetMenu(fileName = "New Equipment Template", menuName = "Happy Hotel/Item/Equipment Template")]
    public class EquipmentTemplate : ItemTemplate
    {
        [Header("装备设置")] [Tooltip("如果为true，该装备在对应Prop被触发过一次后，不会再从背包中刷新出来，直到读取新的关卡为止")]
        public bool isConsumable;

        [Tooltip("如果为true，该装备在触发时会销毁自身，但下次仍然会被刷新")]
        public bool isSingleUse;

        [Header("升级系统")] [Tooltip("该装备是否可以升级")]
        public bool isUpgradeable;

        [Tooltip("升级后的装备类型ID（如铁剑升级为铁剑+）")] public string upgradedEquipmentTypeId = "";

        [Tooltip("该装备是否为升级后的装备（升级后的装备不可再升级）")] public bool isUpgradedEquipment;

        [Tooltip("如果该装备是升级版本，这里填写原始装备的类型ID")] public string baseEquipmentTypeId = "";

        [Header("标签系统")] [Tooltip("装备的标签列表，用于分类和筛选")]
        public List<string> tags = new();
    }
}