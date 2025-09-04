#if UNITY_EDITOR
using System;
using System.IO;
using HappyHotel.Action.Templates;
using HappyHotel.Character.Templates;
using HappyHotel.Device.Templates;
using HappyHotel.Enemy.Templates;
using HappyHotel.Equipment.Templates;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace HappyHotel.Core.Editor
{
    public class QuickCreateTemplateWindow : OdinEditorWindow
    {
        public enum TemplateType
        {
            [LabelText("装备模板")] Equipment,
            [LabelText("道具模板")] Prop,
            [LabelText("角色模板")] Character,
            [LabelText("敌人模板")] Enemy,
            [LabelText("动作模板")] Action,
            [LabelText("装置模板")] Device
        }

        [TitleGroup("模板类型")] [EnumToggleButtons] [OnValueChanged("OnTemplateTypeChanged")]
        public TemplateType selectedTemplateType = TemplateType.Equipment;

        [TitleGroup("基本信息")] [LabelText("模板名称")] [ValidateInput("ValidateTemplateName", "模板名称不能为空")]
        public string templateName = "";

        [LabelText("图标/精灵")] public Sprite templateSprite;

        [ShowIf("IsEquipmentTemplate")] [LabelText("稀有度")]
        public Rarity.Rarity rarity = Rarity.Rarity.Common;

        [ShowIf("IsPropTemplate")] [LabelText("触发时自动销毁")]
        public bool autoDestroyOnTrigger;

        [TitleGroup("角色敌人专用设置")] [ShowIf("IsCharacterOrEnemyTemplate")] [LabelText("基础生命值")]
        public int baseHealth = 5;

        [TitleGroup("描述")] [ShowIf("HasDescriptionField")] [LabelText("描述")] [TextArea(2, 4)]
        public string description = "";

        private EnhancedUnifiedTemplateEditor parentWindow;

        [TitleGroup("预设模板")]
        [ShowIf("IsEquipmentTemplate")]
        [HorizontalGroup("预设模板/Equipment")]
        [Button("普通武器")]
        private void LoadCommonWeaponPreset()
        {
            templateName = "新武器";
            rarity = Rarity.Rarity.Common;
            description = "一把普通的武器";
        }

        [HorizontalGroup("预设模板/Equipment")]
        [Button("稀有装备")]
        private void LoadRareEquipmentPreset()
        {
            templateName = "稀有装备";
            rarity = Rarity.Rarity.Rare;
            description = "一件稀有的装备";
        }

        [ShowIf("IsPropTemplate")]
        [HorizontalGroup("预设模板/Prop")]
        [Button("消耗道具")]
        private void LoadConsumablePropPreset()
        {
            templateName = "消耗道具";
            autoDestroyOnTrigger = true;
            description = "使用后会消失的道具";
        }

        [HorizontalGroup("预设模板/Prop")]
        [Button("永久道具")]
        private void LoadPermanentPropPreset()
        {
            templateName = "永久道具";
            autoDestroyOnTrigger = false;
            description = "可以重复使用的道具";
        }

        [ShowIf("IsCharacterOrEnemyTemplate")]
        [HorizontalGroup("预设模板/Character")]
        [Button("普通角色")]
        private void LoadNormalCharacterPreset()
        {
            templateName = selectedTemplateType == TemplateType.Character ? "新角色" : "新敌人";
            baseHealth = selectedTemplateType == TemplateType.Character ? 5 : 3;
        }

        [HorizontalGroup("预设模板/Character")]
        [Button("强力角色")]
        private void LoadStrongCharacterPreset()
        {
            templateName = selectedTemplateType == TemplateType.Character ? "强力角色" : "强力敌人";
            baseHealth = selectedTemplateType == TemplateType.Character ? 10 : 8;
        }

        [TitleGroup("操作")]
        [HorizontalGroup("操作/Buttons")]
        [Button("创建模板", ButtonSizes.Large)]
        [EnableIf("CanCreateTemplate")]
        private void CreateTemplate()
        {
            try
            {
                var newTemplate = CreateTemplateInstance();
                if (newTemplate != null)
                {
                    var folderPath = GetResourcesFolderPath(selectedTemplateType);

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                        AssetDatabase.Refresh();
                    }

                    var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{templateName}.asset");
                    AssetDatabase.CreateAsset(newTemplate, assetPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = newTemplate;

                    // 刷新主窗口
                    if (parentWindow != null) parentWindow.ForceMenuTreeRebuild();

                    // 显示成功消息
                    EditorUtility.DisplayDialog("创建成功",
                        $"模板 '{templateName}' 已成功创建！\n路径: {assetPath}", "确定");

                    // 重置表单
                    ResetForm();
                }
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("创建失败", $"创建模板时发生错误：{e.Message}", "确定");
            }
        }

        [HorizontalGroup("操作/Buttons")]
        [Button("重置表单", ButtonSizes.Large)]
        private void ResetForm()
        {
            templateName = "";
            templateSprite = null;
            description = "";
            rarity = Rarity.Rarity.Common;
            autoDestroyOnTrigger = false;
            baseHealth = selectedTemplateType == TemplateType.Character ? 5 : 3;
        }

        [HorizontalGroup("操作/Buttons")]
        [Button("取消", ButtonSizes.Large)]
        private void Cancel()
        {
            Close();
        }

        public static void ShowWindow(EnhancedUnifiedTemplateEditor parent)
        {
            var window = GetWindow<QuickCreateTemplateWindow>("快速创建模板", true);
            window.parentWindow = parent;
            window.minSize = new Vector2(450, 600);
            window.maxSize = new Vector2(450, 800);
            window.Show();
        }

        private void OnTemplateTypeChanged()
        {
            switch (selectedTemplateType)
            {
                case TemplateType.Character:
                    baseHealth = 5;
                    break;
                case TemplateType.Enemy:
                    baseHealth = 3;
                    break;
                case TemplateType.Equipment:
                    rarity = Rarity.Rarity.Common;
                    break;
            }
        }

        private ScriptableObject CreateTemplateInstance()
        {
            ScriptableObject template = selectedTemplateType switch
            {
                TemplateType.Equipment => CreateEquipmentTemplate(),
                TemplateType.Prop => CreatePropTemplate(),
                TemplateType.Character => CreateCharacterTemplate(),
                TemplateType.Enemy => CreateEnemyTemplate(),
                TemplateType.Action => CreateActionTemplate(),
                TemplateType.Device => CreateDeviceTemplate(),
                _ => null
            };

            return template;
        }

        private ItemTemplate CreateEquipmentTemplate()
        {
            var template = CreateInstance<ItemTemplate>();
            template.name = templateName;
            template.icon = templateSprite;
            template.description = description;
            template.rarity = rarity;
            return template;
        }

        private ActivePlacementCardTemplate CreatePropTemplate()
        {
            var template = CreateInstance<ActivePlacementCardTemplate>();
            template.name = templateName;
            template.icon = templateSprite;
            template.description = description;
            template.rarity = rarity;
            template.autoDestroyOnTrigger = autoDestroyOnTrigger;
            return template;
        }

        private CharacterTemplate CreateCharacterTemplate()
        {
            var template = CreateInstance<CharacterTemplate>();
            template.name = templateName;
            template.characterSprite = templateSprite;
            template.baseHealth = baseHealth;
            return template;
        }

        private EnemyTemplate CreateEnemyTemplate()
        {
            var template = CreateInstance<EnemyTemplate>();
            template.name = templateName;
            template.enemySprite = templateSprite;
            template.baseHealth = baseHealth;
            return template;
        }

        private ActionTemplate CreateActionTemplate()
        {
            var template = CreateInstance<ActionTemplate>();
            template.name = templateName;
            template.actionSprite = templateSprite;
            return template;
        }

        private DeviceTemplate CreateDeviceTemplate()
        {
            var template = CreateInstance<DeviceTemplate>();
            template.name = templateName;
            template.deviceSprite = templateSprite;
            return template;
        }

        private string GetResourcesFolderPath(TemplateType type)
        {
            var basePath = "Assets/Happy Hotel";
            return type switch
            {
                TemplateType.Equipment => $"{basePath}/Equipment/Resources/Templates",
                TemplateType.Prop => $"{basePath}/Prop/Resources/Templates",
                TemplateType.Character => $"{basePath}/Character/Resources/Templates",
                TemplateType.Enemy => $"{basePath}/Enemy/Resources/Templates",
                TemplateType.Action => $"{basePath}/Action/Resources/Templates",
                TemplateType.Device => $"{basePath}/Device/Resources/Templates",
                _ => $"{basePath}/Templates"
            };
        }

        private bool IsEquipmentTemplate()
        {
            return selectedTemplateType == TemplateType.Equipment;
        }

        private bool IsPropTemplate()
        {
            return selectedTemplateType == TemplateType.Prop;
        }

        private bool IsEnemyTemplate()
        {
            return selectedTemplateType == TemplateType.Enemy;
        }

        private bool IsCharacterOrEnemyTemplate()
        {
            return selectedTemplateType == TemplateType.Character || selectedTemplateType == TemplateType.Enemy;
        }

        private bool HasDescriptionField()
        {
            return selectedTemplateType == TemplateType.Equipment || selectedTemplateType == TemplateType.Prop;
        }

        private bool CanCreateTemplate()
        {
            return !string.IsNullOrEmpty(templateName.Trim());
        }

        private bool ValidateTemplateName(string name)
        {
            return !string.IsNullOrEmpty(name?.Trim());
        }
    }
}
#endif