using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Text;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace UIM.Input
{
    [CustomEditor(typeof(UIM_InputSystem))]
    public class UIM_InputSystemEditor : Editor
    {
        private StringBuilder _inputLog = new StringBuilder();
        private Vector2 _scrollPosition;
        private float _lastInputTime;
        private string _lastInputAction;
        private string _lastInputDevice;
        private string _lastInputValue;

        public override void OnInspectorGUI()
        {
            // Draw default inspector
            DrawDefaultInspector();

            UIM_InputSystem inputSystem = (UIM_InputSystem)target;

            // Add space
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Input System Debugger", EditorStyles.boldLabel);

            // Display current state information
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Current State", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Mode: {inputSystem.currentMode}");
                EditorGUILayout.LabelField($"Device: {inputSystem.currentControlDevice}");
                EditorGUILayout.LabelField($"Gamepad: {inputSystem.currentGamepad}");

                EditorGUILayout.Space(10);

                EditorGUILayout.LabelField("Previous State", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField($"Mode: {inputSystem._previousMode}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Device: {inputSystem._previousControlDevice}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Gamepad: {inputSystem._previousGamepad}", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();

            // Display last input information
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Last Input", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Action: {_lastInputAction}");
                EditorGUILayout.LabelField($"Device: {_lastInputDevice}");
                EditorGUILayout.LabelField($"Value: {_lastInputValue}");
                EditorGUILayout.LabelField($"Time: {_lastInputTime:0.00}s ago");
            }
            EditorGUILayout.EndVertical();

            // Display input history
            EditorGUILayout.LabelField("Input History", EditorStyles.boldLabel);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(150));
            EditorGUILayout.TextArea(_inputLog.ToString(), EditorStyles.textArea);
            EditorGUILayout.EndScrollView();

            // Add buttons for manual control
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Clear Log"))
                {
                    _inputLog.Clear();
                }

                if (GUILayout.Button("Refresh Bindings"))
                {
                    RefreshBindingsInfo(inputSystem);
                }

                if (GUILayout.Button("Switch Gamepad"))
                {
                    inputSystem.SwitchToNextGamepad();
                }
            }
            EditorGUILayout.EndHorizontal();

            // Display current bindings
            DisplayCurrentBindings(inputSystem);
        }

        private void OnEnable()
        {
            // Subscribe to input events
            InputSystem.onEvent += OnInputSystemEvent;
        }

        private void OnDisable()
        {
            // Unsubscribe from input events
            InputSystem.onEvent -= OnInputSystemEvent;
        }

        private void OnInputSystemEvent(InputEventPtr eventPtr, InputDevice device)
        {
            if (!eventPtr.valid) return;

            foreach (var control in eventPtr.EnumerateChangedControls(device))
            {
                // 1. 跳过无意义的控件（如触摸屏的噪声）
                if (control.name.EndsWith("Noise") || control.name.Contains("Haptic"))
                    continue;

                // 2. 读取当前值
                var value = control.ReadValueAsObject();
                if (value == null) continue;

                // 3. 类型特定判断
                if (control is ButtonControl button)
                {
                    if (!button.isPressed) continue; // 只记录按下状态
                }
                else if (value is float floatValue)
                {
                    if (Mathf.Abs(floatValue) < 0.1f) continue; // 忽略微小浮动
                }

                // 4. 记录有效输入
                _lastInputTime = Time.time;
                _lastInputAction = control.name;
                _lastInputDevice = device.displayName;
                _lastInputValue = value.ToString();

                //Debug.Log($"Input detected: {_lastInputDevice}.{_lastInputAction} = {_lastInputValue}");
            }
        }

        private void DisplayCurrentBindings(UIM_InputSystem inputSystem)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Current Bindings", EditorStyles.boldLabel);

            PlayerInput playerInput = inputSystem.GetComponent<PlayerInput>();
            if (playerInput == null || playerInput.actions == null)
            {
                EditorGUILayout.HelpBox("No PlayerInput or Input Actions found", MessageType.Warning);
                return;
            }

            // 安全获取当前控制方案
            string currentControlScheme = playerInput.currentControlScheme ?? "Unknown";
            EditorGUILayout.LabelField($"Current Control Scheme: {currentControlScheme}");

            // 安全获取当前动作地图
            string actionMapName = playerInput.currentActionMap?.name ?? "None";
            EditorGUILayout.LabelField($"Current Action Map: {actionMapName}");

            EditorGUILayout.Space(5);

            // 添加空引用检查
            if (playerInput.actions == null)
            {
                EditorGUILayout.HelpBox("Input Actions are not assigned", MessageType.Error);
                return;
            }

            foreach (InputAction action in playerInput.actions)
            {
                if (action == null) continue; // 跳过空动作

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.LabelField(action.name ?? "Unnamed Action", EditorStyles.boldLabel);

                    foreach (InputBinding binding in action.bindings)
                    {
                        // 安全处理可能为null的binding.groups
                        if (string.IsNullOrEmpty(binding.groups)) continue;

                        // 安全比较控制方案
                        if (binding.groups.Contains(currentControlScheme))
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField(binding.ToDisplayString() ?? "N/A", GUILayout.Width(200));
                                EditorGUILayout.LabelField(binding.effectivePath ?? "No path");
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }


        private void RefreshBindingsInfo(UIM_InputSystem inputSystem)
        {
            PlayerInput playerInput = inputSystem.GetComponent<PlayerInput>();
            if (playerInput != null && playerInput.actions != null)
            {
                // Force a refresh of the bindings display
                Repaint();
            }
        }
    }
}