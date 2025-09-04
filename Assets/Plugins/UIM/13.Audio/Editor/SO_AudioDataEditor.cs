#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

[CustomEditor(typeof(SO_AudioData))]
public class SO_AudioDataEditor : OdinEditor
{
    private AudioSource previewSource;
    private string currentlyPlayingKey;

    protected override void OnEnable()
    {
        base.OnEnable();
        
        // 创建用于预览的临时AudioSource
        previewSource = EditorUtility.CreateGameObjectWithHideFlags(
            "AudioPreviewSource", 
            HideFlags.HideAndDontSave, 
            typeof(AudioSource)
        ).GetComponent<AudioSource>();
    }

    protected override void OnDisable()
    {
        // 清理临时对象
        if (previewSource != null)
        {
            DestroyImmediate(previewSource.gameObject);
        }
        base.OnDisable();
    }

    public override void OnInspectorGUI()
    {
        // 先绘制默认的Odin属性
        base.OnInspectorGUI();

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("音频预览", EditorStyles.boldLabel);

        // 获取当前目标对象
        SO_AudioData audioData = (SO_AudioData)target;

        // 全局控制
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("停止所有预览", GUILayout.Height(25)))
            {
                StopAllPreviews();
            }
            
            EditorGUILayout.LabelField("当前播放: " + (currentlyPlayingKey ?? "无"), EditorStyles.miniLabel);
        }
        EditorGUILayout.EndHorizontal();

        // 绘制音频列表
        if (audioData.audioDataList != null && audioData.audioDataList.Count > 0)
        {
            EditorGUILayout.Space(10);
            
            for (int i = 0; i < audioData.audioDataList.Count; i++)
            {
                DrawAudioEntry(audioData, i, audioData.audioDataList[i]);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("没有音频数据", MessageType.Info);
        }
    }

    private void DrawAudioEntry(SO_AudioData audioData, int index, AudioClipEntry entry)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.BeginHorizontal();
            {
                // 键名标签
                EditorGUILayout.LabelField(entry.key, GUILayout.Width(150));
                
                // 音频剪辑字段
                EditorGUI.BeginChangeCheck();
                var newClip = (AudioClip)EditorGUILayout.ObjectField(
                    entry.audioClip, 
                    typeof(AudioClip), 
                    false,
                    GUILayout.Width(200)
                );
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(audioData, "Modify Audio Clip");
                    entry.audioClip = newClip;
                    audioData.audioDataList[index] = entry;
                    EditorUtility.SetDirty(audioData);
                }
                
                // 播放/停止按钮
                bool isPlaying = currentlyPlayingKey == entry.key && previewSource.isPlaying;
                GUI.enabled = entry.audioClip != null;
                if (GUILayout.Button(
                    isPlaying ? "■ 停止" : "▶ 播放", 
                    GUILayout.Width(80), 
                    GUILayout.Height(20)))
                {
                    if (isPlaying)
                    {
                        StopPreview();
                    }
                    else
                    {
                        PlayPreview(audioData, entry);
                    }
                }
                GUI.enabled = true;
                
                // 时长信息
                if (entry.audioClip != null)
                {
                    EditorGUILayout.LabelField(
                        $"{entry.audioClip.length:F2}秒", 
                        EditorStyles.miniLabel,
                        GUILayout.Width(60)
                    );
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    private void PlayPreview(SO_AudioData audioData, AudioClipEntry entry)
    {
        if (entry.audioClip == null) return;
        
        StopPreview();
        
        previewSource.clip = entry.audioClip;
        previewSource.volume = audioData.SFVolume;
        previewSource.Play();
        
        currentlyPlayingKey = entry.key;
        EditorApplication.update += UpdatePreview;
    }

    private void StopPreview()
    {
        if (previewSource.isPlaying)
            previewSource.Stop();
        
        currentlyPlayingKey = null;
        EditorApplication.update -= UpdatePreview;
    }

    private void StopAllPreviews()
    {
        StopPreview();
    }

    private void UpdatePreview()
    {
        if (!previewSource.isPlaying)
        {
            StopPreview();
        }
        Repaint();
    }
}
#endif