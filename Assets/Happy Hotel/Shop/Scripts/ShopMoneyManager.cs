using HappyHotel.Core.Singleton;
using UnityEngine;

namespace HappyHotel.Shop
{
    // 商店金币管理器，负责金币的增减和管理
    [ManagedSingleton(true)]
    public class ShopMoneyManager : SingletonBase<ShopMoneyManager>
    {
        // 当前金币数量
        [SerializeField] private int currentMoney = 1000;

        // 属性访问器
        public int CurrentMoney => currentMoney;

        // 设置当前金币数量
        public void SetCurrentMoney(int money)
        {
            currentMoney = Mathf.Max(0, money);
            Debug.Log($"金币已设置为: {currentMoney}");
        }

        // 增加金币
        public void AddMoney(int amount)
        {
            if (amount > 0)
            {
                currentMoney += amount;
                Debug.Log($"增加金币: {amount}，当前金币: {currentMoney}");
            }
        }

        // 减少金币
        public bool SpendMoney(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning("消费金额必须大于0");
                return false;
            }

            if (currentMoney >= amount)
            {
                currentMoney -= amount;
                Debug.Log($"消费金币: {amount}，剩余金币: {currentMoney}");
                return true;
            }

            Debug.LogWarning($"金币不足！需要 {amount} 金币，当前只有 {currentMoney} 金币");
            return false;
        }

        // 检查是否有足够的金币
        public bool HasEnoughMoney(int amount)
        {
            return currentMoney >= amount;
        }

        // 重置金币到初始值
        public void ResetMoney()
        {
            currentMoney = 1000;
            Debug.Log($"金币已重置为: {currentMoney}");
        }
    }
}