#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HappyHotel.Action.Templates;
using HappyHotel.Character.Templates;
using HappyHotel.Device.Templates;
using HappyHotel.Enemy.Templates;
using HappyHotel.Equipment.Templates;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace HappyHotel.Core.Editor
{
    public class EnhancedUnifiedTemplateEditor : OdinMenuEditorWindow
    {
        public enum TemplateTypeFilter
        {
            [LabelText("全部")] All,
            [LabelText("装备")] Equipment,
            [LabelText("道具")] Prop,
            [LabelText("角色")] Character,
            [LabelText("敌人")] Enemy,
            [LabelText("动作")] Action,
            [LabelText("装置")] Device,
            [LabelText("商店道具")] ShopItem
        }

        // 工具栏配置
        [ShowInInspector]
        [PropertySpace(5)]
        [HorizontalGroup("Toolbar", Width = 0.7f)]
        [LabelText("搜索")]
        [LabelWidth(40)]
        [OnValueChanged("UpdateMenuTree")]
        public string searchFilter = "";

        [HorizontalGroup("Toolbar")] [LabelText("显示路径")] [LabelWidth(60)] [OnValueChanged("UpdateMenuTree")]
        public bool showFullPath;

        [HorizontalGroup("Toolbar")] [LabelText("按类型分组")] [LabelWidth(70)] [OnValueChanged("UpdateMenuTree")]
        public bool groupByType = true;

        [HorizontalGroup("Toolbar")] [LabelText("显示图标")] [LabelWidth(60)] [OnValueChanged("UpdateMenuTree")]
        public bool showIcons = true;

        // 过滤器配置
        [PropertySpace(5)]
        [FoldoutGroup("高级过滤器")]
        [LabelText("模板类型过滤")]
        [EnumToggleButtons]
        [OnValueChanged("UpdateMenuTree")]
        public TemplateTypeFilter typeFilter = TemplateTypeFilter.All;

        [FoldoutGroup("高级过滤器")] [LabelText("只显示缺失精灵的模板")] [OnValueChanged("UpdateMenuTree")]
        public bool showOnlyMissingSprites;

        [FoldoutGroup("高级过滤器")] [LabelText("只显示空描述的模板")] [OnValueChanged("UpdateMenuTree")]
        public bool showOnlyEmptyDescriptions;

        // 统计信息
        [PropertySpace(10)] [FoldoutGroup("统计信息")] [ShowInInspector] [ReadOnly] [LabelText("总模板数")]
        public int totalTemplateCount;

        [FoldoutGroup("统计信息")] [ShowInInspector] [ReadOnly] [LabelText("当前显示数")]
        public int filteredTemplateCount;

        [FoldoutGroup("统计信息")] [ShowInInspector] [ReadOnly] [LabelText("缺失精灵数")]
        public int missingSpritesCount;

        [FoldoutGroup("统计信息")] [ShowInInspector] [ReadOnly] [LabelText("空描述数")]
        public int emptyDescriptionsCount;

        [FoldoutGroup("高级过滤器")]
        [LabelText("稀有度过滤")]
        [ShowIf(
            "@typeFilter == TemplateTypeFilter.Equipment || typeFilter == TemplateTypeFilter.ShopItem || typeFilter == TemplateTypeFilter.All")]
        [OnValueChanged("UpdateMenuTree")]
        public Rarity.Rarity? rarityFilter;

        [MenuItem("Happy Hotel/Enhanced Template Editor")]
        private static void OpenWindow()
        {
            var window = GetWindow<EnhancedUnifiedTemplateEditor>("增强模板编辑器");
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1200, 800);
            window.Show();
        }

        // 快速操作按钮
        [PropertySpace(10)]
        [HorizontalGroup("QuickActions")]
        [Button("刷新", ButtonSizes.Medium)]
        private void RefreshTemplates()
        {
            ForceMenuTreeRebuild();
        }

        [HorizontalGroup("QuickActions")]
        [Button("创建模板", ButtonSizes.Medium)]
        public void ShowCreateDialog()
        {
            QuickCreateTemplateWindow.ShowWindow(this);
        }

        [HorizontalGroup("QuickActions")]
        [Button("批量操作", ButtonSizes.Medium)]
        public void ShowBatchDialog()
        {
            BatchOperationWindow.ShowWindow(this);
        }

        [HorizontalGroup("QuickActions")]
        [Button("导出报告", ButtonSizes.Medium)]
        public void ExportReport()
        {
            ExportAnalysisReport();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(true);

            tree.Add("功能/创建新模板", new CreateTemplateAction(), EditorIcons.Plus);
            tree.Add("功能/批量操作", new BatchOperationAction(), EditorIcons.List);
            tree.Add("功能/统计分析", new AnalysisAction(), EditorIcons.Info);

            tree.Add("", null); // 分隔符

            // 获取所有模板并应用过滤器
            var allTemplates = GetAllTemplates();
            var filteredTemplates = ApplyFilters(allTemplates);

            // 更新统计信息
            UpdateStatistics(allTemplates, filteredTemplates);

            if (groupByType)
                AddTemplatesByTypeToTree(tree, filteredTemplates);
            else
                AddTemplatesFlat(tree, filteredTemplates);

            tree.Selection.SupportsMultiSelect = true;
            tree.Config.DrawSearchToolbar = false;

            return tree;
        }

        private void AddTemplatesByTypeToTree(OdinMenuTree tree, List<ScriptableObject> templates)
        {
            var groups = templates.GroupBy(GetTemplateCategory).OrderBy(g => g.Key);

            foreach (var group in groups)
            {
                var categoryName = group.Key;
                var categoryTemplates = group.OrderBy(t => t.name).ToList();

                foreach (var template in categoryTemplates)
                {
                    var displayName = GetDisplayName(template);
                    var icon = showIcons ? GetTemplateIcon(template) : null;
                    var menuPath = $"{categoryName}/{displayName}";

                    tree.Add(menuPath, template, icon);
                }
            }
        }

        private void AddTemplatesFlat(OdinMenuTree tree, List<ScriptableObject> templates)
        {
            foreach (var template in templates.OrderBy(t => t.name))
            {
                var displayName = GetDisplayName(template);
                var icon = showIcons ? GetTemplateIcon(template) : null;
                tree.Add(displayName, template, icon);
            }
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

            // 搜索过滤
            if (!string.IsNullOrEmpty(searchFilter))
            {
                var searchLower = searchFilter.ToLower();
                filtered = filtered.Where(t =>
                    t.name.ToLower().Contains(searchLower) ||
                    AssetDatabase.GetAssetPath(t).ToLower().Contains(searchLower));
            }

            // 类型过滤
            if (typeFilter != TemplateTypeFilter.All) filtered = filtered.Where(t => MatchesTypeFilter(t, typeFilter));

            // 稀有度过滤
            if (rarityFilter.HasValue)
                filtered = filtered.Where(t =>
                    t is ItemTemplate eq && eq.rarity == rarityFilter.Value);

            // 缺失精灵过滤
            if (showOnlyMissingSprites) filtered = filtered.Where(HasMissingSprite);

            // 空描述过滤
            if (showOnlyEmptyDescriptions) filtered = filtered.Where(HasEmptyDescription);

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

        private void UpdateStatistics(List<ScriptableObject> allTemplates, List<ScriptableObject> filteredTemplates)
        {
            totalTemplateCount = allTemplates.Count;
            filteredTemplateCount = filteredTemplates.Count;
            missingSpritesCount = allTemplates.Count(HasMissingSprite);
            emptyDescriptionsCount = allTemplates.Count(HasEmptyDescription);
        }

        private string GetTemplateCategory(ScriptableObject template)
        {
            return template switch
            {
                ItemTemplate => "装备模板",
                CharacterTemplate => "角色模板",
                EnemyTemplate => "敌人模板",
                ActionTemplate => "动作模板",
                DeviceTemplate => "装置模板",
                _ => "其他模板"
            };
        }

        private string GetDisplayName(ScriptableObject template)
        {
            var name = template.name;

            if (showFullPath)
            {
                var path = AssetDatabase.GetAssetPath(template);
                name += $" ({path})";
            }

            // 添加状态指示器
            var indicators = new List<string>();
            if (HasMissingSprite(template)) indicators.Add("🚫");
            if (HasEmptyDescription(template)) indicators.Add("📝");

            if (indicators.Count > 0) name += $" {string.Join("", indicators)}";

            return name;
        }

        private EditorIcon GetTemplateIcon(ScriptableObject template)
        {
            return template switch
            {
                _ => EditorIcons.File
            };
        }

        protected override void OnBeginDrawEditors()
        {
            var selected = MenuTree.Selection.FirstOrDefault();
            var toolbarHeight = MenuTree.Config.SearchToolbarHeight;

            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                DrawEditorToolbar(selected);
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        private void DrawEditorToolbar(OdinMenuItem selected)
        {
            if (selected?.Value is ScriptableObject template)
            {
                // 单选状态
                GUILayout.Label($"编辑: {template.name}", SirenixGUIStyles.BoldLabel);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("复制", EditorStyles.toolbarButton, GUILayout.Width(40))) // 复制
                    DuplicateTemplate(template);

                if (GUILayout.Button("删除", EditorStyles.toolbarButton, GUILayout.Width(40))) // 删除
                    DeleteTemplate(template);

                if (GUILayout.Button("定位", EditorStyles.toolbarButton, GUILayout.Width(40))) // 定位
                {
                    EditorGUIUtility.PingObject(template);
                    Selection.activeObject = template;
                }

                if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(40))) // 刷新
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(template));
            }
            else if (MenuTree.Selection.Count > 1)
            {
                // 多选状态
                var selectedTemplates = GetSelectedTemplates();
                GUILayout.Label($"已选中 {selectedTemplates.Count} 个模板", SirenixGUIStyles.BoldLabel);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("批量复制", EditorStyles.toolbarButton, GUILayout.Width(60))) // 批量复制
                    BatchDuplicateTemplates(selectedTemplates);

                if (GUILayout.Button("批量删除", EditorStyles.toolbarButton, GUILayout.Width(60))) // 批量删除
                    BatchDeleteTemplates(selectedTemplates);

                if (GUILayout.Button("批量编辑", EditorStyles.toolbarButton, GUILayout.Width(60))) // 批量编辑
                    ShowBatchDialog();
            }
            else if (selected?.Value is ITemplateAction action)
            {
                // 功能菜单项
                GUILayout.Label($"功能: {action.GetDisplayName()}", SirenixGUIStyles.BoldLabel);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("执行", EditorStyles.toolbarButton, GUILayout.Width(40))) action.Execute(this);
            }
            else
            {
                GUILayout.Label("Happy Hotel 增强模板编辑器", SirenixGUIStyles.BoldLabel);
                GUILayout.FlexibleSpace();

                GUILayout.Label($"显示 {filteredTemplateCount}/{totalTemplateCount} 个模板", EditorStyles.miniLabel);
            }
        }

        protected override void OnEndDrawEditors()
        {
            var selected = MenuTree.Selection.FirstOrDefault();

            if (selected?.Value is ITemplateAction action)
                DrawActionPanel(action);
            else if (selected?.Value == null) DrawWelcomePanel();
        }

        private void DrawActionPanel(ITemplateAction action)
        {
            GUILayout.BeginVertical();
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical(GUILayout.MaxWidth(400));

            GUILayout.Label(action.GetDisplayName(), SirenixGUIStyles.Title);
            GUILayout.Space(10);
            GUILayout.Label(action.GetDescription(), EditorStyles.wordWrappedLabel);
            GUILayout.Space(20);

            if (GUILayout.Button($"执行 {action.GetDisplayName()}", GUILayout.Height(40))) action.Execute(this);

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void DrawWelcomePanel()
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical(GUILayout.MaxWidth(500));

            GUILayout.Label("欢迎使用增强模板编辑器", SirenixGUIStyles.Title);
            GUILayout.Space(15);

            GUILayout.Label("功能特色:", SirenixGUIStyles.BoldLabel);
            GUILayout.Label("• 统一管理所有类型的模板文件");
            GUILayout.Label("• 强大的搜索和过滤功能");
            GUILayout.Label("• 支持批量操作和多选编辑");
            GUILayout.Label("• 实时统计和状态指示");
            GUILayout.Label("• 直观的树形结构显示");

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("创建新模板", GUILayout.Height(35))) ShowCreateDialog();
            if (GUILayout.Button("查看统计", GUILayout.Height(35)))
            {
                // 展开统计信息折叠组
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        // 模板操作方法
        private List<ScriptableObject> GetSelectedTemplates()
        {
            return MenuTree.Selection
                .Where(item => item.Value is ScriptableObject)
                .Select(item => item.Value as ScriptableObject)
                .ToList();
        }

        private void DuplicateTemplate(ScriptableObject template)
        {
            var originalPath = AssetDatabase.GetAssetPath(template);
            var directory = Path.GetDirectoryName(originalPath);
            var fileName = Path.GetFileNameWithoutExtension(originalPath);
            var extension = Path.GetExtension(originalPath);

            var newPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{fileName} Copy{extension}");

            if (AssetDatabase.CopyAsset(originalPath, newPath))
            {
                AssetDatabase.Refresh();
                ForceMenuTreeRebuild();

                var newTemplate = AssetDatabase.LoadAssetAtPath<ScriptableObject>(newPath);
                Selection.activeObject = newTemplate;
                EditorGUIUtility.PingObject(newTemplate);
            }
        }

        private void DeleteTemplate(ScriptableObject template)
        {
            if (EditorUtility.DisplayDialog("确认删除",
                    $"确定要删除模板 '{template.name}' 吗？此操作不可撤销。",
                    "删除", "取消"))
            {
                var assetPath = AssetDatabase.GetAssetPath(template);
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.Refresh();
                ForceMenuTreeRebuild();
            }
        }

        private void BatchDuplicateTemplates(List<ScriptableObject> templates)
        {
            foreach (var template in templates) DuplicateTemplate(template);
        }

        private void BatchDeleteTemplates(List<ScriptableObject> templates)
        {
            if (EditorUtility.DisplayDialog("确认批量删除",
                    $"确定要删除 {templates.Count} 个选中的模板吗？此操作不可撤销。",
                    "删除", "取消"))
            {
                foreach (var template in templates)
                {
                    var assetPath = AssetDatabase.GetAssetPath(template);
                    AssetDatabase.DeleteAsset(assetPath);
                }

                AssetDatabase.Refresh();
                ForceMenuTreeRebuild();
            }
        }

        private void ExportAnalysisReport()
        {
            var allTemplates = GetAllTemplates();
            var report = GenerateAnalysisReport(allTemplates);

            var path = EditorUtility.SaveFilePanel("导出分析报告", "", "TemplateAnalysisReport", "txt");
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, report);
                EditorUtility.DisplayDialog("导出成功", $"报告已导出到：{path}", "确定");
            }
        }

        private string GenerateAnalysisReport(List<ScriptableObject> templates)
        {
            var report = new StringBuilder();
            report.AppendLine("=== Happy Hotel 模板分析报告 ===");
            report.AppendLine($"生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();

            // 基本统计
            report.AppendLine("=== 基本统计 ===");
            report.AppendLine($"总模板数: {templates.Count}");
            report.AppendLine($"装备模板: {templates.OfType<ItemTemplate>().Count()}");
            report.AppendLine($"角色模板: {templates.OfType<CharacterTemplate>().Count()}");
            report.AppendLine($"敌人模板: {templates.OfType<EnemyTemplate>().Count()}");
            report.AppendLine($"动作模板: {templates.OfType<ActionTemplate>().Count()}");
            report.AppendLine($"装置模板: {templates.OfType<DeviceTemplate>().Count()}");
            report.AppendLine();

            // 问题统计
            report.AppendLine("=== 问题统计 ===");
            report.AppendLine($"缺失精灵: {templates.Count(HasMissingSprite)}");
            report.AppendLine($"空描述: {templates.Count(HasEmptyDescription)}");

            return report.ToString();
        }

        private void UpdateMenuTree()
        {
            ForceMenuTreeRebuild();
        }
    }

    // 功能操作接口和实现
    public interface ITemplateAction
    {
        string GetDisplayName();
        string GetDescription();
        void Execute(EnhancedUnifiedTemplateEditor editor);
    }

    public class CreateTemplateAction : ITemplateAction
    {
        public string GetDisplayName()
        {
            return "创建新模板";
        }

        public string GetDescription()
        {
            return "打开模板创建向导，快速创建各种类型的模板文件。";
        }

        public void Execute(EnhancedUnifiedTemplateEditor editor)
        {
            editor.ShowCreateDialog();
        }
    }

    public class BatchOperationAction : ITemplateAction
    {
        public string GetDisplayName()
        {
            return "批量操作";
        }

        public string GetDescription()
        {
            return "对多个模板执行批量操作，如重命名、移动、属性设置等。";
        }

        public void Execute(EnhancedUnifiedTemplateEditor editor)
        {
            editor.ShowBatchDialog();
        }
    }

    public class AnalysisAction : ITemplateAction
    {
        public string GetDisplayName()
        {
            return "统计分析";
        }

        public string GetDescription()
        {
            return "分析项目中所有模板的统计信息，检测潜在问题。";
        }

        public void Execute(EnhancedUnifiedTemplateEditor editor)
        {
            editor.ExportReport();
        }
    }
}
#endif