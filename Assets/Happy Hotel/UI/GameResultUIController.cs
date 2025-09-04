using HappyHotel.Core.Scene;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HappyHotel.UI
{
    // 游戏结算面板控制器
    public class GameResultUIController : MonoBehaviour
    {
        [Header("UI组件")] [SerializeField] private TextMeshProUGUI resultTMPText; // 结果文字（TMP）

        [SerializeField] private Button confirmButton; // 返回主菜单按钮

        [Header("文本配置")] [SerializeField] private string winText = "胜利"; // 胜利时显示文本

        [SerializeField] private string loseText = "失败"; // 失败时显示文本

        [Header("场景配置")] [SerializeField] private string mainMenuSceneName = "MainMenu"; // 主菜单场景名

        private void Awake()
        {
            // 绑定按钮
            if (confirmButton) confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        private void OnDestroy()
        {
            // 解绑按钮
            if (confirmButton) confirmButton.onClick.RemoveListener(OnConfirmClicked);
        }

        // 显示结算面板
        public void Show(bool isWin)
        {
            gameObject.SetActive(true);
            var text = isWin ? winText : loseText;
            UpdateResultText(text);
        }

        // 隐藏结算面板
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        // 更新结果文本
        private void UpdateResultText(string text)
        {
            if (resultTMPText != null)
            {
                resultTMPText.text = text;
                return;
            }

            // 尝试在子物体中寻找文字组件
            var tmp = GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
            {
                resultTMPText = tmp;
                resultTMPText.text = text;
            }
        }

        // 返回主菜单
        private void OnConfirmClicked()
        {
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.SwitchScene(mainMenuSceneName);
            else
                SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}