using HappyHotel.TurnRestart;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI
{
    // 重启回合按钮控制脚本
    public class TurnRestartButton : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private Button restartButton;

        private void Start()
        {
            // 如果没有手动指定按钮，尝试从当前GameObject获取
            if (restartButton == null)
                restartButton = GetComponent<Button>();

            // 添加按钮点击事件
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartButtonClicked);
            }

            // 初始状态更新
            UpdateButtonState();
        }

        private void Update()
        {
            // 每帧检查状态并更新按钮
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            if (restartButton == null) return;

            // 检查是否可以重启回合
            bool canRestart = TurnRestartService.Instance != null && 
                              TurnRestartService.Instance.CanRestartNow();

            // 设置按钮交互状态
            restartButton.interactable = canRestart;
        }

        private void OnRestartButtonClicked()
        {
            // 再次检查是否可以重启（防止在点击时状态发生变化）
            if (TurnRestartService.Instance != null && 
                TurnRestartService.Instance.CanRestartNow())
            {
                // 执行重启回合
                TurnRestartService.Instance.RestoreSnapshot();
                
                // 可以在这里添加音效或动画效果
                Debug.Log("[TurnRestartButton] 回合已重启");
            }
        }

        private void OnDestroy()
        {
            // 清理事件监听
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartButtonClicked);
            }
        }
    }
}
