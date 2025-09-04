using System.Collections.Generic;
using HappyHotel.Action;
using HappyHotel.Core.ValueProcessing.Components;
using HappyHotel.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI.HoverDisplay.EnemyHover
{
    // 敌人悬停显示UI，专门用于显示敌人的详细信息
    public class EnemyHoverDisplayUI : HoverDisplayUI
    {
        [Header("敌人信息UI组件")] [SerializeField] private Image enemyIcon;

        [SerializeField] private TextMeshProUGUI enemyNameText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI attackPowerText;
        [SerializeField] private TMP_Text armorText; // 护甲显示文本

        [Header("Buff显示")] [SerializeField] private BuffListDisplayer buffListDisplayer; // Buff列表显示器

        [Header("Canvas设置")] [SerializeField] private Canvas targetCanvas; // 指定用于位置计算的Canvas
        
        private EnemyHoverData currentEnemyData;

        protected override void Awake()
        {
            base.Awake();

            // 如果没有指定targetCanvas，尝试找到父级Canvas
            if (targetCanvas == null) targetCanvas = GetComponentInParent<Canvas>();
        }

        // 重写UpdatePosition方法，使用指定的Canvas
        public override void UpdatePosition(Vector2 screenPosition)
        {
            UpdatePosition(screenPosition, targetCanvas);
        }

        // 重写ShowHoverInfo方法以正确处理EnemyHoverData
        public override void ShowHoverInfo(HoverDisplayData data)
        {
            // 检查数据类型
            if (data is EnemyHoverData enemyData)
            {
                currentEnemyData = enemyData;
                UpdateEnemyDisplay(enemyData);
                base.ShowHoverInfo(data);
            }
            else
            {
                Debug.LogWarning($"EnemyHoverDisplayUI接收到非EnemyHoverData类型的数据: {data?.GetType().Name}");
                base.ShowHoverInfo(data);
            }
        }

        // 更新敌人显示
        protected override void UpdateDisplay(HoverDisplayData data)
        {
            if (currentEnemyData != null) UpdateEnemyDisplay(currentEnemyData);
        }

        // 更新敌人显示内容
        private void UpdateEnemyDisplay(EnemyHoverData enemyData)
        {
            if (enemyData == null) return;

            // 更新敌人图标
            if (enemyIcon != null)
            {
                enemyIcon.sprite = enemyData.enemyIcon;
                enemyIcon.gameObject.SetActive(enemyData.enemyIcon != null);
            }

            // 更新敌人名字
            if (enemyNameText != null) enemyNameText.text = enemyData.enemyName;

            // 更新血量信息
            UpdateHealthDisplay(enemyData);

            // 更新攻击力信息
            UpdateAttackPowerDisplay(enemyData);

            // 更新护甲信息
            UpdateArmorDisplay(enemyData);

            // 绑定Buff列表显示器到当前敌人
            if (buffListDisplayer != null && enemyData.enemy != null)
            {
                buffListDisplayer.SetRelatedContainer(enemyData.enemy);
            }
        }

        // 更新血量显示
        private void UpdateHealthDisplay(EnemyHoverData enemyData)
        {
            if (healthText != null) healthText.text = $"{enemyData.currentHealth} / {enemyData.maxHealth}";
        }

        // 更新攻击力显示
        private void UpdateAttackPowerDisplay(EnemyHoverData enemyData)
        {
            if (attackPowerText != null) attackPowerText.text = $"{enemyData.attackPower}";
        }

        // 更新护甲显示（为0时显示0，不隐藏）
        private void UpdateArmorDisplay(EnemyHoverData enemyData)
        {
            if (armorText == null) return;

            var armorValue = 0;
            var enemy = enemyData.enemy;
            if (enemy != null)
            {
                var armorComponent = enemy.GetBehaviorComponent<ArmorValueComponent>();
                if (armorComponent != null) armorValue = armorComponent.CurrentArmor;
            }

            armorText.text = $"{armorValue}";
        }

        // 隐藏悬停信息
        public override void HideHoverInfo()
        {
            base.HideHoverInfo();
            currentEnemyData = null;
        }

        // 实时更新敌人数据
        public void UpdateEnemyData()
        {
            if (currentEnemyData != null)
            {
                currentEnemyData.UpdateData();
                UpdateEnemyDisplay(currentEnemyData);
            }
        }

        // 获取当前敌人数据
        public EnemyHoverData GetCurrentEnemyData()
        {
            return currentEnemyData;
        }

        // 设置目标Canvas
        public void SetTargetCanvas(Canvas canvas)
        {
            targetCanvas = canvas;
        }
    }
}