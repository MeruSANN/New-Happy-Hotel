using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Animations;
using System.Collections.Generic;

public class AnimatorOverrideCreator : EditorWindow
{
    private Object selectedAnimatorObject;
    private string newAnimationName = "NewAnimation";
    private string basePath = "Resources/Animators";
    private bool createOverrideClips = true;
    private int targetFPS = 24; // 新增FPS设置

    [MenuItem("Tools/UIM/Animator Override Creator")]
    public static void ShowWindow()
    {
        GetWindow<AnimatorOverrideCreator>("AOC Creator Pro");
    }

    private void OnGUI()
    {
        GUILayout.Label("Animator Override Controller Creator Pro", EditorStyles.boldLabel);

        selectedAnimatorObject = EditorGUILayout.ObjectField(
            "Target Animator", 
            selectedAnimatorObject, 
            typeof(UnityEngine.Object), 
            false);

        if (selectedAnimatorObject != null && 
            !(selectedAnimatorObject is Animator) && 
            !(selectedAnimatorObject is RuntimeAnimatorController))
        {
            EditorGUILayout.HelpBox("请选择 Animator 组件或 Animator Controller 资源", MessageType.Error);
            selectedAnimatorObject = null;
        }

        newAnimationName = EditorGUILayout.TextField("Animation Name", newAnimationName);
        basePath = EditorGUILayout.TextField("Save Path", basePath);
        
        createOverrideClips = EditorGUILayout.Toggle("Create Override Clips", createOverrideClips);
        
        // 新增FPS设置字段
        targetFPS = EditorGUILayout.IntField("Animation FPS", targetFPS);
        targetFPS = Mathf.Clamp(targetFPS, 1, 120); // 限制FPS范围

        EditorGUILayout.Space();

        if (GUILayout.Button("Create AOC with Selected Animator"))
        {
            CreateAOCWithValidation();
        }
    }

    private void CreateAOCWithValidation()
    {
        if (selectedAnimatorObject == null)
        {
            EditorUtility.DisplayDialog("Error", "请先选择 Animator 或 Animator Controller!", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(newAnimationName))
        {
            EditorUtility.DisplayDialog("Error", "请输入有效的动画名称!", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(basePath))
        {
            EditorUtility.DisplayDialog("Error", "保存路径不能为空!", "OK");
            return;
        }

        CreateAnimatorOverrideController();
    }

    private void CreateAnimatorOverrideController()
    {
        // 规范化路径
        basePath = basePath.Trim('/').Trim('\\');
        string fullBasePath = Path.Combine("Assets", basePath);
        string folderPath = Path.Combine(fullBasePath, newAnimationName);

        // 创建基础文件夹
        CreateDirectoryIfNotExists(fullBasePath);
        CreateDirectoryIfNotExists(folderPath);

        RuntimeAnimatorController sourceController = GetSourceController();
        if (sourceController == null) return;

        // 创建 Animator Override Controller
        AnimatorOverrideController aoc = new AnimatorOverrideController(sourceController);
        string aocName = $"AOC_{newAnimationName}.overrideController";
        string aocPath = Path.Combine(folderPath, aocName);
        aocPath = AssetDatabase.GenerateUniqueAssetPath(aocPath);

        // 处理override clips
        if (createOverrideClips)
        {
            ProcessOverrideClips(aoc, folderPath);
        }

        // 保存AOC
        AssetDatabase.CreateAsset(aoc, aocPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = aoc;
        
        Debug.Log($"成功创建 Animator Override Controller: {aocPath}");
    }

    private void CreateDirectoryIfNotExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            Directory.CreateDirectory(Path.GetFullPath(path));
            AssetDatabase.Refresh();
        }
    }

    private RuntimeAnimatorController GetSourceController()
    {
        if (selectedAnimatorObject is Animator animator)
        {
            if (animator.runtimeAnimatorController == null)
            {
                EditorUtility.DisplayDialog("Error", "选中的Animator没有分配Controller!", "OK");
                return null;
            }
            return animator.runtimeAnimatorController;
        }
        else if (selectedAnimatorObject is RuntimeAnimatorController controller)
        {
            return controller;
        }
        
        EditorUtility.DisplayDialog("Error", "选中的对象没有有效的 Animator Controller!", "OK");
        return null;
    }

    private void ProcessOverrideClips(AnimatorOverrideController aoc, string folderPath)
    {
        var originalClips = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        aoc.GetOverrides(originalClips);

        for (int i = 0; i < originalClips.Count; i++)
        {
            var pair = originalClips[i];
            AnimationClip originalClip = pair.Key;
            
            if (originalClip == null) continue;

            // 精确处理以"An"开头的clip名称（区分大小写）
            string newClipName = originalClip.name.StartsWith("An") 
                ? $"An_{newAnimationName}{originalClip.name.Substring(2)}"
                : $"{newAnimationName}_{originalClip.name}";
            
            // 创建并保存新clip
            AnimationClip newClip = new AnimationClip();
            newClip.name = newClipName;
            
            // 复制原始clip的设置
            EditorUtility.CopySerialized(originalClip, newClip);
            
            // 设置FPS
            SetAnimationClipFPS(newClip, targetFPS);
            
            string clipPath = Path.Combine(folderPath, $"{newClipName}.anim");
            clipPath = AssetDatabase.GenerateUniqueAssetPath(clipPath);
            AssetDatabase.CreateAsset(newClip, clipPath);
            
            // 更新AOC中的映射
            originalClips[i] = new KeyValuePair<AnimationClip, AnimationClip>(originalClip, newClip);
        }

        aoc.ApplyOverrides(originalClips);
    }

    // 新增方法：设置Animation Clip的FPS
    private void SetAnimationClipFPS(AnimationClip clip, int fps)
    {
        SerializedObject serializedClip = new SerializedObject(clip);
        SerializedProperty sampleRate = serializedClip.FindProperty("m_SampleRate");
        sampleRate.floatValue = fps;
        serializedClip.ApplyModifiedProperties();
    }
}