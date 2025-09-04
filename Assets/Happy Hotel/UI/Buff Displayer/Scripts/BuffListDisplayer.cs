using System.Collections;
using System.Collections.Generic;
using HappyHotel.Buff;
using HappyHotel.Buff.Components;
using HappyHotel.Core.BehaviorComponent;
using UnityEngine;

namespace HappyHotel.UI
{
    // Buff列表显示控制器，用于显示实体身上的所有Buff
    public class BuffListDisplayer : MonoBehaviour
    {
        [SerializeField] private BehaviorComponentContainer container;
        [SerializeField] private GameObject buffIconPrefab; // Buff图标预制体
        [SerializeField] private bool autoFindContainer; // 是否自动查找BehaviorComponentContainer
        [SerializeField] private string targetTag = "MainCharacter"; // 自动查找对象的tag
        private readonly List<BuffIconDisplayer> iconDisplayers = new();

        private BuffContainer buffContainer;
        private Coroutine delayedUpdateCoroutine; // 延迟更新协程

        private void Start()
        {
            RefreshIconDisplayers();
            GetBuffContainer();
        }

        private void Update()
        {
            // 如果开启自动查找且container为空，则根据tag查找
            if (autoFindContainer && !container)
            {
                AutoFindContainer();
                GetBuffContainer();
            }
        }

        private void OnDestroy()
        {
            // 停止延迟更新协程
            if (delayedUpdateCoroutine != null)
            {
                StopCoroutine(delayedUpdateCoroutine);
                delayedUpdateCoroutine = null;
            }

            // 取消事件监听
            if (buffContainer != null)
            {
                buffContainer.onBuffsChanged -= OnBuffsChanged;
                buffContainer = null;
            }
        }

        public void SetRelatedContainer(BehaviorComponentContainer componentContainer)
        {
            container = componentContainer;
            GetBuffContainer();
        }

        private void AutoFindContainer()
        {
            // 根据tag查找对象
            var targetObject = GameObject.FindGameObjectWithTag(targetTag);
            if (targetObject != null)
            {
                container = targetObject.GetComponent<BehaviorComponentContainer>();
                if (container == null) Debug.LogWarning($"找到tag为{targetTag}的对象，但没有BehaviorComponentContainer组件");
            }
        }

        private void GetBuffContainer()
        {
            if (container != null)
            {
                // 取消之前的事件监听
                if (buffContainer != null) buffContainer.onBuffsChanged -= OnBuffsChanged;

                // 获取BuffContainer
                buffContainer = container.GetBehaviorComponent<BuffContainer>();
                if (buffContainer != null)
                {
                    // 注册事件监听
                    buffContainer.onBuffsChanged += OnBuffsChanged;

                    // 立即更新显示
                    UpdateDisplay();
                }
                else
                {
                    Debug.LogWarning("BuffListDisplayer: 无法获取BuffContainer");
                }
            }
        }

        // Buff变化事件处理
        private void OnBuffsChanged()
        {
            // 检查游戏对象是否还处于active状态
            if (!gameObject.activeInHierarchy) return;

            // 停止之前的延迟更新协程（如果存在）
            if (delayedUpdateCoroutine != null) StopCoroutine(delayedUpdateCoroutine);

            // 启动新的延迟更新协程
            delayedUpdateCoroutine = StartCoroutine(DelayedUpdateDisplay());
        }

        // 延迟一帧更新显示，确保所有处理器都已应用
        private IEnumerator DelayedUpdateDisplay()
        {
            yield return null; // 等待一帧，确保所有事件处理器都已执行完毕
            UpdateDisplay();
            delayedUpdateCoroutine = null;
        }

        private void UpdateDisplay()
        {
            if (buffContainer == null) return;

            // 获取当前所有的Buff
            var buffs = buffContainer.GetAllBuffs();

            // 如果现有的显示器数量不够，创建新的
            while (iconDisplayers.Count < buffs.Count) CreateNewIconDisplayer();

            // 更新所有图标显示器
            for (var i = 0; i < iconDisplayers.Count; i++)
                if (i < buffs.Count && buffs[i] is BuffBase buff)
                    // 传递完整的Buff实例给显示器
                    iconDisplayers[i].SetBuff(buff);
                else
                    // 隐藏多余的显示器
                    iconDisplayers[i].Hide();
        }

        // 刷新图标显示器列表，获取所有子对象中的BuffIconDisplayer组件
        private void RefreshIconDisplayers()
        {
            foreach (var icon in iconDisplayers) icon.Show();

            iconDisplayers.Clear();
            var displayers = GetComponentsInChildren<BuffIconDisplayer>();
            iconDisplayers.AddRange(displayers);
        }

        // 创建新的图标显示器
        private void CreateNewIconDisplayer()
        {
            if (buffIconPrefab != null)
            {
                // 使用预制体创建
                var iconObject = Instantiate(buffIconPrefab, transform);
                var displayer = iconObject.GetComponent<BuffIconDisplayer>();
                if (displayer != null) iconDisplayers.Add(displayer);
            }
            else
            {
                // 没有预制体时输出错误
                Debug.LogError("BuffListDisplayer: 未设置buffIconPrefab预制体，无法创建图标显示器");
            }
        }

        // 手动刷新显示（供外部调用）
        public void RefreshDisplay()
        {
            // 检查游戏对象是否还处于active状态
            if (!gameObject.activeInHierarchy) return;

            // 停止之前的延迟更新协程（如果存在）
            if (delayedUpdateCoroutine != null) StopCoroutine(delayedUpdateCoroutine);

            // 启动新的延迟更新协程
            delayedUpdateCoroutine = StartCoroutine(DelayedUpdateDisplay());
        }
    }
}