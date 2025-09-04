using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.GameManager;
using UnityEngine;

namespace HappyHotel.Buff.Components
{
    // Buff容器组件，管理Character上的Buff
    public class BuffContainer : BehaviorComponentBase
    {
        private readonly List<BuffBase> activeBuffs = new();

        // Buff变化事件
        public event System.Action onBuffsChanged;

        public override void OnAttach(BehaviorComponentContainer host)
        {
            base.OnAttach(host);

            // 根据宿主标签注册不同的回合结束事件
            if (host != null && host.HasTag("Enemy"))
            {
                TurnManager.onEnemyTurnEnd += OnTurnEnd;
                TurnManager.onEnemyTurnStart += OnTurnStart;
            }
            else
            {
                // 角色（包含主角）在玩家回合结束处理
                TurnManager.onPlayerTurnEnd += OnTurnEnd;
                TurnManager.onPlayerTurnStart += OnTurnStart;
            }

            Debug.Log($"{host.gameObject.name} 初始化BuffContainer");
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            // 移除所有Buff
            var buffsToRemove = new List<BuffBase>(activeBuffs);
            foreach (var buff in buffsToRemove) RemoveBuff(buff);

            // 取消回合事件监听（两者都尝试取消，避免遗漏）
            TurnManager.onEnemyTurnEnd -= OnTurnEnd;
            TurnManager.onPlayerTurnEnd -= OnTurnEnd;
            TurnManager.onEnemyTurnStart -= OnTurnStart;
            TurnManager.onPlayerTurnStart -= OnTurnStart;
        }

        // 添加Buff
        public void AddBuff(BuffBase newBuff)
        {
            if (newBuff == null) return;

            // 检查是否已存在相同实例
            if (activeBuffs.Contains(newBuff)) return;


            // 查找相同类型的现有Buff
            var existingBuffsOfSameType = activeBuffs.Where(b => newBuff.CanMergeWith(b)).ToList();

            if (existingBuffsOfSameType.Count == 0)
            {
                // 没有相同类型的Buff，直接添加
                AddBuffDirectly(newBuff);
                return;
            }

            // 处理与第一个相同类型Buff的合并（简化处理，只与第一个合并）
            var existingBuff = existingBuffsOfSameType[0];
            var mergeResult = existingBuff.TryMergeWith(newBuff);

            switch (mergeResult.MergeType)
            {
                case BuffMergeType.Replace:
                    RemoveBuff(existingBuff);
                    AddBuffDirectly(mergeResult.ResultBuff);
                    Debug.Log($"Buff替换: {mergeResult.Reason}");
                    break;

                case BuffMergeType.Merge:
                    // 现有Buff已被修改，触发变化事件
                    onBuffsChanged?.Invoke();
                    Debug.Log($"Buff合并: {mergeResult.Reason}");
                    break;

                case BuffMergeType.Reject:
                    Debug.Log($"Buff被拒绝: {mergeResult.Reason}");
                    break;

                case BuffMergeType.Coexist:
                default:
                    // 共存，添加新Buff
                    AddBuffDirectly(newBuff);
                    break;
            }
        }

        // 直接添加Buff的内部方法
        private void AddBuffDirectly(BuffBase buff)
        {
            activeBuffs.Add(buff);
            buff.SetBuffContainer(this); // 设置BuffContainer引用
            buff.OnApply(GetHost());

            // 触发Buff变化事件
            onBuffsChanged?.Invoke();

            Debug.Log($"{GetHost().gameObject.name} 添加Buff: {buff.GetType().Name}");
        }

        // 移除Buff
        public void RemoveBuff(BuffBase buff)
        {
            if (buff == null || !activeBuffs.Contains(buff))
                return;

            activeBuffs.Remove(buff);
            buff.OnRemove(GetHost());

            // 触发Buff变化事件
            onBuffsChanged?.Invoke();

            Debug.Log($"{GetHost().gameObject.name} 移除Buff: {buff.GetType().Name}");
        }

        // 获取指定类型的所有Buff
        public List<T> GetBuffsOfType<T>() where T : BuffBase
        {
            return activeBuffs.OfType<T>().ToList();
        }

        // 获取所有Buff
        public List<BuffBase> GetAllBuffs()
        {
            return new List<BuffBase>(activeBuffs);
        }

        // 检查是否有指定类型的Buff
        public bool HasBuffOfType<T>() where T : BuffBase
        {
            return activeBuffs.OfType<T>().Any();
        }

        public int GetBuffCount()
        {
            return activeBuffs.Count;
        }

        // 清空所有Buff
        public void ClearAllBuffs()
        {
            var buffsToRemove = new List<BuffBase>(activeBuffs);
            foreach (var buff in buffsToRemove) RemoveBuff(buff);
            Debug.Log($"{GetHost().gameObject.name} 清空了所有Buff，共移除 {buffsToRemove.Count} 个");
        }

        // 处理回合结束事件
        private void OnTurnEnd(int turnNumber)
        {
            // 创建Buff列表的副本，避免在遍历过程中修改集合
            var buffsToProcess = new List<BuffBase>(activeBuffs);

            // 给每个Buff发送回合结束信号，让Buff自己决定要做什么
            foreach (var buff in buffsToProcess) buff.OnTurnEnd(turnNumber);

            Debug.Log($"{GetHost().gameObject.name} 回合结束处理完成，通知了 {buffsToProcess.Count} 个Buff");
        }

        // 处理回合开始事件
        private void OnTurnStart(int turnNumber)
        {
            var buffsToProcess = new List<BuffBase>(activeBuffs);
            foreach (var buff in buffsToProcess) buff.OnTurnStart(turnNumber);
            Debug.Log($"{GetHost().gameObject.name} 回合开始处理完成，通知了 {buffsToProcess.Count} 个Buff");
        }

        // 手动触发Buff变化事件（供Buff在自身数值变化时调用）
        public void NotifyBuffsChanged()
        {
            onBuffsChanged?.Invoke();
        }
    }
}