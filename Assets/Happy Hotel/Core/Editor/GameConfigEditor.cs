#if UNITY_EDITOR
using HappyHotel.GameManager;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace HappyHotel.Core.Editor
{
    [CustomEditor(typeof(GameConfig))]
    public class GameConfigEditor : OdinEditor
    {
        private Vector2 scrollPosition = Vector2.zero;
        private bool showMultiplierPreview = true;

        public override void OnInspectorGUI()
        {
            // 先绘制Odin Inspector的默认内容
            base.OnInspectorGUI();

            var config = (GameConfig)target;

            // 添加分隔线
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // 金币奖励倍数预览
            showMultiplierPreview = EditorGUILayout.Foldout(showMultiplierPreview, "金币奖励倍数预览", true);

            if (showMultiplierPreview)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // 显示配置摘要
                EditorGUILayout.LabelField("配置摘要:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"第一回合倍数: {config.FirstTurnCoinMultiplier:F2}x");
                EditorGUILayout.LabelField($"曲线下降速度: {config.CurveDecayRate:F2}");
                EditorGUILayout.LabelField($"预定回合数: {config.TargetTurnForBaseReward}");
                EditorGUILayout.LabelField(
                    $"随机范围: {config.FinalRewardRandomMin:F1}x - {config.FinalRewardRandomMax:F1}x");

                EditorGUILayout.Space();

                // 显示回合倍数预览
                EditorGUILayout.LabelField("回合奖励倍数:", EditorStyles.boldLabel);

                // 创建滚动区域，限制高度
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(200));

                // 显示每回合的倍数
                for (var turn = 1; turn <= config.TargetTurnForBaseReward; turn++)
                {
                    var multiplier = config.CalculateCoinMultiplier(turn);

                    // 根据倍数高低设置不同颜色
                    var originalColor = GUI.color;
                    if (multiplier >= 1.4f)
                        GUI.color = Color.green;
                    else if (multiplier >= 1.2f)
                        GUI.color = Color.yellow;
                    else if (multiplier > 1.05f)
                        GUI.color = new Color(1f, 0.5f, 0f); // 橙色
                    else
                        GUI.color = Color.white;

                    EditorGUILayout.LabelField($"第{turn}回合: {multiplier:F3}x");
                    GUI.color = originalColor;
                }

                // 显示预定回合之后的倍数
                GUI.color = Color.gray;
                EditorGUILayout.LabelField($"第{config.TargetTurnForBaseReward}回合及之后: 1.000x");
                GUI.color = Color.white;

                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space();

                // 示例计算
                EditorGUILayout.LabelField("示例计算 (基础奖励100金币):", EditorStyles.boldLabel);

                var baseReward = 100;
                for (var turn = 1; turn <= Mathf.Min(5, config.TargetTurnForBaseReward); turn++)
                {
                    // 计算不含随机因子的奖励
                    var multiplier = config.CalculateCoinMultiplier(turn);
                    var rewardWithoutRandom = Mathf.RoundToInt(baseReward * multiplier);

                    // 计算随机范围
                    var minReward = Mathf.RoundToInt(rewardWithoutRandom * config.FinalRewardRandomMin);
                    var maxReward = Mathf.RoundToInt(rewardWithoutRandom * config.FinalRewardRandomMax);

                    EditorGUILayout.LabelField($"第{turn}回合: {minReward}-{maxReward} 金币 (基础: {rewardWithoutRandom})");
                }

                if (config.TargetTurnForBaseReward > 5)
                {
                    EditorGUILayout.LabelField("...");
                    var finalMinReward = Mathf.RoundToInt(baseReward * config.FinalRewardRandomMin);
                    var finalMaxReward = Mathf.RoundToInt(baseReward * config.FinalRewardRandomMax);
                    EditorGUILayout.LabelField(
                        $"第{config.TargetTurnForBaseReward}回合及之后: {finalMinReward}-{finalMaxReward} 金币");
                }

                EditorGUILayout.EndVertical();
            }

            // 验证配置
            EditorGUILayout.Space();
            if (!config.IsValid())
                EditorGUILayout.HelpBox("配置验证失败！请检查配置参数。", MessageType.Error);
            else
                EditorGUILayout.HelpBox("配置验证通过。", MessageType.Info);
        }
    }
}
#endif