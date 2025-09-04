using System;
using System.Linq;
using HappyHotel.Card;
using HappyHotel.Core.Singleton;
using HappyHotel.GameManager;
using UnityEngine;

namespace HappyHotel.Inventory
{
    // 卡牌抽取管理器，管理卡牌的抽取、使用和区域转换机制
    [ManagedSingleton(SceneLoadMode.Exclude, "ShopScene", "MainMenu", "MapEditScene")]
    [SingletonInitializationDependency(typeof(TurnManager))]
    public class CardDrawManager : SingletonBase<CardDrawManager>
    {
        // 配置参数
        private int cardsPerTurnDraw = 5; // 默认值，会被GameConfig覆盖

        private GameConfig gameConfig;
        public Action<CardBase> onCardConsumed; // 卡牌被消耗时触发

        public Action<CardBase> onCardDiscarded; // 卡牌被弃置时触发

        // 事件定义
        public Action<CardBase> onCardDrawn; // 卡牌被抽取时触发
        public Action<CardBase> onCardUsed; // 卡牌被使用时触发
        public System.Action onDeckShuffled; // 牌库洗牌时触发
        public System.Action onTurnEndDiscard; // 回合结束弃牌时触发
        public System.Action onTurnStartDraw; // 回合开始抽牌时触发

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // 取消订阅
            TurnManager.onPlayerTurnStart -= OnTurnStart;
            TurnManager.onEnemyTurnEnd -= OnTurnEnd;
        }

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
            LoadGameConfig();

            // 订阅回合开始/结束事件
            TurnManager.onPlayerTurnStart += OnTurnStart;
            TurnManager.onEnemyTurnEnd += OnTurnEnd;
        }

        private void LoadGameConfig()
        {
            var provider = ConfigProvider.Instance;
            gameConfig = provider ? provider.GetGameConfig() : null;
            if (gameConfig != null)
                cardsPerTurnDraw = gameConfig.CardsToDrawPerTurn;
            else
                Debug.LogWarning("CardDrawManager: 无法通过ConfigProvider获取GameConfig，将使用默认值。");
        }

        // 回合开始时自动抽卡
        private void OnTurnStart(int turnNumber)
        {
            if (gameConfig != null)
                DrawCards(gameConfig.CardsToDrawPerTurn);
            else
                Debug.LogWarning("GameConfig未加载，无法自动抽卡");
        }

        // 回合结束时自动弃牌
        private void OnTurnEnd(int turnNumber)
        {
            if (CardInventory.Instance != null) CardInventory.Instance.DiscardHand();
        }

        // 回合开始时抽牌
        public void DrawCardsForTurnStart()
        {
            var cardsDrawn = DrawCards(cardsPerTurnDraw);
            Debug.Log($"回合开始抽牌，抽取了 {cardsDrawn} 张卡牌");
            onTurnStartDraw?.Invoke();
        }

        // 抽取指定数量的卡牌到手牌区
        public int DrawCards(int count)
        {
            if (CardInventory.Instance == null)
            {
                Debug.LogWarning("CardInventory不存在，无法抽牌");
                return 0;
            }

            var actualDrawn = 0;

            for (var i = 0; i < count; i++)
            {
                var drawnCard = DrawSingleCard();
                if (drawnCard != null)
                {
                    actualDrawn++;
                }
                else
                {
                    Debug.Log($"无法抽取更多卡牌，已抽取 {actualDrawn} 张");
                    break;
                }
            }

            return actualDrawn;
        }

        // 抽取单张卡牌
        public CardBase DrawSingleCard()
        {
            if (CardInventory.Instance == null) return null;

            var inventory = CardInventory.Instance;

            // 检查牌库区是否为空
            if (inventory.DeckCardCount == 0)
            {
                // 自动洗牌
                inventory.ShuffleDiscardIntoDeck();
                if (inventory.DeckCardCount == 0)
                {
                    Debug.Log("弃牌区也为空，无法抽牌");
                    return null;
                }
            }

            // 从牌库区顶部抽取卡牌
            var deckCards = inventory.GetCardsInZone(CardInventory.CardZone.Deck);
            if (deckCards.Count > 0)
            {
                var cardToDraw = deckCards[0]; // 取第一张（顶部）

                // 移动到手牌区
                if (inventory.MoveCardToZone(cardToDraw, CardInventory.CardZone.Hand))
                {
                    Debug.Log($"抽取卡牌: {cardToDraw.TypeId.Id}");
                    onCardDrawn?.Invoke(cardToDraw);
                    return cardToDraw;
                }
            }

            return null;
        }

        // 使用卡牌（调用CardBase的UseCard方法）
        public void UseCard(CardBase card)
        {
            if (card == null) return;

            // 调用卡牌自身的UseCard方法，它会处理事件发送和消耗逻辑
            card.UseCard();
        }

        // 处理卡牌消耗逻辑（由CardBase调用）
        public void HandleCardConsumption(CardBase card)
        {
            if (card == null || CardInventory.Instance == null)
            {
                Debug.LogWarning("[CardDrawManager] HandleCardConsumption: 卡牌或CardInventory为空");
                return;
            }

            var inventory = CardInventory.Instance;

            // 检查卡牌是否在手牌区
            if (!inventory.HasCardOfType(card.TypeId, CardInventory.CardZone.Hand))
            {
                Debug.LogWarning($"[CardDrawManager] HandleCardConsumption: 卡牌 {card.TypeId.Id} 不在手牌区，无法处理消耗");
                return;
            }

            // 检查卡牌是否已经在临时区（ActivePlacementCard的特殊处理）
            if (inventory.HasCardOfType(card.TypeId, CardInventory.CardZone.Temporary))
            {
                Debug.Log($"[CardDrawManager] HandleCardConsumption: 卡牌 {card.TypeId.Id} 已在临时区，跳过消耗处理");
                return;
            }

            // 根据消耗型标记决定去向
            var targetZone = card.IsConsumable ? CardInventory.CardZone.Consumed : CardInventory.CardZone.Discard;

            if (inventory.MoveCardToZone(card, targetZone))
            {
                Debug.Log($"[CardDrawManager] HandleCardConsumption: 处理卡牌消耗 {card.TypeId.Id}，移动到 {targetZone} 区域");
                onCardUsed?.Invoke(card);

                if (card.IsConsumable)
                    onCardConsumed?.Invoke(card);
                else
                    onCardDiscarded?.Invoke(card);
            }
            else
            {
                Debug.LogError($"[CardDrawManager] HandleCardConsumption: 移动卡牌 {card.TypeId.Id} 失败");
            }
        }

        // 使用指定类型的卡牌
        public void UseCard(CardTypeId cardTypeId)
        {
            if (cardTypeId == null || CardInventory.Instance == null) return;

            var card = CardInventory.Instance.GetCardByTypeId(cardTypeId);
            if (card != null)
                UseCard(card);
            else
                Debug.LogWarning($"手牌区未找到卡牌类型 {cardTypeId.Id}");
        }

        // 弃置单张手牌
        public void DiscardCard(CardBase card)
        {
            if (card == null || CardInventory.Instance == null) return;

            var inventory = CardInventory.Instance;

            if (inventory.MoveCardToZone(card, CardInventory.CardZone.Discard))
            {
                Debug.Log($"弃置卡牌: {card.TypeId.Id}");
                onCardDiscarded?.Invoke(card);
            }
        }

        // 弃置指定类型的手牌
        public void DiscardCard(CardTypeId cardTypeId)
        {
            if (cardTypeId == null || CardInventory.Instance == null) return;

            var card = CardInventory.Instance.GetCardByTypeId(cardTypeId);
            if (card != null)
                DiscardCard(card);
            else
                Debug.LogWarning($"手牌区未找到卡牌类型 {cardTypeId.Id}");
        }

        // 弃置所有手牌
        public void DiscardAllHandCards()
        {
            if (CardInventory.Instance == null) return;

            var inventory = CardInventory.Instance;
            var handCards = inventory.GetCardsInZone(CardInventory.CardZone.Hand).ToList();

            var discardedCount = 0;
            foreach (var card in handCards)
                if (inventory.MoveCardToZone(card, CardInventory.CardZone.Discard))
                {
                    onCardDiscarded?.Invoke(card);
                    discardedCount++;
                }

            Debug.Log($"弃置了 {discardedCount} 张手牌");
            if (discardedCount > 0) onTurnEndDiscard?.Invoke();
        }

        // 手动洗牌（将弃牌区洗入牌库区）
        public void ShuffleDiscardIntoDeck()
        {
            if (CardInventory.Instance == null) return;

            CardInventory.Instance.ShuffleDiscardIntoDeck();
            onDeckShuffled?.Invoke();
            Debug.Log("手动执行了洗牌操作");
        }

        // 打乱牌库区顺序
        public void ShuffleDeck()
        {
            if (CardInventory.Instance == null) return;

            CardInventory.Instance.ShuffleDeck();
            onDeckShuffled?.Invoke();
            Debug.Log("打乱了牌库区顺序");
        }

        // 重置为新关卡状态
        public void ResetForNewLevel()
        {
            if (CardInventory.Instance == null) return;

            CardInventory.Instance.ResetForNewLevel();
            // 记录新关开始时的牌组基线
            CardInventory.Instance.CaptureLevelBaseline();
            Debug.Log("卡牌系统已重置为新关卡状态");
        }

        // 获取各区域统计信息
        public string GetZoneStatistics()
        {
            if (CardInventory.Instance == null) return "CardInventory不存在";

            var inventory = CardInventory.Instance;
            return $"牌库区: {inventory.DeckCardCount}, " +
                   $"弃牌区: {inventory.DiscardCardCount}, " +
                   $"手牌区: {inventory.HandCardCount}, " +
                   $"消耗区: {inventory.ConsumedCardCount}";
        }

        // 配置参数设置方法
        public void SetCardsPerTurnDraw(int count)
        {
            cardsPerTurnDraw = Mathf.Max(0, count);
            Debug.Log($"每回合抽牌数量设置为: {cardsPerTurnDraw}");
        }

        // 获取配置参数
        public int GetCardsPerTurnDraw()
        {
            return cardsPerTurnDraw;
        }
    }
}