using HappyHotel.Core.Singleton;
using UnityEngine;

namespace HappyHotel.Utils
{
    // UI单例连接基类，用于解决UI控制器获取单例的时序问题
    public abstract class SingletonConnectedUIBase<T> : MonoBehaviour where T : SingletonBase<T>
    {
        [Header("连接设置")] [SerializeField] private bool enableConnectionDebugLog; // 是否启用连接调试日志

        [SerializeField] private float maxRetryTime = 5f; // 最大重试时间（秒）
        private bool hasStarted;

        // 连接状态
        private bool isConnected;
        private float retryStartTime;

        // 单例引用
        protected T singletonInstance;

        protected virtual void Start()
        {
            hasStarted = true;
            retryStartTime = Time.time;
            TryConnectToSingleton();
            OnUIStart();
        }

        protected virtual void Update()
        {
            // 如果还没有连接且在重试时间内，持续尝试连接
            if (!isConnected && hasStarted && Time.time - retryStartTime < maxRetryTime) TryConnectToSingleton();
        }

        protected virtual void OnDestroy()
        {
            DisconnectFromSingleton();
            OnUIDestroy();
        }

        // 尝试连接到单例
        private void TryConnectToSingleton()
        {
            if (!isConnected && singletonInstance == null)
            {
                singletonInstance = SingletonBase<T>.Instance;

                if (singletonInstance != null)
                {
                    isConnected = true;

                    if (enableConnectionDebugLog) Debug.Log($"{GetType().Name}: 成功连接到 {typeof(T).Name}");

                    OnSingletonConnected();
                }
            }
        }

        // 断开与单例的连接
        private void DisconnectFromSingleton()
        {
            if (isConnected && singletonInstance != null)
            {
                OnSingletonDisconnected();

                isConnected = false;
                singletonInstance = null;

                if (enableConnectionDebugLog) Debug.Log($"{GetType().Name}: 已断开与 {typeof(T).Name} 的连接");
            }
        }

        // 检查是否已连接到单例
        protected bool IsConnectedToSingleton()
        {
            return isConnected && singletonInstance != null;
        }

        // 手动尝试重新连接
        public void ForceReconnect()
        {
            DisconnectFromSingleton();
            retryStartTime = Time.time;
            TryConnectToSingleton();
        }

        // 设置最大重试时间
        public void SetMaxRetryTime(float time)
        {
            maxRetryTime = Mathf.Max(0f, time);
        }

        // 启用/禁用连接调试日志
        public void SetConnectionDebugLog(bool enable)
        {
            enableConnectionDebugLog = enable;
        }

        // 抽象方法：当单例连接成功时调用
        protected abstract void OnSingletonConnected();

        // 抽象方法：当单例断开连接时调用
        protected abstract void OnSingletonDisconnected();

        // 虚方法：UI Start时调用（可选重写）
        protected virtual void OnUIStart()
        {
        }

        // 虚方法：UI Destroy时调用（可选重写）
        protected virtual void OnUIDestroy()
        {
        }
    }
}