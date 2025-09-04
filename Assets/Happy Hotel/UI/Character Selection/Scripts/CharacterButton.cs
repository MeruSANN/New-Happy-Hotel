using System;
using HappyHotel.GameManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI
{
    public class CharacterButton : MonoBehaviour
    {
        [Header("UI组件")] [SerializeField] private TMP_Text characterNameText;

        [SerializeField] private Image characterIconImage;
        [SerializeField] private Button button;

        // 关联的角色配置
        private CharacterSelectionConfig characterConfig;

        private void Awake()
        {
            // 如果没有手动分配按钮组件，自动获取
            if (button == null) button = GetComponent<Button>();

            // 设置按钮点击事件
            if (button != null) button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDestroy()
        {
            // 清理事件监听器
            if (button != null) button.onClick.RemoveListener(OnButtonClicked);
        }

        // 角色选择事件
        public static event Action<CharacterSelectionConfig> onCharacterSelected;

        // 设置角色配置
        public void SetCharacterConfig(CharacterSelectionConfig config)
        {
            characterConfig = config;
            UpdateUI();
        }

        // 更新UI显示
        private void UpdateUI()
        {
            if (characterConfig == null)
                return;

            // 更新角色名称
            if (characterNameText != null) characterNameText.text = characterConfig.CharacterName;

            // 更新角色图标
            if (characterIconImage != null && characterConfig.CharacterIcon != null)
            {
                characterIconImage.sprite = characterConfig.CharacterIcon;
                characterIconImage.gameObject.SetActive(true);
            }
            else if (characterIconImage != null)
            {
                characterIconImage.gameObject.SetActive(false);
            }
        }

        // 按钮点击处理
        private void OnButtonClicked()
        {
            if (characterConfig != null)
            {
                Debug.Log($"角色按钮被点击: {characterConfig.CharacterName}");
                onCharacterSelected?.Invoke(characterConfig);
            }
        }

        // 获取关联的角色配置
        public CharacterSelectionConfig GetCharacterConfig()
        {
            return characterConfig;
        }

        // 设置按钮是否可交互
        public void SetInteractable(bool interactable)
        {
            if (button != null) button.interactable = interactable;
        }

        // 设置按钮文字颜色
        public void SetTextColor(Color color)
        {
            if (characterNameText != null) characterNameText.color = color;
        }
    }
}