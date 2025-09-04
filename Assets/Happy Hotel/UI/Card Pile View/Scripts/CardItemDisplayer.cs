using HappyHotel.Card;
using HappyHotel.Core.Description;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI.CardPileUI
{
    // 挂载在卡牌UI预制体上，负责显示单张卡牌的信息
    public class CardItemDisplayer : MonoBehaviour
    {
        [Header("UI元素引用")] [SerializeField] private TMP_Text cardNameText;

        [SerializeField] private TMP_Text cardDescriptionText;
        [SerializeField] private TMP_Text energyCostText; // 可选，如果您的卡牌有能量消耗
        [SerializeField] private Image cardArtwork; // 可选，用于显示卡牌图画

        // 接收卡牌数据并更新UI显示
        public void DisplayCard(CardBase card)
        {
            if (card == null)
            {
                Debug.LogWarning("尝试显示一个空的卡牌数据");
                gameObject.SetActive(false);
                return;
            }

            // 从卡牌模板获取信息
            if (card.Template == null)
            {
                Debug.LogError($"无法获取卡牌 {card.TypeId.Id} 的模板!");
                return;
            }

            if (cardNameText != null) cardNameText.text = card.Template.itemName;

            if (cardDescriptionText != null)
                // 使用格式化描述而不是原始模板描述
                cardDescriptionText.text =
                    DescriptionFormatter.GetFormattedDescriptionOrDefault(card, card.Template.description);

            if (energyCostText != null)
            {
                // 显示卡牌消耗
                energyCostText.text = card.Template.cardCost.ToString();
                energyCostText.gameObject.SetActive(true);
            }

            if (cardArtwork != null)
            {
                // 显示卡牌图画
                cardArtwork.sprite = card.Template.cardImage;
                cardArtwork.gameObject.SetActive(cardArtwork.sprite != null);
            }
        }

        // 设置临时状态（新增）
        public void SetTemporaryState(bool isTemporary)
        {
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = isTemporary ? 0.5f : 1.0f;
        }
    }
}