#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Linq;

[CustomEditor(typeof(UIM_PanelsManager))]
public class UIM_PanelsManagerEditor : OdinEditor
{
    private UIM_PanelsManager manager;
    private bool showPanelObjects = true;
    private Vector2 scrollPos;

    protected override void OnEnable()
    {
        base.OnEnable();
        manager = (UIM_PanelsManager)target;
    }

    public override void OnInspectorGUI()
    {
        // 绘制默认的Odin属性
        base.OnInspectorGUI();

        EditorGUILayout.Space(10);
        
        // 添加一个按钮来切换面板显示
        showPanelObjects = EditorGUILayout.Foldout(showPanelObjects, "面板对象管理", true);
        
        if (showPanelObjects && manager.regeditSO != null)
        {
            EditorGUILayout.HelpBox("在这里可以快速查看和管理所有面板对象", MessageType.Info);
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
            
            // 显示所有面板对象的状态
            foreach (var panelItem in manager.regeditSO.PanelItemList)
            {
                EditorGUILayout.BeginHorizontal();
                
                // 显示面板键名
                EditorGUILayout.LabelField(panelItem.key, GUILayout.Width(150));
                
                // 显示当前对象状态
                bool isActive = panelItem.currentObj != null && panelItem.currentObj.activeSelf;
                GUI.enabled = panelItem.currentObj != null;
                bool newActive = EditorGUILayout.Toggle("激活状态", isActive);
                GUI.enabled = true;
                
                if (panelItem.currentObj != null && isActive != newActive)
                {
                    panelItem.currentObj.SetActive(newActive);
                    EditorUtility.SetDirty(panelItem.currentObj);
                }
                
                // 创建/删除按钮
                if (panelItem.currentObj == null)
                {
                    if (GUILayout.Button("创建", GUILayout.Width(60)))
                    {
                        CreatePanelObject(panelItem);
                    }
                }
                else
                {
                    if (GUILayout.Button("删除", GUILayout.Width(60)))
                    {
                        DestroyImmediate(panelItem.currentObj);
                        panelItem.currentObj = null;
                    }
                    
                    // 定位按钮
                    if (GUILayout.Button("定位", GUILayout.Width(60)))
                    {
                        Selection.activeObject = panelItem.currentObj;
                        EditorGUIUtility.PingObject(panelItem.currentObj);
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            
            // 批量操作按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("创建所有面板"))
            {
                CreateAllPanels();
            }
            if (GUILayout.Button("删除所有面板"))
            {
                DeleteAllPanels();
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void CreatePanelObject(PanelItem panelItem)
    {
        if (panelItem.obj == null)
        {
            Debug.LogWarning($"无法创建面板 {panelItem.key}，因为预制体未设置");
            return;
        }
        
        panelItem.currentObj = (GameObject)PrefabUtility.InstantiatePrefab(panelItem.obj, manager.transform);
        panelItem.currentObj.name = panelItem.key;
        panelItem.currentObj.SetActive(true);
        
        EditorUtility.SetDirty(manager.regeditSO);
        EditorUtility.SetDirty(manager);
    }

    private void CreateAllPanels()
    {
        foreach (var panelItem in manager.regeditSO.PanelItemList)
        {
            if (panelItem.currentObj == null && panelItem.obj != null)
            {
                CreatePanelObject(panelItem);
            }
        }
    }

    private void DeleteAllPanels()
    {
        foreach (var panelItem in manager.regeditSO.PanelItemList)
        {
            if (panelItem.currentObj != null)
            {
                DestroyImmediate(panelItem.currentObj);
                panelItem.currentObj = null;
            }
        }
        
        EditorUtility.SetDirty(manager.regeditSO);
        EditorUtility.SetDirty(manager);
    }
}

[CustomEditor(typeof(PanelRegeditSO))]
public class PanelRegeditSOEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        PanelRegeditSO regedit = (PanelRegeditSO)target;
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("查找场景中的面板对象"))
        {
            FindScenePanels(regedit);
        }
    }

    private void FindScenePanels(PanelRegeditSO regedit)
    {
        foreach (var panelItem in regedit.PanelItemList)
        {
            // 查找场景中是否有同名的对象
            GameObject sceneObj = GameObject.Find(panelItem.key);
            if (sceneObj != null)
            {
                panelItem.currentObj = sceneObj;
                EditorUtility.SetDirty(regedit);
            }
        }
    }
}
#endif