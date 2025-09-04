using HappyHotel.UI.EquipmentInventoryUI;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI.Shop
{
    // 商店装备背包按钮控制器
    public class ShopInventoryButtonController : MonoBehaviour
    {
        [Header("UI组件")] [SerializeField] private Button inventoryButton; // 装备背包按钮

        [SerializeField] private EquipmentInventoryAreaPanelController inventoryPanel; // 装备背包面板控制器

        private void Awake()
        {
            SetupInventoryButton();
        }

        private void OnDestroy()
        {
            if (inventoryButton != null) inventoryButton.onClick.RemoveListener(OnInventoryButtonClicked);
        }

        private void SetupInventoryButton()
        {
            if (inventoryButton != null)
                inventoryButton.onClick.AddListener(OnInventoryButtonClicked);
            else
                Debug.LogWarning("ShopInventoryButtonController: 未找到装备背包按钮引用，请检查Inspector中的设置");
        }

        private void OnInventoryButtonClicked()
        {
            if (inventoryPanel != null)
                // 显示装备库的全部内容
                inventoryPanel.ShowArea(InventoryArea.All);
            else
                Debug.LogWarning("ShopInventoryButtonController: 未找到装备背包面板控制器引用，请检查Inspector中的设置");
        }
    }
}