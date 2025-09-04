using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing.Processors;
using HappyHotel.GameManager;
using UnityEngine;

namespace HappyHotel.Core.ValueProcessing.Components
{
    // 护甲值组件 - 使用新的数值处理系统
    [DependsOnComponent(typeof(HitPointValueComponent))]
    public class ArmorValueComponent : BehaviorComponentBase
    {
        private ArmorValueProcessor armorProcessor;

        private HitPointValueComponent hitPointComponent;
        private bool isGameStateListenerRegistered; // 标记是否已注册游戏状态监听

        private bool subscribedPlayerTurnStart;
        private bool subscribedEnemyTurnStart;

        public ArmorValue ArmorValue { get; private set; }

        public int CurrentArmor => ArmorValue?.CurrentValue ?? 0;

        public override void OnAttach(BehaviorComponentContainer host)
        {
            base.OnAttach(host);

            // 获取生命值组件
            hitPointComponent = host.GetBehaviorComponent<HitPointValueComponent>();

            // 只在护甲值未初始化时才创建新实例，避免重复初始化导致数值丢失
            if (ArmorValue == null)
            {
                ArmorValue = new ArmorValue();
                ArmorValue.Initialize(host);
            }

            // 创建并注册护甲处理器到生命值
            if (hitPointComponent != null && armorProcessor == null)
            {
                armorProcessor = new ArmorValueProcessor(ArmorValue);
                hitPointComponent.RegisterProcessor(armorProcessor);
                Debug.Log($"{host.gameObject.name} 注册护甲处理器到生命值");
            }
            else if (hitPointComponent == null)
            {
                Debug.LogWarning($"{host.gameObject.name} 无法获取HitPointValueComponent，跳过护甲处理器注册");
            }

            // 按所属阵营订阅回合阶段事件
            if (host != null)
            {
                if (host.HasTag("Enemy"))
                {
                    TurnManager.onEnemyTurnStart += OnEnemyTurnStart;
                    subscribedEnemyTurnStart = true;
                }

                if (host.HasTag("Character") || host.HasTag("MainCharacter"))
                {
                    TurnManager.onPlayerTurnStart += OnPlayerTurnStart;
                    subscribedPlayerTurnStart = true;
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            // 注销处理器
            if (hitPointComponent != null && armorProcessor != null)
                hitPointComponent.UnregisterProcessor(armorProcessor);

            // 取消监听游戏状态变化
            ArmorValue?.Dispose();

            // 取消回合阶段事件监听
            if (subscribedEnemyTurnStart)
            {
                TurnManager.onEnemyTurnStart -= OnEnemyTurnStart;
                subscribedEnemyTurnStart = false;
            }

            if (subscribedPlayerTurnStart)
            {
                TurnManager.onPlayerTurnStart -= OnPlayerTurnStart;
                subscribedPlayerTurnStart = false;
            }
        }

        // 玩家回合开始时（用于角色）清理护甲
        private void OnPlayerTurnStart(int turnNumber)
        {
            if (CurrentArmor > 0)
            {
                ClearArmor();
                Debug.Log($"{host?.gameObject.name} 在玩家回合开始时清空护甲");
            }
        }

        // 敌人回合开始时（用于敌人）清理护甲
        private void OnEnemyTurnStart(int turnNumber)
        {
            if (CurrentArmor > 0)
            {
                ClearArmor();
                Debug.Log($"{host?.gameObject.name} 在敌人回合开始时清空护甲");
            }
        }

        // 添加护甲
        public int AddArmor(int amount, IComponentContainer source = null)
        {
            return ArmorValue?.AddArmor(amount, source) ?? 0;
        }

        // 清除护甲
        public void ClearArmor()
        {
            ArmorValue?.ClearArmor();
        }

        // 注册处理器到护甲值本身
        public void RegisterProcessor(IValueProcessor processor)
        {
            ArmorValue?.RegisterProcessor(processor);
        }

        // 注销处理器
        public void UnregisterProcessor(IValueProcessor processor)
        {
            ArmorValue?.UnregisterProcessor(processor);
        }
    }
}