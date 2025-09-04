using System.Collections.Generic;
using System.Linq;
using HappyHotel.Card;
using HappyHotel.Card.Setting;
using HappyHotel.Core.Singleton;
using UnityEngine;

namespace HappyHotel.Inventory
{
    // 卡牌背包管理器，管理五个区域：牌库区、弃牌区、手牌区、消耗区、临时区
    [ManagedSingleton(true)]
    public class CardInventory : SingletonBase<CardInventory>
    {
        // 事件定义
        public delegate void CardInventoryEvent(CardTypeId cardTypeId);

        public delegate void CardInventoryFullEvent(CardTypeId cardTypeId);

        public delegate void CardZoneEvent(CardTypeId cardTypeId, CardZone fromZone, CardZone toZone);

        // 卡牌区域枚举
        public enum CardZone
        {
            Deck, // 牌库区
            Discard, // 弃牌区
            Hand, // 手牌区
            Consumed, // 消耗区
            Temporary, // 临时区（新增）
            All // 全部（新增，不参与原序列化索引）
        }

        private readonly List<CardBase> consumedCards = new(); // 消耗区（无顺序）

        // 五个区域的卡牌存储
        private readonly List<CardBase> deckCards = new(); // 牌库区（有顺序）
        private readonly List<CardBase> discardCards = new(); // 弃牌区（无顺序）
        private readonly List<CardBase> handCards = new(); // 手牌区（无顺序）
        private readonly List<CardBase> temporaryCards = new(); // 临时区（无顺序）

        // 获取各区域卡牌数量
        public int DeckCardCount => deckCards.Count;
        public int DiscardCardCount => discardCards.Count;
        public int HandCardCount => handCards.Count;
        public int ConsumedCardCount => consumedCards.Count;
        public int TemporaryCardCount => temporaryCards.Count;

        public int CurrentCardCount => deckCards.Count + discardCards.Count + handCards.Count + consumedCards.Count +
                                       temporaryCards.Count;

        public event CardInventoryEvent onCardAdded;
        public event CardInventoryEvent onCardRemoved;
        public event CardZoneEvent onCardMovedBetweenZones;

        // 静默标志：为批量恢复提供事件抑制
        private bool suppressEvents;

        // 设置是否抑制事件（静默模式）
        public void SetEventSuppressed(bool suppressed)
        {
            suppressEvents = suppressed;
        }

        // 本关基线计数（按类型）
        private readonly Dictionary<CardTypeId, int> levelBaselineCounts = new();

        // 本关额外持久化配额（按类型）
        private readonly Dictionary<CardTypeId, int> extraPersistCounts = new();

        // 添加卡牌到牌库区（保持原有接口）
        public bool AddCard(CardTypeId cardTypeId, ICardSetting setting = null)
        {
            if (cardTypeId == null)
            {
                Debug.LogWarning("尝试添加无效的卡牌类型ID");
                return false;
            }

            var card = CardManager.Instance.Create(cardTypeId, setting);
            if (card == null)
            {
                Debug.LogError($"无法创建类型为 {cardTypeId.Id} 的卡牌");
                return false;
            }

            // 默认添加到牌库区
            deckCards.Add(card);
            if (!suppressEvents) onCardAdded?.Invoke(cardTypeId);

            return true;
        }

        // 从所有区域中移除卡牌（保持原有接口）
        public bool RemoveCard(CardTypeId cardTypeId)
        {
            if (cardTypeId == null)
            {
                Debug.LogWarning("尝试移除无效的卡牌类型ID");
                return false;
            }

            // 依次从各个区域查找并移除
            var card = deckCards.FirstOrDefault(c => c.TypeId.Equals(cardTypeId));
            if (card != null)
            {
                deckCards.Remove(card);
                if (!suppressEvents) onCardRemoved?.Invoke(cardTypeId);
                return true;
            }

            card = discardCards.FirstOrDefault(c => c.TypeId.Equals(cardTypeId));
            if (card != null)
            {
                discardCards.Remove(card);
                if (!suppressEvents) onCardRemoved?.Invoke(cardTypeId);
                return true;
            }

            card = handCards.FirstOrDefault(c => c.TypeId.Equals(cardTypeId));
            if (card != null)
            {
                handCards.Remove(card);
                if (!suppressEvents) onCardRemoved?.Invoke(cardTypeId);
                return true;
            }

            card = consumedCards.FirstOrDefault(c => c.TypeId.Equals(cardTypeId));
            if (card != null)
            {
                consumedCards.Remove(card);
                if (!suppressEvents) onCardRemoved?.Invoke(cardTypeId);
                return true;
            }

            card = temporaryCards.FirstOrDefault(c => c.TypeId.Equals(cardTypeId));
            if (card != null)
            {
                temporaryCards.Remove(card);
                if (!suppressEvents) onCardRemoved?.Invoke(cardTypeId);
                return true;
            }

            return false;
        }

        // 记录本关基线（统计实体所在区域：牌库/弃牌/手牌/消耗，不包含临时区）
        public void CaptureLevelBaseline()
        {
            levelBaselineCounts.Clear();
            extraPersistCounts.Clear();

            var counts = CountPhysicalCardTypes();
            foreach (var kv in counts) levelBaselineCounts[kv.Key] = kv.Value;

            Debug.Log($"[CardInventory] 已记录本关基线，类型数: {levelBaselineCounts.Count}");
        }

        // 增加本关跨关保留的新增卡配额（按类型）
        public void AllowExtraPersist(CardTypeId cardTypeId, int count = 1)
        {
            if (cardTypeId == null || count <= 0) return;
            if (extraPersistCounts.TryGetValue(cardTypeId, out var cur))
                extraPersistCounts[cardTypeId] = cur + count;
            else
                extraPersistCounts[cardTypeId] = count;
        }

        // 在关末对齐到“基线 + 配额”，移除超出的卡（不影响奖励后续添加）
        public void CleanupToBaseline()
        {
            // 先清理临时区引用，避免重复删除同一实例
            if (temporaryCards.Count > 0)
            {
                var tempCopy = new List<CardBase>(temporaryCards);
                foreach (var card in tempCopy) RemoveFromTemporaryZone(card);
            }

            // 当前实体计数
            var current = CountPhysicalCardTypes();

            // 计算允许的目标计数 = 基线 + 配额（若未记录基线，则视为0）
            foreach (var kv in current)
            {
                var typeId = kv.Key;
                var curCount = kv.Value;
                var baseline = levelBaselineCounts.TryGetValue(typeId, out var b) ? b : 0;
                var extra = extraPersistCounts.TryGetValue(typeId, out var e) ? e : 0;
                var allowed = baseline + extra;

                var toRemove = curCount - allowed;
                if (toRemove <= 0) continue;

                for (var i = 0; i < toRemove; i++)
                {
                    // 使用现有接口逐个移除
                    var removed = RemoveCard(typeId);
                    if (!removed)
                    {
                        Debug.LogWarning($"[CardInventory] CleanupToBaseline: 试图移除多余卡失败: {typeId.Id}");
                        break;
                    }
                }
            }

            Debug.Log("[CardInventory] 已按基线清理本关临时新增卡");
        }

        // 统计实体所在区域（不包含临时区引用）
        private Dictionary<CardTypeId, int> CountPhysicalCardTypes()
        {
            var dict = new Dictionary<CardTypeId, int>();
            void AddMany(IEnumerable<CardBase> list)
            {
                foreach (var c in list)
                {
                    if (c == null || c.TypeId == null) continue;
                    if (dict.TryGetValue(c.TypeId, out var n)) dict[c.TypeId] = n + 1; else dict[c.TypeId] = 1;
                }
            }

            AddMany(deckCards);
            AddMany(discardCards);
            AddMany(handCards);
            AddMany(consumedCards);
            // 不计入temporaryCards

            return dict;
        }

        // 获取指定类型和区域的卡牌（保持原有接口）
        public CardBase GetCardByTypeId(CardTypeId typeId, CardZone zone = CardZone.Hand)
        {
            return GetCardsInZone(zone).FirstOrDefault(c => c.TypeId.Equals(typeId));
        }

        // 获取指定类型和区域的所有卡牌（保持原有接口）
        public IEnumerable<CardBase> GetCardsByTypeId(CardTypeId typeId, CardZone zone = CardZone.Hand)
        {
            return GetCardsInZone(zone).Where(c => c.TypeId.Equals(typeId));
        }

        // 检查指定区域是否有某类型卡牌
        public bool HasCardOfType(CardTypeId typeId, CardZone zone = CardZone.Hand)
        {
            return GetCardsInZone(zone).Any(c => c.TypeId.Equals(typeId));
        }

        // 检查所有区域是否有某类型卡牌（保持原有接口）
        public bool HasCardOfType(CardTypeId typeId)
        {
            return deckCards.Any(c => c.TypeId.Equals(typeId)) ||
                   discardCards.Any(c => c.TypeId.Equals(typeId)) ||
                   handCards.Any(c => c.TypeId.Equals(typeId)) ||
                   consumedCards.Any(c => c.TypeId.Equals(typeId)) ||
                   temporaryCards.Any(c => c.TypeId.Equals(typeId));
        }

        // 获取指定区域的所有卡牌
        public IReadOnlyList<CardBase> GetCardsInZone(CardZone zone)
        {
            switch (zone)
            {
                case CardZone.Deck: return deckCards.AsReadOnly();
                case CardZone.Discard: return discardCards.AsReadOnly();
                case CardZone.Hand: return handCards.AsReadOnly();
                case CardZone.Consumed: return consumedCards.AsReadOnly();
                case CardZone.Temporary: return temporaryCards.AsReadOnly();
                case CardZone.All:
                    var merged = new List<CardBase>();
                    merged.AddRange(deckCards);
                    merged.AddRange(discardCards);
                    merged.AddRange(handCards);
                    merged.AddRange(consumedCards);
                    return merged.AsReadOnly();
                default: return new List<CardBase>().AsReadOnly();
            }
        }

        // 获取手牌区卡牌（便捷方法）
        public IReadOnlyList<CardBase> GetHandCards()
        {
            return handCards.AsReadOnly();
        }

        // 获取临时区卡牌（新增便捷方法）
        public IReadOnlyList<CardBase> GetTemporaryCards()
        {
            return temporaryCards.AsReadOnly();
        }

        // 移动卡牌到指定区域
        public bool MoveCardToZone(CardBase card, CardZone targetZone)
        {
            if (card == null)
            {
                Debug.LogError("[CardInventory] MoveCardToZone: 卡牌为空");
                return false;
            }

            var sourceZone = GetCardZone(card);

            if (sourceZone == targetZone) return true; // 已经在目标区域

            // 特殊处理：如果目标区域是临时区，只添加引用而不从源区域移除
            if (targetZone == CardZone.Temporary)
            {
                AddCardToZone(card, targetZone);
                if (!suppressEvents) onCardMovedBetweenZones?.Invoke(card.TypeId, sourceZone, targetZone);
                return true;
            }

            if (RemoveCardFromZone(card, sourceZone))
            {
                AddCardToZone(card, targetZone);
                if (!suppressEvents) onCardMovedBetweenZones?.Invoke(card.TypeId, sourceZone, targetZone);
                return true;
            }

            return false;
        }

        // 添加卡牌到临时区（新增方法，不移动卡牌）
        public bool AddToTemporaryZone(CardBase card)
        {
            if (card == null) return false;

            // 检查卡牌是否已经在临时区
            if (temporaryCards.Contains(card)) return true; // 已经在临时区

            // 添加到临时区
            temporaryCards.Add(card);
            if (!suppressEvents) onCardMovedBetweenZones?.Invoke(card.TypeId, GetCardZone(card), CardZone.Temporary);
            return true;
        }

        // 从临时区移除卡牌（新增方法，不移动卡牌）
        public bool RemoveFromTemporaryZone(CardBase card)
        {
            if (card == null) return false;

            if (temporaryCards.Remove(card))
            {
                if (!suppressEvents) onCardMovedBetweenZones?.Invoke(card.TypeId, CardZone.Temporary, GetCardZone(card));
                return true;
            }

            return false;
        }

        // 移动指定类型的卡牌到指定区域（保持原有接口）
        public bool MoveCardToZone(CardTypeId cardTypeId, CardZone sourceZone, CardZone targetZone)
        {
            var card = GetCardByTypeId(cardTypeId, sourceZone);
            if (card != null) return MoveCardToZone(card, targetZone);
            return false;
        }

        // 打乱牌库区（保持原有接口）
        public void ShuffleDeck()
        {
            if (deckCards.Count <= 1) return;

            for (var i = deckCards.Count - 1; i > 0; i--)
            {
                var randomIndex = Random.Range(0, i + 1);
                var temp = deckCards[i];
                deckCards[i] = deckCards[randomIndex];
                deckCards[randomIndex] = temp;
            }
        }

        // 将弃牌区洗入牌库区（保持原有接口）
        public void ShuffleDiscardIntoDeck()
        {
            if (discardCards.Count == 0)
            {
                Debug.Log("弃牌区为空，无需洗牌");
                return;
            }

            // 将弃牌区卡牌加入牌库区
            deckCards.AddRange(discardCards);
            discardCards.Clear();

            // 打乱牌库区
            ShuffleDeck();
        }

        // 将手牌区的所有卡牌移动到弃牌区
        public void DiscardHand()
        {
            if (handCards.Count == 0) return;

            // 为了避免在遍历时修改集合，先复制一份
            var cardsToDiscard = new List<CardBase>(handCards);
            foreach (var card in cardsToDiscard) MoveCardToZone(card, CardZone.Discard);
        }

        // 清空所有卡牌（保持原有接口）
        public void ClearCards()
        {
            var allCards = new List<CardBase>();
            allCards.AddRange(deckCards);
            allCards.AddRange(discardCards);
            allCards.AddRange(handCards);
            allCards.AddRange(consumedCards);
            allCards.AddRange(temporaryCards);

            foreach (var card in allCards)
                if (!suppressEvents) onCardRemoved?.Invoke(card.TypeId);

            deckCards.Clear();
            discardCards.Clear();
            handCards.Clear();
            consumedCards.Clear();
            temporaryCards.Clear();
        }

        // 重置到新关卡状态（包含：将弃牌、手牌、临时区、消耗区卡牌全部回到牌库区）
        public void ResetForNewLevel()
        {
            // 将所有非牌库区的卡牌都移回牌库区
            deckCards.AddRange(discardCards);
            deckCards.AddRange(handCards);
            deckCards.AddRange(temporaryCards);
            deckCards.AddRange(consumedCards);

            discardCards.Clear();
            handCards.Clear();
            consumedCards.Clear(); // 消耗区卡牌已回流到牌库，这里清空容器
            temporaryCards.Clear(); // 临时区卡牌已回流到牌库，这里清空容器

            // 打乱牌库区
            ShuffleDeck();

            Debug.Log($"已重置为新关卡状态，牌库区卡牌数量: {deckCards.Count}");
        }

        // 处理临时区卡牌（新增方法）
        public void ProcessTemporaryCards()
        {
            if (temporaryCards.Count == 0) return;

            var cardsToProcess = new List<CardBase>(temporaryCards);
            foreach (var card in cardsToProcess)
                // 根据卡牌类型决定最终去向
                if (card.Template.isConsumable)
                    MoveCardToZone(card, CardZone.Consumed);
                else
                    MoveCardToZone(card, CardZone.Discard);
        }

        // 私有辅助方法
        private CardZone GetCardZone(CardBase card)
        {
            if (deckCards.Contains(card)) return CardZone.Deck;
            if (discardCards.Contains(card)) return CardZone.Discard;
            if (consumedCards.Contains(card)) return CardZone.Consumed;
            if (temporaryCards.Contains(card)) return CardZone.Temporary;
            if (handCards.Contains(card)) return CardZone.Hand;

            Debug.LogWarning($"卡牌 {card.TypeId.Id} 不在任何区域中");
            return CardZone.Deck; // 默认返回牌库区
        }

        private bool RemoveCardFromZone(CardBase card, CardZone zone)
        {
            switch (zone)
            {
                case CardZone.Deck: return deckCards.Remove(card);
                case CardZone.Discard: return discardCards.Remove(card);
                case CardZone.Hand: return handCards.Remove(card);
                case CardZone.Consumed: return consumedCards.Remove(card);
                case CardZone.Temporary: return temporaryCards.Remove(card);
                default: return false;
            }
        }

        private void AddCardToZone(CardBase card, CardZone zone)
        {
            switch (zone)
            {
                case CardZone.Deck: deckCards.Add(card); break;
                case CardZone.Discard: discardCards.Add(card); break;
                case CardZone.Hand: handCards.Add(card); break;
                case CardZone.Consumed: consumedCards.Add(card); break;
                case CardZone.Temporary: temporaryCards.Add(card); break;
            }
        }

        // 从快照重建所有区域（保持给定顺序）；suppress控制是否抑制事件
        public void RestoreFromSnapshot(IList<CardTypeId> deck,
            IList<CardTypeId> discard,
            IList<CardTypeId> hand,
            IList<CardTypeId> consumed,
            IList<CardTypeId> temporary,
            bool suppress = false)
        {
            var prevSuppress = suppressEvents;
            suppressEvents = suppress;
            try
            {
                ClearCards();

                // 第一步：将所有卡牌先以AddCard添加到牌库（会触发onCardAdded，非静默时刷新UI）
                var totalToAdd = new List<CardTypeId>();
                if (deck != null) totalToAdd.AddRange(deck);
                if (discard != null) totalToAdd.AddRange(discard);
                if (hand != null) totalToAdd.AddRange(hand);
                if (consumed != null) totalToAdd.AddRange(consumed);
                if (temporary != null) totalToAdd.AddRange(temporary);

                var addOk = 0;
                foreach (var id in totalToAdd)
                {
                    if (AddCard(id)) addOk++;
                }

                // 第二步：按目标区域数量将牌从牌库移动到对应区域（触发onCardMovedBetweenZones）
                void MoveMany(IList<CardTypeId> ids, CardZone target)
                {
                    if (ids == null) return;
                    var moved = 0;
                    foreach (var id in ids)
                    {
                        var card = GetCardByTypeId(id, CardZone.Deck);
                        if (card == null)
                        {
                            continue;
                        }
                        if (MoveCardToZone(card, target)) moved++;
                    }
                }

                MoveMany(discard, CardZone.Discard);
                MoveMany(hand, CardZone.Hand);
                MoveMany(consumed, CardZone.Consumed);
                // 临时区是添加引用，不移出源区域；此处从手牌/牌库添加引用
                if (temporary != null)
                {
                    var tempAdded = 0;
                    foreach (var id in temporary)
                    {
                        // 优先从手牌拿引用，否则从牌库
                        var from = GetCardByTypeId(id, CardZone.Hand) ?? GetCardByTypeId(id, CardZone.Deck);
                        if (from != null && AddToTemporaryZone(from)) tempAdded++;
                    }
                }

            }
            finally
            {
                suppressEvents = prevSuppress;
            }
        }
    }
}