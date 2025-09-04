using System;
using System.Collections.Generic;
using HappyHotel.Core.Singleton;
using UnityEngine;

namespace HappyHotel.GameManager
{
    [ManagedSingleton(true)]
    public class CharacterSelectionManager : SingletonBase<CharacterSelectionManager>
    {
        [Header("默认角色")] [SerializeField] [Tooltip("默认选择的角色类型ID（如果未找到则使用第一个可用角色）")]
        private string defaultCharacterTypeId = "Default";

        // 所有可选择的角色配置列表
        private CharacterSelectionConfig[] availableCharacters = new CharacterSelectionConfig[0];

        // 默认角色
        private CharacterSelectionConfig defaultCharacter;

        // 当前选择的角色
        private CharacterSelectionConfig selectedCharacter;

        // 角色选择事件
        public static event Action<CharacterSelectionConfig> onCharacterSelected;

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();

            // 从Resources加载角色配置
            LoadCharacterConfigsFromResources();

            // 设置默认角色
            SetDefaultCharacter();

            // 设置默认选择
            selectedCharacter = defaultCharacter;

            Debug.Log($"CharacterSelectionManager 初始化完成，加载了 {availableCharacters.Length} 个角色配置");
        }

        // 获取所有可用角色
        public CharacterSelectionConfig[] GetAvailableCharacters()
        {
            return availableCharacters;
        }

        // 获取默认角色
        public CharacterSelectionConfig GetDefaultCharacter()
        {
            return defaultCharacter;
        }

        // 获取当前选择的角色
        public CharacterSelectionConfig GetSelectedCharacter()
        {
            return selectedCharacter ?? defaultCharacter;
        }

        // 选择角色
        public void SelectCharacter(CharacterSelectionConfig character)
        {
            if (character == null)
            {
                Debug.LogWarning("尝试选择空角色配置");
                return;
            }

            selectedCharacter = character;
            onCharacterSelected?.Invoke(selectedCharacter);
            Debug.Log($"已选择角色: {character.CharacterName}");
        }

        // 通过索引选择角色
        public void SelectCharacterByIndex(int index)
        {
            if (index < 0 || index >= availableCharacters.Length)
            {
                Debug.LogWarning($"角色索引超出范围: {index}");
                return;
            }

            SelectCharacter(availableCharacters[index]);
        }

        // 通过角色类型ID选择角色
        public void SelectCharacterByTypeId(string typeId)
        {
            foreach (var character in availableCharacters)
                if (character.CharacterTypeId == typeId)
                {
                    SelectCharacter(character);
                    return;
                }

            Debug.LogWarning($"未找到角色类型ID为 {typeId} 的角色");
        }

        // 检查角色是否可用
        public bool IsCharacterAvailable(CharacterSelectionConfig character)
        {
            if (character == null)
                return false;

            foreach (var availableCharacter in availableCharacters)
                if (availableCharacter == character)
                    return true;

            return false;
        }

        // 获取角色索引
        public int GetCharacterIndex(CharacterSelectionConfig character)
        {
            for (var i = 0; i < availableCharacters.Length; i++)
                if (availableCharacters[i] == character)
                    return i;

            return -1;
        }

        // 获取可用角色数量
        public int GetAvailableCharacterCount()
        {
            return availableCharacters.Length;
        }


        // 重置选择为默认角色
        public void ResetToDefaultCharacter()
        {
            SelectCharacter(defaultCharacter);
        }

        // 清除选择
        public void ClearSelection()
        {
            selectedCharacter = null;
            Debug.Log("已清除角色选择");
        }

        // 从Resources文件夹加载角色配置
        private void LoadCharacterConfigsFromResources()
        {
            try
            {
                // 从Resources/CharacterSelectionConfigs文件夹加载所有CharacterSelectionConfig
                var configs = Resources.LoadAll<CharacterSelectionConfig>("CharacterSelectionConfigs");

                if (configs.Length == 0)
                {
                    Debug.LogWarning("在Resources/CharacterSelectionConfigs文件夹中未找到角色配置");
                    availableCharacters = new CharacterSelectionConfig[0];
                    return;
                }

                // 过滤有效的配置
                var validConfigs = new List<CharacterSelectionConfig>();
                foreach (var config in configs)
                    if (config != null && config.IsValid())
                        validConfigs.Add(config);
                    else
                        Debug.LogWarning($"角色配置无效或为空: {config?.name ?? "null"}");

                availableCharacters = validConfigs.ToArray();
                Debug.Log($"从Resources/CharacterSelectionConfigs加载了 {availableCharacters.Length} 个有效角色配置");
            }
            catch (Exception e)
            {
                Debug.LogError($"加载角色配置时发生错误: {e.Message}");
                availableCharacters = new CharacterSelectionConfig[0];
            }
        }

        // 设置默认角色
        private void SetDefaultCharacter()
        {
            // 首先尝试通过类型ID找到默认角色
            if (!string.IsNullOrEmpty(defaultCharacterTypeId))
            {
                foreach (var character in availableCharacters)
                    if (character.CharacterTypeId == defaultCharacterTypeId)
                    {
                        defaultCharacter = character;
                        Debug.Log($"设置默认角色: {character.CharacterName} (类型ID: {defaultCharacterTypeId})");
                        return;
                    }

                Debug.LogWarning($"未找到类型ID为 {defaultCharacterTypeId} 的角色，使用第一个可用角色作为默认角色");
            }

            // 如果未找到指定类型ID的角色，使用第一个可用角色
            if (availableCharacters.Length > 0)
            {
                defaultCharacter = availableCharacters[0];
                Debug.Log($"设置默认角色: {defaultCharacter.CharacterName} (第一个可用角色)");
            }
            else
            {
                defaultCharacter = null;
                Debug.LogWarning("没有可用的角色配置，无法设置默认角色");
            }
        }

        // 重新加载角色配置
        public void ReloadCharacterConfigs()
        {
            Debug.Log("重新加载角色配置...");
            LoadCharacterConfigsFromResources();
            SetDefaultCharacter();

            // 如果当前选择的角色不在新列表中，重置为默认角色
            if (selectedCharacter != null && !IsCharacterAvailable(selectedCharacter))
            {
                selectedCharacter = defaultCharacter;
                Debug.Log("当前选择的角色已不可用，重置为默认角色");
            }
        }

        // 设置默认角色类型ID
        public void SetDefaultCharacterTypeId(string typeId)
        {
            defaultCharacterTypeId = typeId;
            SetDefaultCharacter();

            // 如果当前没有选择角色，设置为默认角色
            if (selectedCharacter == null) selectedCharacter = defaultCharacter;

            Debug.Log($"已设置默认角色类型ID: {typeId}");
        }

        // 获取默认角色类型ID
        public string GetDefaultCharacterTypeId()
        {
            return defaultCharacterTypeId;
        }
    }
}