using System;
using System.Collections.Generic;
using HappyHotel.Action;
using HappyHotel.Action.Components;
using HappyHotel.Core.ValueProcessing.Components;
using HappyHotel.Enemy;
using UnityEngine;

namespace HappyHotel.UI.HoverDisplay.EnemyHover
{
    // 敌人悬停数据结构，包含敌人的详细信息
    [Serializable]
    public class EnemyHoverData : HoverDisplayData
    {
        // 敌人对象
        public EnemyBase enemy;

        // 敌人图标
        public Sprite enemyIcon;

        // 敌人名字
        public string enemyName;

        // 当前血量
        public int currentHealth;

        // 最大血量
        public int maxHealth;

        // 攻击力
        public int attackPower;

        public EnemyHoverData(EnemyBase enemy, Vector3 position, Vector2 mousePos) : base(position, mousePos)
        {
            this.enemy = enemy;

            if (enemy != null)
            {
                // 获取敌人图标和名字
                if (enemy.Template != null)
                {
                    enemyIcon = enemy.Template.enemySprite;
                    enemyName = enemy.Template.enemyName;
                }
                else
                {
                    // 如果没有Template，使用GameObject的名字作为后备
                    enemyName = enemy.gameObject.name;
                }

                // 获取血量信息
                var hitPointComponent = enemy.GetBehaviorComponent<HitPointValueComponent>();
                if (hitPointComponent != null)
                {
                    currentHealth = hitPointComponent.CurrentHitPoint;
                    maxHealth = hitPointComponent.MaxHitPoint;
                }
                else
                {
                    Debug.LogWarning($"敌人 {enemy.name} 没有找到HitPointValueComponent");
                }

                // 获取攻击力信息
                var attackPowerComponent = enemy.GetBehaviorComponent<AttackPowerComponent>();
                if (attackPowerComponent != null)
                {
                    attackPower = attackPowerComponent.GetAttackPower();
                }
                else
                {
                    Debug.LogWarning($"敌人 {enemy.name} 没有找到AttackPowerComponent");
                    attackPower = 0;
                }
            }
        }

        // 更新数据（用于实时更新）
        public void UpdateData()
        {
            if (enemy == null) return;

            // 更新血量信息
            var hitPointComponent = enemy.GetBehaviorComponent<HitPointValueComponent>();
            if (hitPointComponent != null)
            {
                currentHealth = hitPointComponent.CurrentHitPoint;
                maxHealth = hitPointComponent.MaxHitPoint;
            }

            // 更新攻击力信息
            var attackPowerComponent = enemy.GetBehaviorComponent<AttackPowerComponent>();
            if (attackPowerComponent != null)
            {
                attackPower = attackPowerComponent.GetAttackPower();
            }
        }

        // 获取血量百分比
        public float GetHealthPercentage()
        {
            if (maxHealth <= 0) return 0f;
            return (float)currentHealth / maxHealth;
        }
    }
}