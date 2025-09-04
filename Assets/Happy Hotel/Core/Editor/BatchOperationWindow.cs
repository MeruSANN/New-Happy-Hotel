#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public class BatchOperationWindow : OdinEditorWindow
    {
        public enum BatchOperationType
        {
            [LabelText("批量重命名")] Rename,
            [LabelText("批量移动")] Move,
            [LabelText("批量设置属性")] SetProperties,
            [LabelText("批量复制")] Duplicate,
            [LabelText("批量删除")] Delete,
            [LabelText("批量修复")] Repair
        }

        public enum TemplateTypeFilter
        {
            [LabelText("全部")] All,
            [LabelText("装备")] Equipment,
            [LabelText("道具")] Prop,
            [LabelText("角色")] Character,
            [LabelText("敌人")] Enemy,
            [LabelText("动作")] Action,
            [LabelText("装置")] Device
        }

        [TitleGroup("操作类型")] [EnumToggleButtons] [OnValueChanged("OnOperationTypeChanged")]
        public BatchOperationType operationType = BatchOperationType.Rename;

        [TitleGroup("目标选择")] [InfoBox("选择要操作的模板类型和条件")] [LabelText("模板类型")] [EnumToggleButtons]
        public TemplateTypeFilter targetType = TemplateTypeFilter.All;

        [LabelText("包含名称")] public string nameFilter = "";

        [LabelText("只处理缺失精灵的模板")] public bool onlyMissingSprites;

        [LabelText("只处理空描述的模板")] public bool onlyEmptyDescriptions;

        // 重命名操作设置
        [TitleGroup("重命名设置")] [ShowIf("@operationType == BatchOperationType.Rename")] [LabelText("查找文本")]
        public string findText = "";

        [ShowIf("@operationType == BatchOperationType.Rename")] [LabelText("替换文本")]
        public string replaceText = "";

        [ShowIf("@operationType == BatchOperationType.Rename")] [LabelText("添加前缀")]
        public string addPrefix = "";

        [ShowIf("@operationType == BatchOperationType.Rename")] [LabelText("添加后缀")]
        public string addSuffix = "";

        // 移动操作设置
        [TitleGroup("移动设置")] [ShowIf("@operationType == BatchOperationType.Move")] [LabelText("目标文件夹")] [FolderPath]
        public string targetFolder = "";

        [ShowIf("@operationType == BatchOperationType.Move")] [LabelText("保持原有结构")]
        public bool keepOriginalStructure = true;

        // 属性设置
        [TitleGroup("属性设置")] [ShowIf("@operationType == BatchOperationType.SetProperties")] [LabelText("设置稀有度")]
        public bool setRarity;

        [ShowIf("@operationType == BatchOperationType.SetProperties && setRarity")] [LabelText("稀有度值")]
        public Rarity.Rarity rarityValue = Rarity.Rarity.Common;

        [ShowIf("@operationType == BatchOperationType.SetProperties")] [LabelText("设置描述")]
        public bool setDescription;

        [ShowIf("@operationType == BatchOperationType.SetProperties && setDescription")]
        [LabelText("描述内容")]
        [TextArea(2, 4)]
        public string descriptionValue = "";

        [ShowIf("@operationType == BatchOperationType.SetProperties")] [LabelText("设置生命值")]
        public bool setHealth;

        [ShowIf("@operationType == BatchOperationType.SetProperties && setHealth")] [LabelText("生命值")] [Range(1, 100)]
        public int healthValue = 5;

        // 修复操作设置
        [TitleGroup("修复设置")] [ShowIf("@operationType == BatchOperationType.Repair")] [LabelText("自动分配默认精灵")]
        public bool assignDefaultSprites = true;

        [ShowIf("@operationType == BatchOperationType.Repair")] [LabelText("生成默认描述")]
        public bool generateDefaultDescriptions = true;

        [ShowIf("@operationType == BatchOperationType.Repair")] [LabelText("修复空名称")]
        public bool fixEmptyNames = true;

        // 预览和执行
        [TitleGroup("预览")]
        [ShowInInspector]
        [ReadOnly]
        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "name")]
        public List<ScriptableObject> affectedTemplates = new();

        private EnhancedUnifiedTemplateEditor parentWindow;

        [TitleGroup("操作")]
        [HorizontalGroup("操作/Buttons")]
        [Button("预览操作", ButtonSizes.Large)]
        private void PreviewOperation()
        {
            RefreshAffectedTemplates();
        }

        [HorizontalGroup("操作/Buttons")]
        [Button("执行操作", ButtonSizes.Large)]
        [EnableIf("CanExecuteOperation")]
        private void ExecuteOperation()
        {
            if (affectedTemplates.Count == 0)
            {
                EditorUtility.DisplayDialog("无操作目标", "没有找到符合条件的模板", "确定");
                return;
            }

            var confirmMessage = $"确定要对 {affectedTemplates.Count} 个模板执行 {GetOperationDisplayName()} 操作吗？";
            if (operationType == BatchOperationType.Delete) confirmMessage += "\n\n警告：删除操作不可撤销！";

            if (EditorUtility.DisplayDialog("确认操作", confirmMessage, "执行", "取消"))
                try
                {
                    ExecuteBatchOperation();
                    EditorUtility.DisplayDialog("操作完成", $"成功对 {affectedTemplates.Count} 个模板执行了操作", "确定");

                    // 刷新主窗口
                    if (parentWindow != null) parentWindow.ForceMenuTreeRebuild();
                }
                catch (Exception e)
                {
                    EditorUtility.DisplayDialog("操作失败", $"执行操作时发生错误：{e.Message}", "确定");
                }
        }

        [HorizontalGroup("操作/Buttons")]
        [Button("关闭", ButtonSizes.Large)]
        private void CloseWindow()
        {
            Close();
        }

        public static void ShowWindow(EnhancedUnifiedTemplateEditor parent)
        {
            var window = GetWindow<BatchOperationWindow>("批量操作", true);
            window.parentWindow = parent;
            window.minSize = new Vector2(500, 700);
            window.Show();
        }

        private void OnOperationTypeChanged()
        {
            RefreshAffectedTemplates();
        }

        private void RefreshAffectedTemplates()
        {
            affectedTemplates.Clear();

            var allTemplates = GetAllTemplates();
            var filteredTemplates = ApplyFilters(allTemplates);

            affectedTemplates.AddRange(filteredTemplates);
        }

        private List<ScriptableObject> GetAllTemplates()
        {
            var allTemplates = new List<ScriptableObject>();

            allTemplates.AddRange(FindTemplatesOfType<ItemTemplate>());
            allTemplates.AddRange(FindTemplatesOfType<CharacterTemplate>());
            allTemplates.AddRange(FindTemplatesOfType<EnemyTemplate>());
            allTemplates.AddRange(FindTemplatesOfType<ActionTemplate>());
            allTemplates.AddRange(FindTemplatesOfType<DeviceTemplate>());

            return allTemplates;
        }

        private List<T> FindTemplatesOfType<T>() where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            var templates = new List<T>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var template = AssetDatabase.LoadAssetAtPath<T>(path);
                if (template != null) templates.Add(template);
            }

            return templates;
        }

        private List<ScriptableObject> ApplyFilters(List<ScriptableObject> templates)
        {
            var filtered = templates.AsEnumerable();

            // 类型过滤
            if (targetType != TemplateTypeFilter.All) filtered = filtered.Where(t => MatchesTypeFilter(t, targetType));

            // 名称过滤
            if (!string.IsNullOrEmpty(nameFilter))
            {
                var nameLower = nameFilter.ToLower();
                filtered = filtered.Where(t => t.name.ToLower().Contains(nameLower));
            }

            // 缺失精灵过滤
            if (onlyMissingSprites) filtered = filtered.Where(HasMissingSprite);

            // 空描述过滤
            if (onlyEmptyDescriptions) filtered = filtered.Where(HasEmptyDescription);

            return filtered.ToList();
        }

        private bool MatchesTypeFilter(ScriptableObject template, TemplateTypeFilter filter)
        {
            return filter switch
            {
                TemplateTypeFilter.Equipment => template is ItemTemplate,
                TemplateTypeFilter.Character => template is CharacterTemplate,
                TemplateTypeFilter.Enemy => template is EnemyTemplate,
                TemplateTypeFilter.Action => template is ActionTemplate,
                TemplateTypeFilter.Device => template is DeviceTemplate,
                _ => true
            };
        }

        private bool HasMissingSprite(ScriptableObject template)
        {
            return template switch
            {
                ItemTemplate eq => eq.icon == null,
                CharacterTemplate character => character.characterSprite == null,
                EnemyTemplate enemy => enemy.enemySprite == null,
                ActionTemplate action => action.actionSprite == null,
                DeviceTemplate device => device.deviceSprite == null,
                _ => false
            };
        }

        private bool HasEmptyDescription(ScriptableObject template)
        {
            return template switch
            {
                ItemTemplate eq => string.IsNullOrEmpty(eq.description),
                _ => false
            };
        }

        private void ExecuteBatchOperation()
        {
            switch (operationType)
            {
                case BatchOperationType.Rename:
                    ExecuteRenameOperation();
                    break;
                case BatchOperationType.Move:
                    ExecuteMoveOperation();
                    break;
                case BatchOperationType.SetProperties:
                    ExecuteSetPropertiesOperation();
                    break;
                case BatchOperationType.Duplicate:
                    ExecuteDuplicateOperation();
                    break;
                case BatchOperationType.Delete:
                    ExecuteDeleteOperation();
                    break;
                case BatchOperationType.Repair:
                    ExecuteRepairOperation();
                    break;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void ExecuteRenameOperation()
        {
            foreach (var template in affectedTemplates)
            {
                var newName = template.name;

                // 查找替换
                if (!string.IsNullOrEmpty(findText) && !string.IsNullOrEmpty(replaceText))
                    newName = newName.Replace(findText, replaceText);

                // 添加前缀
                if (!string.IsNullOrEmpty(addPrefix)) newName = addPrefix + newName;

                // 添加后缀
                if (!string.IsNullOrEmpty(addSuffix)) newName = newName + addSuffix;

                if (newName != template.name)
                {
                    // 重命名资源文件
                    var assetPath = AssetDatabase.GetAssetPath(template);
                    var directory = Path.GetDirectoryName(assetPath);
                    var extension = Path.GetExtension(assetPath);
                    var newPath = Path.Combine(directory, newName + extension);

                    AssetDatabase.RenameAsset(assetPath, newName + extension);
                    template.name = newName;
                    EditorUtility.SetDirty(template);
                }
            }
        }

        private void ExecuteMoveOperation()
        {
            if (string.IsNullOrEmpty(targetFolder))
            {
                EditorUtility.DisplayDialog("错误", "请指定目标文件夹", "确定");
                return;
            }

            foreach (var template in affectedTemplates)
            {
                var currentPath = AssetDatabase.GetAssetPath(template);
                var fileName = Path.GetFileName(currentPath);
                var newPath = Path.Combine(targetFolder, fileName);

                if (AssetDatabase.MoveAsset(currentPath, newPath) != "")
                    Debug.LogWarning($"移动文件失败: {currentPath} -> {newPath}");
            }
        }

        private void ExecuteSetPropertiesOperation()
        {
            foreach (var template in affectedTemplates)
            {
                var modified = false;

                // 设置稀有度
                if (setRarity && template is ItemTemplate equipment)
                {
                    equipment.rarity = rarityValue;
                    modified = true;
                }

                // 设置描述
                if (setDescription)
                    switch (template)
                    {
                        case ItemTemplate eq:
                            eq.description = descriptionValue;
                            modified = true;
                            break;
                    }

                // 设置生命值
                if (setHealth)
                    switch (template)
                    {
                        case CharacterTemplate character:
                            character.baseHealth = healthValue;
                            modified = true;
                            break;
                        case EnemyTemplate enemy:
                            enemy.baseHealth = healthValue;
                            modified = true;
                            break;
                    }

                if (modified) EditorUtility.SetDirty(template);
            }
        }

        private void ExecuteDuplicateOperation()
        {
            foreach (var template in affectedTemplates)
            {
                var originalPath = AssetDatabase.GetAssetPath(template);
                var directory = Path.GetDirectoryName(originalPath);
                var fileName = Path.GetFileNameWithoutExtension(originalPath);
                var extension = Path.GetExtension(originalPath);

                var newPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{fileName} Copy{extension}");
                AssetDatabase.CopyAsset(originalPath, newPath);
            }
        }

        private void ExecuteDeleteOperation()
        {
            foreach (var template in affectedTemplates)
            {
                var assetPath = AssetDatabase.GetAssetPath(template);
                AssetDatabase.DeleteAsset(assetPath);
            }
        }

        private void ExecuteRepairOperation()
        {
            foreach (var template in affectedTemplates)
            {
                var modified = false;

                // 修复空名称
                if (fixEmptyNames && string.IsNullOrEmpty(template.name))
                {
                    template.name = $"New{template.GetType().Name}";
                    modified = true;
                }

                // 生成默认描述
                if (generateDefaultDescriptions)
                    switch (template)
                    {
                        case ItemTemplate eq when string.IsNullOrEmpty(eq.description):
                            eq.description = $"一个{eq.name}装备";
                            modified = true;
                            break;
                    }

                if (modified) EditorUtility.SetDirty(template);
            }
        }

        private string GetOperationDisplayName()
        {
            return operationType switch
            {
                BatchOperationType.Rename => "重命名",
                BatchOperationType.Move => "移动",
                BatchOperationType.SetProperties => "设置属性",
                BatchOperationType.Duplicate => "复制",
                BatchOperationType.Delete => "删除",
                BatchOperationType.Repair => "修复",
                _ => "未知操作"
            };
        }

        private bool CanExecuteOperation()
        {
            return affectedTemplates.Count > 0;
        }
    }
}
#endif