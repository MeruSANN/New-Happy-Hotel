#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HappyHotel.Action.Templates;
using HappyHotel.Character.Templates;
using HappyHotel.Core.Rarity;
using HappyHotel.Device.Templates;
using HappyHotel.Enemy.Templates;
using HappyHotel.Equipment.Templates;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace HappyHotel.Core.Editor
{
    public class TemplateAnalyzerWindow : OdinEditorWindow
    {
        [ShowInInspector] [ReadOnly] private string emptyDescriptionsInfo = "";

        [ShowInInspector] [ReadOnly] private string missingSpritesInfo = "";

        [TitleGroup("装备稀有度分布")] [ShowInInspector] [ReadOnly] [TableList(ShowIndexLabels = false)]
        private List<RarityStatistics> rarityStatistics = new();

        [TitleGroup("总体统计")] [ShowInInspector] [ReadOnly] [TableList(ShowIndexLabels = false)]
        private List<TemplateStatistics> templateStatistics = new();

        [TitleGroup("详细信息")] [ShowInInspector] [ReadOnly] [PropertySpace(10)]
        private string totalTemplatesInfo = "";

        protected override void OnEnable()
        {
            base.OnEnable();
            titleContent = new GUIContent("模板分析器");
            RefreshStatistics();
        }

        [MenuItem("Happy Hotel/Template Analyzer")]
        private static void OpenWindow()
        {
            GetWindow<TemplateAnalyzerWindow>("模板分析器").Show();
        }

        [TitleGroup("操作")]
        [Button("刷新统计", ButtonSizes.Large)]
        private void RefreshStatistics()
        {
            AnalyzeTemplates();
        }

        [Button("导出报告", ButtonSizes.Large)]
        private void ExportReport()
        {
            var report = GenerateReport();
            var path = EditorUtility.SaveFilePanel("导出模板分析报告", "", "TemplateAnalysisReport", "txt");
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, report);
                EditorUtility.DisplayDialog("导出成功", $"报告已导出到：{path}", "确定");
            }
        }

        [Button("修复缺失精灵", ButtonSizes.Large)]
        private void FixMissingSprites()
        {
            var fixedCount = 0;

            // 查找所有模板
            var allTemplates = FindAllTemplates();

            foreach (var template in allTemplates)
            {
                var needsSave = false;

                switch (template)
                {
                    case ItemTemplate equipTemplate when equipTemplate.icon == null:
                        // 可以设置默认图标或提示用户
                        Debug.LogWarning($"装备模板 '{equipTemplate.name}' 缺少图标");
                        break;
                    case CharacterTemplate charTemplate when charTemplate.characterSprite == null:
                        Debug.LogWarning($"角色模板 '{charTemplate.name}' 缺少精灵");
                        break;
                    case EnemyTemplate enemyTemplate when enemyTemplate.enemySprite == null:
                        Debug.LogWarning($"敌人模板 '{enemyTemplate.name}' 缺少精灵");
                        break;
                    case ActionTemplate actionTemplate when actionTemplate.actionSprite == null:
                        Debug.LogWarning($"动作模板 '{actionTemplate.name}' 缺少精灵");
                        break;
                    case DeviceTemplate deviceTemplate when deviceTemplate.deviceSprite == null:
                        Debug.LogWarning($"装置模板 '{deviceTemplate.name}' 缺少精灵");
                        break;
                }

                if (needsSave)
                {
                    EditorUtility.SetDirty(template);
                    fixedCount++;
                }
            }

            if (fixedCount > 0)
            {
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("修复完成", $"已修复 {fixedCount} 个模板的缺失精灵问题", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("无需修复", "没有发现需要修复的缺失精灵问题", "确定");
            }

            RefreshStatistics();
        }

        private void AnalyzeTemplates()
        {
            templateStatistics.Clear();
            rarityStatistics.Clear();

            // 分析各类型模板
            AnalyzeTemplateType<ItemTemplate>("装备模板", "Assets/Happy Hotel/Equipment/Resources/Templates");
            AnalyzeTemplateType<CharacterTemplate>("角色模板", "Assets/Happy Hotel/Character/Resources/Templates");
            AnalyzeTemplateType<EnemyTemplate>("敌人模板", "Assets/Happy Hotel/Enemy/Resources/Templates");
            AnalyzeTemplateType<ActionTemplate>("动作模板", "Assets/Happy Hotel/Action/Resources/Templates");
            AnalyzeTemplateType<DeviceTemplate>("装置模板", "Assets/Happy Hotel/Device/Resources/Templates");

            // 分析稀有度分布
            AnalyzeRarityDistribution();

            // 生成详细信息
            GenerateDetailedInfo();
        }

        private void AnalyzeTemplateType<T>(string typeName, string folderPath) where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            var templates = new List<T>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var template = AssetDatabase.LoadAssetAtPath<T>(path);
                if (template != null) templates.Add(template);
            }

            var stats = new TemplateStatistics
            {
                templateType = typeName,
                count = templates.Count,
                folderPath = folderPath,
                templateNames = templates.Select(t => t.name).OrderBy(name => name).ToList()
            };

            templateStatistics.Add(stats);
        }

        private void AnalyzeRarityDistribution()
        {
            var equipmentTemplates = FindTemplatesOfType<ItemTemplate>();
            var rarityGroups = equipmentTemplates.GroupBy(t => t.rarity).ToList();

            var totalEquipment = equipmentTemplates.Count;

            foreach (Rarity.Rarity rarity in Enum.GetValues(typeof(Rarity.Rarity)))
            {
                var group = rarityGroups.FirstOrDefault(g => g.Key == rarity);
                var count = group?.Count() ?? 0;
                var percentage = totalEquipment > 0 ? count * 100f / totalEquipment : 0f;

                rarityStatistics.Add(new RarityStatistics
                {
                    rarity = rarity,
                    count = count,
                    percentage = percentage
                });
            }
        }

        private void GenerateDetailedInfo()
        {
            var totalTemplates = templateStatistics.Sum(s => s.count);
            totalTemplatesInfo = $"项目中共有 {totalTemplates} 个模板文件";

            // 检查缺失精灵
            var missingSprites = new List<string>();
            var allTemplates = FindAllTemplates();

            foreach (var template in allTemplates)
            {
                var hasMissingSprite = template switch
                {
                    ItemTemplate equipTemplate => equipTemplate.icon == null,
                    CharacterTemplate charTemplate => charTemplate.characterSprite == null,
                    EnemyTemplate enemyTemplate => enemyTemplate.enemySprite == null,
                    ActionTemplate actionTemplate => actionTemplate.actionSprite == null,
                    DeviceTemplate deviceTemplate => deviceTemplate.deviceSprite == null,
                    _ => false
                };

                if (hasMissingSprite) missingSprites.Add($"{template.GetType().Name}: {template.name}");
            }

            missingSpritesInfo = missingSprites.Count > 0
                ? $"发现 {missingSprites.Count} 个模板缺少精灵：\n" + string.Join("\n", missingSprites)
                : "所有模板都有对应的精灵";

            // 检查空描述
            var emptyDescriptions = new List<string>();

            foreach (var template in allTemplates)
            {
                var hasEmptyDescription = template switch
                {
                    ItemTemplate equipTemplate => string.IsNullOrEmpty(equipTemplate.description),
                    _ => false
                };

                if (hasEmptyDescription) emptyDescriptions.Add($"{template.GetType().Name}: {template.name}");
            }

            emptyDescriptionsInfo = emptyDescriptions.Count > 0
                ? $"发现 {emptyDescriptions.Count} 个模板缺少描述：\n" + string.Join("\n", emptyDescriptions)
                : "所有需要描述的模板都有描述";
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

        private List<ScriptableObject> FindAllTemplates()
        {
            var allTemplates = new List<ScriptableObject>();

            allTemplates.AddRange(FindTemplatesOfType<ItemTemplate>());
            allTemplates.AddRange(FindTemplatesOfType<CharacterTemplate>());
            allTemplates.AddRange(FindTemplatesOfType<EnemyTemplate>());
            allTemplates.AddRange(FindTemplatesOfType<ActionTemplate>());
            allTemplates.AddRange(FindTemplatesOfType<DeviceTemplate>());

            return allTemplates;
        }

        private string GenerateReport()
        {
            var report = new StringBuilder();
            report.AppendLine("=== Happy Hotel 模板分析报告 ===");
            report.AppendLine($"生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();

            report.AppendLine("=== 总体统计 ===");
            foreach (var stat in templateStatistics)
            {
                report.AppendLine($"{stat.templateType}: {stat.count} 个");
                report.AppendLine($"  路径: {stat.folderPath}");
                if (stat.templateNames.Count > 0) report.AppendLine($"  模板列表: {string.Join(", ", stat.templateNames)}");
                report.AppendLine();
            }

            report.AppendLine("=== 装备稀有度分布 ===");
            foreach (var rarity in rarityStatistics)
                report.AppendLine($"{rarity.rarity}: {rarity.count} 个 ({rarity.percentage:F1}%)");
            report.AppendLine();

            report.AppendLine("=== 详细信息 ===");
            report.AppendLine(totalTemplatesInfo);
            report.AppendLine();
            report.AppendLine("缺失精灵:");
            report.AppendLine(missingSpritesInfo);
            report.AppendLine();
            report.AppendLine("空描述:");
            report.AppendLine(emptyDescriptionsInfo);

            return report.ToString();
        }

        [Serializable]
        public class TemplateStatistics
        {
            [LabelText("模板类型")] public string templateType;

            [LabelText("数量")] public int count;

            [LabelText("文件夹路径")] public string folderPath;

            [LabelText("模板列表")] [ListDrawerSettings(ShowIndexLabels = false, DraggableItems = false)]
            public List<string> templateNames = new();
        }

        [Serializable]
        public class RarityStatistics
        {
            [LabelText("稀有度")] public Rarity.Rarity rarity;

            [LabelText("数量")] public int count;

            [LabelText("百分比")] [ProgressBar(0, 100, ColorGetter = "GetRarityColor")]
            public float percentage;

            private Color GetRarityColor()
            {
                return RarityColorManager.GetRarityColor(rarity);
            }
        }
    }
}
#endif