#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

public class PanelPrefabEditorWindow : OdinEditorWindow
{
    [MenuItem("Tools/UIM/Panel预制体编辑器")]
    private static void OpenWindow()
    {
        var window = GetWindow<PanelPrefabEditorWindow>();
        window.titleContent = new GUIContent("Panel预制体编辑器");
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(900, 700);
        window.minSize = new Vector2(900, 500);
        window.Show();
    }

    [BoxGroup("配置", centerLabel: true)]
    [FolderPath(RequireExistingPath = true)]
    [SerializeField, LabelText("搜索路径")]
    private string searchPath = "Assets/";

    [BoxGroup("操作", centerLabel: true, Order = -1)]
    [HorizontalGroup("操作/按钮组")]
    [Button("刷新Regedit列表", ButtonSizes.Medium)]
    [PropertyTooltip("查找指定路径下所有PanelRegeditSO文件")]
    private void RefreshList()
    {
        regeditFiles.Clear();
        string[] guids = AssetDatabase.FindAssets("t:PanelRegeditSO", new[] { searchPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PanelRegeditSO regedit = AssetDatabase.LoadAssetAtPath<PanelRegeditSO>(path);
            if (regedit != null) regeditFiles.Add(regedit);
        }

        regeditFiles = regeditFiles.OrderBy(x => x.name).ToList();
        EditorUtility.DisplayDialog("刷新完成", $"找到 {regeditFiles.Count} 个Regedit文件", "确定");
    }

    [HorizontalGroup("操作/按钮组")]
    [Button("生成预制体列表", ButtonSizes.Medium)]
    [PropertyTooltip("从所有Regedit生成预制体总表")]
    [EnableIf("@regeditFiles.Count > 0")]
    private void GeneratePrefabList()
    {
        prefabEntries.Clear();
        foreach (var regedit in regeditFiles)
        {
            if (regedit.PanelItemList == null) continue;

            foreach (var panelItem in regedit.PanelItemList)
            {
                if (panelItem.obj == null) continue;

                prefabEntries.Add(new PrefabEntry
                {
                    regeditName = regedit.name,
                    panelKey = panelItem.key,
                    prefab = panelItem.obj
                });
            }
        }

        prefabEntries = prefabEntries
            .OrderBy(x => x.regeditName)
            .ThenBy(x => x.panelKey)
            .ToList();
    }

    [BoxGroup("Regedit文件", centerLabel: true)]
    [ListDrawerSettings(
        NumberOfItemsPerPage = 10,
        DraggableItems = false,
        HideAddButton = true,
        HideRemoveButton = true,
        Expanded = false
    )]
    [SerializeField, LabelText("已找到的Regedit文件")]
    private List<PanelRegeditSO> regeditFiles = new List<PanelRegeditSO>();

    [BoxGroup("预制体管理", centerLabel: true)]
    [Searchable]
    [TableList(
        NumberOfItemsPerPage = 20,
        AlwaysExpanded = true,
        IsReadOnly = true,
        ShowIndexLabels = true
    )]
    [SerializeField, LabelText("预制体总览")]
    private List<PrefabEntry> prefabEntries = new List<PrefabEntry>();

    [System.Serializable]
    public class PrefabEntry
    {
        [TableColumnWidth(180, Resizable = false)]
        [DisplayAsString]
        public string regeditName;

        [TableColumnWidth(150, Resizable = false)]
        [DisplayAsString]
        public string panelKey;

        [TableColumnWidth(150)]
        [AssetsOnly]
        [PreviewField(Height = 30)]
        public GameObject prefab;

        [TableColumnWidth(120, Resizable = false)]
        [Button("编辑", ButtonSizes.Small)]
        private void EditPrefab()
        {
            if (prefab != null)
            {
                AssetDatabase.OpenAsset(prefab);
                GUIUtility.ExitGUI();
            }
        }

        [TableColumnWidth(120, Resizable = false)]
        [Button("场景创建", ButtonSizes.Small)]
        private void CreateInScene()
        {
            if (prefab != null)
            {
                Scene activeScene = EditorSceneManager.GetActiveScene();
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                Undo.RegisterCreatedObjectUndo(instance, $"Create {prefab.name}");
                EditorSceneManager.MarkSceneDirty(activeScene);
                Selection.activeObject = instance;
                SceneView.FrameLastActiveSceneView();
            }
        }
    }

    protected override void OnGUI()
    {
        // 顶部标题

        GUILayout.Space(5);

        // 主区域滚动视图
        this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos);
        {
            // 自动绘制所有标记的属性
            // this.DrawDefaultInspectorWithoutScriptField();
        }
        EditorGUILayout.EndScrollView();

        // 底部状态栏
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"状态: 找到 {regeditFiles.Count} 个Regedit文件 | {prefabEntries.Count} 个预制体", 
            EditorStyles.centeredGreyMiniLabel);
    }

    private Vector2 scrollPos;

    private void DrawDefaultInspectorWithoutScriptField()
    {
        var iterator = this.PropertyTree.RootProperty.Children.GetEnumerator();
        iterator.MoveNext(); // 跳过Script字段
        while (iterator.MoveNext())
        {
            iterator.Current.Draw();
        }
    }
}
#endif