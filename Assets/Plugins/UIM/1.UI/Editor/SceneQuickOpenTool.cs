using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using System.Linq;

public class EnhancedSceneQuickOpenTool : EditorWindow
{
    private Vector2 scrollPos;
    private List<string> scenePaths = new List<string>();
    private string searchFilter = "";
    private Dictionary<string, bool> folderExpandStates = new Dictionary<string, bool>();
    private List<string> favoriteScenes = new List<string>();
    private const string FAVORITES_KEY = "SceneQuickOpen_Favorites";
    
    [MenuItem("Tools/UIM/Scene Quick Open")]
    public static void ShowWindow()
    {
        var window = GetWindow<EnhancedSceneQuickOpenTool>("场景快速打开");
        window.minSize = new Vector2(400, 500);
        window.LoadFavorites();
    }

    private void OnEnable()
    {
        RefreshSceneList();
        LoadFavorites();
    }

    private void LoadFavorites()
    {
        string favorites = EditorPrefs.GetString(FAVORITES_KEY, "");
        favoriteScenes = favorites.Split(new[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    private void SaveFavorites()
    {
        EditorPrefs.SetString(FAVORITES_KEY, string.Join("|", favoriteScenes));
    }

    private void RefreshSceneList()
    {
        scenePaths.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Scene");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            scenePaths.Add(path);
        }
        scenePaths.Sort();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        
        // 搜索框
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("搜索:", GUILayout.Width(40));
        searchFilter = EditorGUILayout.TextField(searchFilter);
        if (GUILayout.Button("刷新", GUILayout.Width(60)))
        {
            RefreshSceneList();
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        // 收藏场景
        if (favoriteScenes.Count > 0)
        {
            EditorGUILayout.LabelField("收藏场景", EditorStyles.boldLabel);
            DrawSceneList(favoriteScenes, true);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("所有场景", EditorStyles.boldLabel);
        }
        
        // 按路径分组
        var groupedScenes = scenePaths
            .Where(p => string.IsNullOrEmpty(searchFilter) || 
                   Path.GetFileNameWithoutExtension(p).ToLower().Contains(searchFilter.ToLower()))
            .GroupBy(p => Path.GetDirectoryName(p))
            .OrderBy(g => g.Key);
        
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        foreach (var group in groupedScenes)
        {
            string folderPath = group.Key;
            string folderName = Path.GetFileName(folderPath);
            
            if (!folderExpandStates.ContainsKey(folderPath))
            {
                folderExpandStates[folderPath] = false;
            }
            
            // 文件夹折叠标题
            EditorGUILayout.BeginHorizontal();
            folderExpandStates[folderPath] = EditorGUILayout.Foldout(folderExpandStates[folderPath], $"{folderName} ({group.Count()})", true);
            EditorGUILayout.LabelField(folderPath, EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            
            // 文件夹内容
            if (folderExpandStates[folderPath])
            {
                EditorGUI.indentLevel++;
                foreach (var scenePath in group)
                {
                    DrawSceneItem(scenePath);
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void DrawSceneList(List<string> scenes, bool isFavorites = false)
    {
        foreach (var scenePath in scenes)
        {
            if (!File.Exists(scenePath)) continue;
            
            DrawSceneItem(scenePath, isFavorites);
        }
    }

    private void DrawSceneItem(string scenePath, bool isFavoriteItem = false)
    {
        string sceneName = Path.GetFileNameWithoutExtension(scenePath);
        
        EditorGUILayout.BeginHorizontal();
        
        // 收藏按钮
        bool isFavorite = favoriteScenes.Contains(scenePath);
        bool newFavoriteState = GUILayout.Toggle(isFavorite, "", GUILayout.Width(20));
        if (newFavoriteState != isFavorite)
        {
            if (newFavoriteState)
            {
                favoriteScenes.Add(scenePath);
            }
            else
            {
                favoriteScenes.Remove(scenePath);
            }
            SaveFavorites();
        }
        
        // 场景按钮
        if (GUILayout.Button(sceneName, EditorStyles.miniButton, GUILayout.Width(200)))
        {
            OpenScene(scenePath);
        }
        
        // 路径标签
        GUILayout.Label(Path.GetDirectoryName(scenePath), EditorStyles.miniLabel);
        
        EditorGUILayout.EndHorizontal();
    }

    private void OpenScene(string path)
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            if (EditorUtility.DisplayDialog("场景已修改", 
                "是否保存当前场景?", "保存", "不保存"))
            {
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }
        }
        
        EditorSceneManager.OpenScene(path);
    }
}