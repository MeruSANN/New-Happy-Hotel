using HappyHotel.Enemy;
using HappyHotel.Intent;
using HappyHotel.Intent.Components;
using HappyHotel.Intent.Templates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI
{
	// 敌人意图显示器：监听指定敌人的意图变化并显示
	public class EnemyIntentDisplay : MonoBehaviour
	{
		[Header("UI组件")]
		[SerializeField] private Image intentIcon;
		[SerializeField] private TextMeshProUGUI intentValueText;

		private EnemyBase targetEnemy;
		private TurnEndIntentExecutorComponent intentExecutor;
		private IntentBase lastDisplayedIntent;

		private void Awake()
		{
			// 验证必要组件
			if (intentIcon == null)
			{
				Debug.LogError("EnemyIntentDisplay: intentIcon未设置");
			}

			if (intentValueText == null)
			{
				Debug.LogError("EnemyIntentDisplay: intentValueText未设置");
			}
		}

        private void Update()
        {
            // 移除每帧轮询，保留为空或做心跳
        }

		private void OnDestroy()
		{
			// 清理监听
			UnsubscribeFromEnemy();
		}

		// 设置要监听的敌人
		public void SetTargetEnemy(EnemyBase enemy)
		{
			// 先取消之前的监听
			UnsubscribeFromEnemy();

			targetEnemy = enemy;
			
			if (targetEnemy != null)
			{
				intentExecutor = targetEnemy.GetBehaviorComponent<TurnEndIntentExecutorComponent>();
				if (intentExecutor != null)
				{
					intentExecutor.onCurrentIntentChanged += OnExecutorIntentChanged;
				}
				UpdateDisplay();
			}
			else
			{
				// 清除显示
				ClearDisplay();
			}
		}

		// 取消监听
		private void UnsubscribeFromEnemy()
		{
			targetEnemy = null;
			if (intentExecutor != null)
			{
				intentExecutor.onCurrentIntentChanged -= OnExecutorIntentChanged;
				intentExecutor = null;
			}
			lastDisplayedIntent = null;
		}

		// 敌人被销毁时的回调
		private void OnEnemyDestroyed()
		{
			ClearDisplay();
			UnsubscribeFromEnemy();
		}

		// 检查意图变化
        private void CheckIntentChange() { }

        private void OnExecutorIntentChanged(IntentBase current)
        {
            lastDisplayedIntent = current;
            UpdateDisplay();
        }

		// 检查意图是否发生变化
		private bool HasIntentChanged(IntentBase currentIntent)
		{
			// 如果之前没有意图，现在有意图，则发生变化
			if (lastDisplayedIntent == null && currentIntent != null) return true;
			
			// 如果之前有意图，现在没有意图，则发生变化
			if (lastDisplayedIntent != null && currentIntent == null) return true;
			
			// 如果两个意图都存在，检查类型ID和数值是否相同
			if (lastDisplayedIntent != null && currentIntent != null)
			{
				return !lastDisplayedIntent.TypeId.Equals(currentIntent.TypeId) || 
					   lastDisplayedIntent.GetDisplayValue() != currentIntent.GetDisplayValue();
			}
			
			return false;
		}

		// 更新显示内容
		private void UpdateDisplay()
		{
			if (targetEnemy == null || intentExecutor == null)
			{
				ClearDisplay();
				return;
			}

			// 使用缓存意图
			var currentIntent = intentExecutor.GetCachedCurrentIntent();
			if (currentIntent == null)
			{
				ClearDisplay();
				return;
			}

			// 显示意图图标
			if (intentIcon != null)
			{
				var template = currentIntent.Template;
				if (template != null && template.icon != null)
				{
					intentIcon.sprite = template.icon;
					intentIcon.enabled = true;
				}
				else
				{
					intentIcon.sprite = null;
					intentIcon.enabled = false;
				}
			}

			// 显示意图数值
			if (intentValueText != null)
			{
				var value = currentIntent.GetDisplayValue();
				intentValueText.text = value;
				intentValueText.enabled = true;
			}
		}

		// 清除显示
		private void ClearDisplay()
		{
			if (intentIcon != null)
			{
				intentIcon.sprite = null;
				intentIcon.enabled = false;
			}

			if (intentValueText != null)
			{
				intentValueText.text = "";
				intentValueText.enabled = false;
			}
		}

		// 手动刷新显示（用于数值变化时调用）
		public void RefreshDisplay()
		{
			UpdateDisplay();
		}

		// 获取当前监听的敌人
		public EnemyBase GetTargetEnemy()
		{
			return targetEnemy;
		}
	}
}
