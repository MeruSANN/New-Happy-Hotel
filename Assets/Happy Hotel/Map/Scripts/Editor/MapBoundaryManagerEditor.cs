#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace HappyHotel.Map
{
    [CustomEditor(typeof(MapBoundaryManager))]
    public class MapBoundaryManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var manager = (MapBoundaryManager)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("手动控制", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("刷新边界")) manager.RefreshBoundary();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("当前状态", EditorStyles.boldLabel);

            if (MapManager.Instance != null)
            {
                var mapSize = MapManager.Instance.GetMapSize();
                EditorGUILayout.LabelField($"地图大小: {mapSize.x} x {mapSize.y}");
            }
            else
            {
                EditorGUILayout.LabelField("地图大小: MapManager未找到");
            }

            var targetCamera = Camera.main;
            if (targetCamera != null)
            {
                EditorGUILayout.LabelField($"摄像机位置: {targetCamera.transform.position}");
                if (targetCamera.orthographic) EditorGUILayout.LabelField($"摄像机尺寸: {targetCamera.orthographicSize}");
            }
            else
            {
                EditorGUILayout.LabelField("摄像机: 未找到主摄像机");
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "使用说明:\n" +
                "1. 边界Tile：直接指定用于边界的TileBase资源\n" +
                "2. 启用边界：是否在地图周围自动添加边界\n" +
                "3. 目标摄像机：指定要控制的摄像机（默认使用主摄像机）\n" +
                "4. 自动调整摄像机：是否自动调整摄像机位置和尺寸\n" +
                "5. 摄像机边距：摄像机视野的额外边距\n" +
                "6. 最小/最大摄像机尺寸：限制摄像机缩放范围\n\n" +
                "注意：边界会直接添加到MapTilemap上，与地图内容共享同一个Tilemap。",
                MessageType.Info
            );
        }
    }
}
#endif