using HappyHotel.UI.CardPileUI;
using UnityEngine;
using UnityEngine.UI;
using static HappyHotel.Inventory.CardInventory;

namespace HappyHotel.UI.Shop
{
    // 商店牌组按钮控制器
    public class ShopCardDeckButtonController : MonoBehaviour
    {
        [Header("UI组件")] [SerializeField] private Button cardDeckButton; // 牌组按钮

        [SerializeField] private CardListPanelController cardDeckPanel; // 牌组面板控制器

        private void Awake()
        {
            SetupCardDeckButton();
        }

        private void OnDestroy()
        {
            if (cardDeckButton != null) cardDeckButton.onClick.RemoveListener(OnCardDeckButtonClicked);
        }

        private void SetupCardDeckButton()
        {
            if (cardDeckButton != null)
                cardDeckButton.onClick.AddListener(OnCardDeckButtonClicked);
            else
                Debug.LogWarning("ShopCardDeckButtonController: 未找到牌组按钮引用，请检查Inspector中的设置");
        }

        private void OnCardDeckButtonClicked()
        {
            if (cardDeckPanel != null)
                // 显示卡牌全部内容（排除临时区）
                cardDeckPanel.ShowPanel(CardZone.All);
            else
                Debug.LogWarning("ShopCardDeckButtonController: 未找到牌组面板控制器引用，请检查Inspector中的设置");
        }
    }
}