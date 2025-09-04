using System.Collections.Generic;
using HappyHotel.Equipment;
using HappyHotel.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI.EquipmentInventoryUI
{
    // 单分区装备面板控制器
    public class EquipmentInventoryAreaPanelController : MonoBehaviour
    {
        [Header("UI组件")] [SerializeField] private Transform itemListContainer;

        [SerializeField] private EquipmentInventoryItemDisplayController itemPrefab;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton; // 关闭按钮

        private readonly List<EquipmentInventoryItemDisplayController> itemDisplays = new();
        private InventoryArea currentArea;
        private EquipmentInventory inventory;

        public bool IsVisible => panelRoot.activeSelf;

        private void Awake()
        {
            SetupCloseButton();
        }

        private void SetupCloseButton()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(HidePanel);
            else
                Debug.LogWarning("EquipmentInventoryAreaPanelController: 未找到关闭按钮引用，请检查Inspector中的设置");
        }

        public void ShowArea(InventoryArea area)
        {
            if (!inventory) inventory = EquipmentInventory.Instance;
            currentArea = area;
            panelRoot.SetActive(true);
            RefreshArea();
        }

        public void HidePanel()
        {
            panelRoot.SetActive(false);
            ClearItems();
        }

        private void RefreshArea()
        {
            ClearItems();
            var list = new List<EquipmentBase>();
            switch (currentArea)
            {
                case InventoryArea.All:
                    // 合并未刷新、已刷新、销毁区
                    list.AddRange(inventory.GetUnrefreshedEquipmentInstances());
                    list.AddRange(inventory.GetRefreshedEquipmentInstances());
                    list.AddRange(inventory.GetDestroyedEquipmentInstances());
                    break;
                case InventoryArea.Unrefreshed:
                    list = inventory.GetUnrefreshedEquipmentInstances();
                    break;
                case InventoryArea.Refreshed:
                    list = inventory.GetRefreshedEquipmentInstances();
                    break;
                case InventoryArea.Destroyed:
                    list = inventory.GetDestroyedEquipmentInstances();
                    break;
            }

            foreach (var equip in list)
            {
                var display = Instantiate(itemPrefab, itemListContainer);
                display.SetEquipment(equip);
                itemDisplays.Add(display);
            }
        }

        private void ClearItems()
        {
            foreach (var d in itemDisplays)
                if (d != null)
                    Destroy(d.gameObject);

            itemDisplays.Clear();
        }
    }

    public enum InventoryArea
    {
        Unrefreshed,
        Refreshed,
        Destroyed,
        All
    }
}