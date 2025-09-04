using HappyHotel.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI.EquipmentInventoryUI
{
    // 主界面分区按钮与面板管理器
    public class EquipmentInventoryAreaPanelManager : MonoBehaviour
    {
        [Header("分区按钮")] [SerializeField] private Button allButton;

        [SerializeField] private Button unrefreshedButton;
        [SerializeField] private Button refreshedButton;
        [SerializeField] private Button destroyedButton;

        [Header("分区面板")] [SerializeField] private EquipmentInventoryAreaPanelController areaPanel;

        [Header("分区数量文本")] [SerializeField] private TMP_Text unrefreshedCountText;

        [SerializeField] private TMP_Text refreshedCountText;
        [SerializeField] private TMP_Text destroyedCountText;

        private InventoryArea? currentArea;
        private EquipmentInventory inventory;

        private void Awake()
        {
            inventory = EquipmentInventory.Instance;
            if (allButton != null)
                allButton.onClick.AddListener(() => OnAreaButtonClicked(InventoryArea.All));
            if (unrefreshedButton != null)
                unrefreshedButton.onClick.AddListener(() => OnAreaButtonClicked(InventoryArea.Unrefreshed));
            if (refreshedButton != null)
                refreshedButton.onClick.AddListener(() => OnAreaButtonClicked(InventoryArea.Refreshed));
            if (destroyedButton != null)
                destroyedButton.onClick.AddListener(() => OnAreaButtonClicked(InventoryArea.Destroyed));
            if (areaPanel != null)
                areaPanel.HidePanel();
            RefreshAreaCounts();
        }

        private void Update()
        {
            RefreshAreaCounts();
        }

        private void OnAreaButtonClicked(InventoryArea area)
        {
            if (!areaPanel) return;
            if (areaPanel.IsVisible && currentArea == area)
            {
                areaPanel.HidePanel();
                currentArea = null;
            }
            else
            {
                areaPanel.ShowArea(area);
                currentArea = area;
            }
        }

        private void RefreshAreaCounts()
        {
            if (inventory == null) inventory = EquipmentInventory.Instance;
            if (unrefreshedCountText != null)
                unrefreshedCountText.text = inventory != null ? inventory.GetUnrefreshedTotalCount().ToString() : "0";
            if (refreshedCountText != null)
                refreshedCountText.text = inventory != null ? inventory.GetRefreshedTotalCount().ToString() : "0";
            if (destroyedCountText != null)
                destroyedCountText.text = inventory != null ? inventory.GetDestroyedTotalCount().ToString() : "0";
        }
    }
}