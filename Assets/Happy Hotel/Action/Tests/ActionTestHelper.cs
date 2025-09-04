using System.Collections;
using HappyHotel.Action;
using HappyHotel.Action.Components;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Grid;
using UnityEngine;


// 行动测试辅助类，提供通用的测试方法
public static class ActionTestHelper
{
    // 通过移动对象触发行动执行
    public static IEnumerator ExecuteActionByMoving(IAction action, BehaviorComponentContainer actor,
        Vector2Int targetPosition)
    {
        // 添加行动到队列
        var actionQueue = actor.GetBehaviorComponent<ActionQueueComponent>();
        if (actionQueue == null)
        {
            Debug.LogError($"对象 {actor.name} 没有 ActionQueueComponent");
            yield break;
        }

        actionQueue.AddAction(action);

        // 移动对象到目标位置触发行动执行
        if (GridObjectManager.Instance != null)
            GridObjectManager.Instance.MoveObject(actor, targetPosition);
        else
            Debug.LogError("GridObjectManager.Instance 为空");

        // 等待一帧让行动执行完成
        yield return null;
    }

    // 通过移动两个对象到同一位置触发行动执行
    public static IEnumerator ExecuteActionsByMovingTogether(
        IAction action1, BehaviorComponentContainer actor1,
        IAction action2, BehaviorComponentContainer actor2,
        Vector2Int meetingPosition)
    {
        // 添加行动到各自的队列
        var actionQueue1 = actor1.GetBehaviorComponent<ActionQueueComponent>();
        var actionQueue2 = actor2.GetBehaviorComponent<ActionQueueComponent>();

        if (actionQueue1 == null || actionQueue2 == null)
        {
            Debug.LogError("其中一个对象没有 ActionQueueComponent");
            yield break;
        }

        if (action1 != null) actionQueue1.AddAction(action1);
        if (action2 != null) actionQueue2.AddAction(action2);

        // 移动两个对象到同一位置
        if (GridObjectManager.Instance != null)
        {
            GridObjectManager.Instance.MoveObject(actor1, meetingPosition);
            GridObjectManager.Instance.MoveObject(actor2, meetingPosition);
        }
        else
        {
            Debug.LogError("GridObjectManager.Instance 为空");
        }

        // 等待一帧让行动执行完成
        yield return null;
    }

    // 仅移动对象到指定位置（不添加行动）
    public static IEnumerator MoveObjectTo(BehaviorComponentContainer actor, Vector2Int targetPosition)
    {
        if (GridObjectManager.Instance != null)
            GridObjectManager.Instance.MoveObject(actor, targetPosition);
        else
            Debug.LogError("GridObjectManager.Instance 为空");

        // 等待一帧让移动完成
        yield return null;
    }

    // 添加行动到队列但不执行
    public static void AddActionToQueue(IAction action, BehaviorComponentContainer actor)
    {
        var actionQueue = actor.GetBehaviorComponent<ActionQueueComponent>();
        if (actionQueue == null)
        {
            Debug.LogError($"对象 {actor.name} 没有 ActionQueueComponent");
            return;
        }

        actionQueue.AddAction(action);
    }

    // 消耗队列中的一个行动（使用统一的执行逻辑）
    public static IEnumerator ConsumeActionFromQueue(BehaviorComponentContainer actor, Vector2Int targetPosition)
    {
        // 移动对象到目标位置触发行动消耗
        yield return MoveObjectTo(actor, targetPosition);
    }
}