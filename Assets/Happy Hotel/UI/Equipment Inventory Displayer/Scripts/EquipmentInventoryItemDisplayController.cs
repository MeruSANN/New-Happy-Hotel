using HappyHotel.Equipment;
using HappyHotel.Equipment.Templates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI.EquipmentInventoryUI
{
    // 单个背包装备的显示控制脚本
    public class EquipmentInventoryItemDisplayController : MonoBehaviour
    {
        [Header("UI组件引用")] [SerializeField] private Image itemIconImage; // 装备图标

        [SerializeField] private TextMeshProUGUI nameText; // 道具名称文本组件
        [SerializeField] private TextMeshProUGUI descriptionText; // 描述文本组件

        private EquipmentBase currentEquipment;

        public void SetEquipment(EquipmentBase equipment)
        {
            currentEquipment = equipment;
            if (currentEquipment != null)
            {
                UpdateDisplay();
                SetUIElementsActive(true);
            }
            else
            {
                SetUIElementsActive(false);
            }
        }

        private void UpdateDisplay()
        {
            if (currentEquipment == null)
                return;
            if (itemIconImage != null)
            {
                var template = currentEquipment.Template;
                if (template != null && template.icon != null)
                {
                    itemIconImage.sprite = template.icon;
                    itemIconImage.color = Color.white;
                }
                else
                {
                    itemIconImage.sprite = null;
                    itemIconImage.color = Color.gray;
                }
            }

            // 更新道具名称显示
            if (nameText != null)
            {
                var template = currentEquipment.Template;
                if (template != null)
                    nameText.text = template.itemName;
                else
                    nameText.text = "未知装备";
            }

            var desc = currentEquipment.GetFormattedDescription();
            descriptionText.text = string.IsNullOrEmpty(desc) ? "暂无描述" : desc;
        }

        private ItemTemplate GetEquipmentTemplate()
        {
            // 此方法不再需要，因为我们直接通过 currentEquipment.Template 访问
            return currentEquipment?.Template;
        }

        public EquipmentBase GetCurrentEquipment()
        {
            return currentEquipment;
        }

        private void SetUIElementsActive(bool active)
        {
            if (itemIconImage != null)
                itemIconImage.gameObject.SetActive(active);
        }

        public bool IsEmpty()
        {
            return currentEquipment == null;
        }

        public void RefreshDisplay()
        {
            if (currentEquipment != null) UpdateDisplay();
        }
    }
}