using System.Collections.Generic;
using HappyHotel.Core;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Prop
{
    [AutoInitComponent(typeof(DirectionComponent))]
    public class DirectionChangerProp : ActivePlaceablePropBase
    {
        // 方向到中文文本的映射
        private static readonly Dictionary<Direction, string> directionTextMap = new()
        {
            { Direction.Up, "上" },
            { Direction.Down, "下" },
            { Direction.Left, "左" },
            { Direction.Right, "右" }
        };

        // 方向指示器
        private SpriteRenderer directionIndicator;

        // 方向组件
        private DirectionComponent directionComponent;

        // 标记是否已经被DirectionalSetting配置过方向
        protected bool hasBeenConfiguredBySetting;

        protected override void Start()
        {
            base.Start();

            if (!directionIndicator) directionIndicator = GetComponentInChildren<SpriteRenderer>();

            // 获取方向组件
            directionComponent = GetBehaviorComponent<DirectionComponent>();

            if (directionComponent != null)
            {
                // 监听方向变化事件（必须在任何方向设置之前）
                directionComponent.onDirectionChanged += OnDirectionChanged;

                // 检查DirectionComponent是否已经有方向设置（通过DirectionalSetting）
                var currentDirection = directionComponent.GetDirection();

                // 如果仍然是默认值(Up)且还没有被Setting配置过，则初始化为Right
                if (currentDirection == Direction.Up && !hasBeenConfiguredBySetting)
                    directionComponent.SetDirection(Direction.Right);

                // 初始化显示
                UpdateDirectionIndicator();
            }
        }

        protected override void OnDestroy()
        {
            // 清理事件监听
            if (directionComponent != null) directionComponent.onDirectionChanged -= OnDirectionChanged;
            base.OnDestroy();
        }

        // 由DirectionalSetting调用，标记已被配置
        public void MarkAsConfiguredBySetting()
        {
            hasBeenConfiguredBySetting = true;
        }

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            if (template is DirectionalPlacementCardTemplate dirTemplate)
            {
                if (!directionIndicator) directionIndicator = GetComponentInChildren<SpriteRenderer>();

                // 确保DirectionComponent已经初始化
                if (directionComponent == null) directionComponent = GetBehaviorComponent<DirectionComponent>();

                // 只有在两个组件都存在时才更新显示
                if (directionIndicator && directionComponent != null) UpdateDirectionIndicator();
            }
            else
            {
                Debug.LogError($"模板不是DirectionalPlacementCardTemplate类型！实际类型: {template?.GetType().Name}");
            }
        }

        public override void OnTriggerInternal(BehaviorComponentContainer triggerer)
        {
            var triggererDirection = triggerer.GetBehaviorComponent<DirectionComponent>();
            if (triggererDirection != null && directionComponent != null)
            {
                // 改变移动方向为道具的方向
                triggererDirection.SetDirection(directionComponent.GetDirection());

                // 播放触发效果
                PlayTriggerEffect();
            }
            else
            {
                Debug.LogError($"触发者 {triggerer.name} 或道具没有DirectionComponent组件");
            }
        }

        // 设置强制方向
        public void SetForcedDirection(Direction direction)
        {
            if (directionComponent != null) directionComponent.SetDirection(direction);
        }

        // 强制更新显示（公共方法，用于调试）
        public void ForceUpdateDisplay()
        {
            UpdateDirectionIndicator();
        }

        // 获取强制方向
        public Direction GetForcedDirection()
        {
            return directionComponent?.GetDirection() ?? Direction.Right;
        }

        // 方向改变时的回调
        private void OnDirectionChanged(Direction newDirection)
        {
            UpdateDirectionIndicator();
        }

        // 更新方向指示器的显示
        private void UpdateDirectionIndicator()
        {
            if (directionIndicator && directionComponent != null)
            {
                if (template is DirectionalPlacementCardTemplate dirTemplate)
                {
                    if (dirTemplate.directionSprites != null &&
                        (int)directionComponent.GetDirection() < dirTemplate.directionSprites.Length)
                    {
                        var newSprite = dirTemplate.directionSprites[(int)directionComponent.GetDirection()];
                        directionIndicator.sprite = newSprite;
                    }
                    else
                    {
                        Debug.LogError(
                            $"精灵数组为空或索引超出范围！数组长度: {dirTemplate.directionSprites?.Length}, 方向索引: {(int)directionComponent.GetDirection()}");
                    }
                }
                else
                {
                    Debug.LogError($"模板不是DirectionalPlacementCardTemplate类型！实际类型: {template?.GetType().Name}");
                }
            }
            else
            {
                Debug.LogError("DirectionIndicator或DirectionComponent为空，无法更新方向指示器");
            }
        }

        // 播放触发效果
        private void PlayTriggerEffect()
        {
            // TODO: 添加触发效果（如粒子效果、声音等）
            var gridPos = GetBehaviorComponent<GridObjectComponent>()?.GetGridPosition() ?? Vector2Int.zero;
            var direction = directionComponent?.GetDirection() ?? Direction.Right;
            Debug.Log($"方向改变器触发{gridPos}：{direction}");
        }

        // 重写GetDescriptionTemplate方法，使用propDescription作为描述模板
        public override string GetDescriptionTemplate()
        {
            if (template is ActivePlacementCardTemplate activeTemplate) return activeTemplate.propDescription ?? "";
            return base.GetDescriptionTemplate();
        }

        // 重写FormatDescriptionInternal方法，替换{direction}占位符
        protected override string FormatDescriptionInternal(string template)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            // 获取当前方向
            var currentDirection = directionComponent?.GetDirection() ?? Direction.Right;

            // 获取方向对应的中文文本
            var directionText = directionTextMap.TryGetValue(currentDirection, out var text)
                ? text
                : currentDirection.ToString();

            // 替换{direction}占位符
            return template.Replace("{direction}", directionText);
        }
    }
}