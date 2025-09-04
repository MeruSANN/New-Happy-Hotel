using System.Collections.Generic;
using System;
using HappyHotel.Action;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Singleton;
using UnityEngine;
using UnityEngine.Events;

// 时钟系统 - 提供统一的时钟信号来控制游戏中的定时行为
// 类似CPU的时钟信号，用于同步AutoMoveComponent等需要定时执行的行为
namespace HappyHotel.GameManager
{
    [ManagedSingleton(SceneLoadMode.Exclude, "MapEditScene", "ShopScene", "MainMenu")]
    public class ClockSystem : SingletonBase<ClockSystem>
    {
        public UnityEvent onClockTick = new(); // 时钟信号事件
        public UnityEvent onActionChainTick = new(); // 行动链时钟信号事件
        private float clockInterval = 0.2f; // 时钟间隔时间
        private int currentActionChainStep;

        private ActionChainExecutionData currentExecution;

        // 行动链执行状态
        private bool isActionChainActive;
        private bool isClockRunning = true; // 时钟是否正在运行
        private float lastActionChainTime;

        // 私有变量
        private float lastClockTime; // 上次时钟信号的时间
        private int totalActionChainSteps;
        private bool useDividedTimeExecution = true; // 是否使用等分时间执行行动链

        // 下一次时钟tick要执行的动作
        private readonly List<System.Action> actionsNextTick = new();
        // 在当前tick分发过程中被安排的动作，将在下一个tick再执行
        private readonly List<System.Action> actionsBuffer = new();
        private bool isTickDispatching;

        // 本tick onClockTick 之后要执行的动作（PostTick）
        private readonly List<System.Action> actionsPostTick = new();

        private void Update()
        {
            if (!isClockRunning) return;

            // 只在播放状态下发出时钟信号
            if (GameManager.Instance != null &&
                GameManager.Instance.GetGameState() != GameManager.GameState.Playing) return;

            // 行动链时钟信号（优先级高于自动移动）
            if (isActionChainActive && Time.time - lastActionChainTime >= GetCurrentActionChainInterval())
            {
                onActionChainTick?.Invoke();
                lastActionChainTime = Time.time;
                currentActionChainStep++;

                // 检查是否完成所有步骤
                if (currentActionChainStep >= totalActionChainSteps) CompleteActionChain();
                return; // 行动链执行期间暂停自动移动时钟
            }

            // 自动移动时钟信号
            if (Time.time - lastClockTime >= clockInterval)
            {
                // 先执行上一次安排在“下一tick”的动作（在分发tick之前执行，避免干扰当前枚举）
                isTickDispatching = true;
                if (actionsNextTick.Count > 0)
                {
                    var pending = new List<System.Action>(actionsNextTick);
                    actionsNextTick.Clear();
                    foreach (var act in pending)
                    {
                        try { act?.Invoke(); }
                        catch (Exception e) { Debug.LogError($"ClockSystem ScheduleNextTick 执行异常: {e}"); }
                    }
                }

                // 发出本次tick
                onClockTick?.Invoke();

                // 执行本tick的PostTick动作（在自动移动与触发之后）
                if (actionsPostTick.Count > 0)
                {
                    var post = new List<System.Action>(actionsPostTick);
                    actionsPostTick.Clear();
                    foreach (var act in post)
                    {
                        try { act?.Invoke(); }
                        catch (Exception e) { Debug.LogError($"ClockSystem SchedulePostTick 执行异常: {e}"); }
                    }
                }

                // 将在分发过程中安排的动作转移到下一个tick队列
                isTickDispatching = false;
                if (actionsBuffer.Count > 0)
                {
                    actionsNextTick.AddRange(actionsBuffer);
                    actionsBuffer.Clear();
                }
                lastClockTime = Time.time;
            }
        }

        protected override void OnSingletonAwake()
        {
            lastClockTime = Time.time;
            Debug.Log("时钟系统已初始化，时钟间隔: " + clockInterval + "秒");
        }

        // 计算当前行动链的时间间隔 - 基于最长链长度
        private float GetCurrentActionChainInterval()
        {
            if (totalActionChainSteps <= 1) return clockInterval;
            // 将整个时钟周期等分为totalActionChainSteps份
            return clockInterval / totalActionChainSteps;
        }

        // 提交行动链执行
        public void SubmitActionChainExecution(Queue<IAction> chainA, Queue<IAction> chainB,
            BehaviorComponentContainer partyA, BehaviorComponentContainer partyB)
        {
            // 立即执行第一组行动
            ExecuteFirstActions(chainA, chainB, partyA, partyB);

            // 设置执行参数
            var maxLength = Mathf.Max(chainA.Count, chainB.Count);
            if (maxLength > 0)
            {
                if (useDividedTimeExecution)
                {
                    // 等分时间执行模式
                    totalActionChainSteps = maxLength;
                    currentActionChainStep = 0; // 从0开始计步，剩余每一步都由时钟驱动
                    isActionChainActive = true;
                    lastActionChainTime = Time.time;

                    // 保存当前执行数据
                    currentExecution = new ActionChainExecutionData
                    {
                        chainA = chainA,
                        chainB = chainB,
                        partyA = partyA,
                        partyB = partyB,
                        maxLength = maxLength
                    };

                    var interval = GetCurrentActionChainInterval();
                    Debug.Log($"行动链开始执行（等分时间模式），最长链长度: {maxLength}，时间间隔: {interval}s");
                }
                else
                {
                    // 一次性执行完所有行动
                    ExecuteAllRemainingActions(chainA, chainB, partyA, partyB);
                    Debug.Log($"行动链开始执行（一次性模式），最长链长度: {maxLength}");
                }
            }
        }

        // 立即执行第一组行动
        private void ExecuteFirstActions(Queue<IAction> chainA, Queue<IAction> chainB,
            BehaviorComponentContainer partyA, BehaviorComponentContainer partyB)
        {
            var actionA = chainA.Count > 0 ? chainA.Dequeue() : null;
            var actionB = chainB.Count > 0 ? chainB.Dequeue() : null;

            // 执行第一组行动
            if (actionA != null || actionB != null)
                // 调用ActionChainResolver的simultaneousResolver
                if (ActionChainResolver.Instance != null)
                    ActionChainResolver.Instance.ExecuteActions(actionA, actionB, partyA, partyB);
        }

        // 执行下一组行动
        public void ExecuteNextActions()
        {
            if (currentExecution == null) return;

            var actionA = currentExecution.chainA.Count > 0 ? currentExecution.chainA.Dequeue() : null;
            var actionB = currentExecution.chainB.Count > 0 ? currentExecution.chainB.Dequeue() : null;

            // 执行行动
            if (actionA != null || actionB != null)
                // 调用ActionChainResolver的simultaneousResolver
                if (ActionChainResolver.Instance != null)
                    ActionChainResolver.Instance.ExecuteActions(actionA, actionB,
                        currentExecution.partyA, currentExecution.partyB);
        }

        // 一次性执行所有剩余行动
        private void ExecuteAllRemainingActions(Queue<IAction> chainA, Queue<IAction> chainB,
            BehaviorComponentContainer partyA, BehaviorComponentContainer partyB)
        {
            while (chainA.Count > 0 || chainB.Count > 0)
            {
                var actionA = chainA.Count > 0 ? chainA.Dequeue() : null;
                var actionB = chainB.Count > 0 ? chainB.Dequeue() : null;

                // 执行行动
                if (actionA != null || actionB != null)
                    // 调用ActionChainResolver的simultaneousResolver
                    if (ActionChainResolver.Instance != null)
                        ActionChainResolver.Instance.ExecuteActions(actionA, actionB, partyA, partyB);
            }
        }

        // 完成行动链执行
        private void CompleteActionChain()
        {
            isActionChainActive = false;
            currentActionChainStep = 0;
            totalActionChainSteps = 0;
            currentExecution = null;
            Debug.Log("行动链执行完成，恢复自动移动时钟");
        }

        // 设置时钟间隔
        public void SetClockInterval(float interval)
        {
            if (interval <= 0)
            {
                Debug.LogWarning("时钟间隔必须大于0，当前值: " + interval);
                return;
            }

            clockInterval = interval;
            Debug.Log("时钟间隔已设置为: " + interval + "秒");
        }

        // 获取当前时钟间隔
        public float GetClockInterval()
        {
            return clockInterval;
        }

        // 启动时钟
        public void StartClock()
        {
            isClockRunning = true;
            lastClockTime = Time.time;
            Debug.Log("时钟已启动");
        }

        // 停止时钟
        public void StopClock()
        {
            isClockRunning = false;
            Debug.Log("时钟已停止");
        }

        // 检查时钟是否正在运行
        public bool IsClockRunning()
        {
            return isClockRunning;
        }

        // 手动触发一次时钟信号
        public void TriggerClockTick()
        {
            onClockTick?.Invoke();
            Debug.Log("手动触发时钟信号");
        }

        // 获取距离下次时钟信号的时间
        public float GetTimeToNextTick()
        {
            if (!isClockRunning) return 0f;

            var timeSinceLastTick = Time.time - lastClockTime;
            return Mathf.Max(0f, clockInterval - timeSinceLastTick);
        }

        // 设置是否使用等分时间执行行动链
        public void SetUseDividedTimeExecution(bool useDivided)
        {
            useDividedTimeExecution = useDivided;
            Debug.Log($"行动链执行模式已设置为: {(useDivided ? "等分时间模式" : "一次性模式")}");
        }

        // 获取当前行动链执行模式
        public bool GetUseDividedTimeExecution()
        {
            return useDividedTimeExecution;
        }

        // 安排一个动作在下一个时钟tick触发前执行
        public void ScheduleNextTick(System.Action action)
        {
            if (action == null) return;
            if (isTickDispatching)
                actionsBuffer.Add(action);
            else
                actionsNextTick.Add(action);

        }

        // 安排一个动作在本tick onClockTick 之后执行；若当前不在tick分发阶段，则会在下一个tick的onClockTick之后执行
        public void SchedulePostTick(System.Action action)
        {
            if (action == null) return;
            actionsPostTick.Add(action);
        }

        // 行动链数据管理
        private class ActionChainExecutionData
        {
            public Queue<IAction> chainA;
            public Queue<IAction> chainB;
            public int maxLength; // 最长链的长度，决定时间间隔
            public BehaviorComponentContainer partyA;
            public BehaviorComponentContainer partyB;
        }
    }
}