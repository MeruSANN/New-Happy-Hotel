using HappyHotel.Enemy;
using HappyHotel.GameManager;
using TMPro;
using UnityEngine;

namespace HappyHotel.UI
{
    // 怪物刷新倒计时显示控制器
    public class EnemySpawnCountdownController : MonoBehaviour
    {
        [Header("UI组件")] [SerializeField] private TextMeshProUGUI countdownText; // 倒计时文本

        [SerializeField] private GameObject countdownPanel; // 倒计时面板（可选，用于控制显示/隐藏）

        [Header("显示设置")] [SerializeField] private string countdownFormat = "下一波怪物援军\n{0} 回合"; // 倒计时格式

        [SerializeField] private string noSpawnText = "暂无怪物刷新"; // 无刷新时显示的文本
        [SerializeField] private string enemiesClearedText = "怪物已清空"; // 敌人被清空时显示的文本

        [Header("颜色设置")] [SerializeField] private Color numberColor = Color.red; // 数字颜色

        [SerializeField] private Color textColor = Color.white; // 其他文字颜色

        private void Update()
        {
            // 每帧更新倒计时显示
            UpdateCountdownDisplay();
        }

        // 更新倒计时显示
        private void UpdateCountdownDisplay()
        {
            if (countdownText == null) return;

            // 检查管理器是否可用
            if (WaveSpawnManager.Instance == null)
            {
                SetDisplayText(noSpawnText, textColor);
                SetPanelVisibility(false);
                return;
            }

            // 检查是否所有敌人都被清空
            if (IsAllEnemiesCleared())
            {
                SetDisplayText(enemiesClearedText, textColor);
                SetPanelVisibility(true);
                return;
            }

            // 获取距离下次刷新的回合数
            var turnsUntilSpawn = WaveSpawnManager.Instance.GetTurnsUntilNextScheduledWave();

            if (turnsUntilSpawn < 0)
            {
                // 没有配置或刷新信息
                SetDisplayText(noSpawnText, textColor);
                SetPanelVisibility(false);
            }
            else if (turnsUntilSpawn == 0)
            {
                // 即将刷新，使用数字颜色显示
                SetDisplayText(string.Format(countdownFormat, turnsUntilSpawn), numberColor);
                SetPanelVisibility(true);
            }
            else
            {
                // 显示倒计时，使用混合颜色
                SetDisplayTextWithMixedColors(string.Format(countdownFormat, turnsUntilSpawn));
                SetPanelVisibility(true);
            }
        }

        // 检查是否所有敌人都被清空
        private bool IsAllEnemiesCleared()
        {
            if (EnemyController.Instance == null) return true; // 如果敌人控制器不存在，认为敌人已被清空

            return EnemyController.Instance.GetCurrentEnemyCount() == 0;
        }

        // 设置显示文本（单色）
        private void SetDisplayText(string text, Color color)
        {
            if (countdownText != null)
            {
                countdownText.text = text;
                countdownText.color = color;
            }
        }

        // 设置显示文本（混合颜色：数字用numberColor，其他文字用textColor）
        private void SetDisplayTextWithMixedColors(string text)
        {
            if (countdownText == null) return;

            // 查找数字部分并应用不同颜色
            var formattedText = text;

            // 使用富文本标签来设置不同颜色
            // 将数字部分用红色标签包围
            for (var i = 0; i < text.Length; i++)
                if (char.IsDigit(text[i]))
                {
                    // 找到数字的开始位置
                    var startIndex = i;
                    while (i < text.Length && char.IsDigit(text[i])) i++;
                    var endIndex = i;

                    // 提取数字部分
                    var numberPart = text.Substring(startIndex, endIndex - startIndex);
                    var beforeNumber = text.Substring(0, startIndex);
                    var afterNumber = text.Substring(endIndex);

                    // 使用富文本标签设置颜色
                    formattedText = beforeNumber +
                                    $"<color=#{ColorUtility.ToHtmlStringRGB(numberColor)}>{numberPart}</color>" +
                                    afterNumber;
                    break; // 只处理第一个数字
                }

            countdownText.text = formattedText;
            countdownText.color = textColor; // 设置默认文字颜色
        }

        // 设置面板可见性
        private void SetPanelVisibility(bool visible)
        {
            if (countdownPanel != null) countdownPanel.SetActive(visible);
        }

        // 获取当前显示的倒计时回合数
        public int GetCurrentCountdown()
        {
            return WaveSpawnManager.Instance != null
                ? WaveSpawnManager.Instance.GetTurnsUntilNextScheduledWave()
                : -1;
        }

        // 手动刷新显示
        public void RefreshDisplay()
        {
            UpdateCountdownDisplay();
        }

        // 设置倒计时格式
        public void SetCountdownFormat(string format)
        {
            countdownFormat = format;
            UpdateCountdownDisplay();
        }

        // 设置数字颜色
        public void SetNumberColor(Color color)
        {
            numberColor = color;
            UpdateCountdownDisplay();
        }

        // 设置文字颜色
        public void SetTextColor(Color color)
        {
            textColor = color;
            UpdateCountdownDisplay();
        }
    }
}