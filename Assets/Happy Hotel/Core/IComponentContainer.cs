using HappyHotel.Core.Tag;
using UnityEngine;

namespace HappyHotel.Core
{
    // 通用组件容器接口，统一BehaviorComponentContainer和EntityComponentContainer
    public interface IComponentContainer : ITaggable
    {
        // 容器名称
        string Name { get; }

        // 检查容器是否有效/未销毁
        bool IsValid { get; }

        // 获取GameObject（BehaviorComponentContainer有，EntityComponentContainer可返回null）
        GameObject GetGameObject();
    }
}