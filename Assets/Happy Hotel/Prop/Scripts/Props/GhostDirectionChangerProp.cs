using System.Collections.Generic;
using HappyHotel.Core;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Equipment.Templates;
using HappyHotel.Prop.Settings;
using UnityEngine;
using HappyHotel.GameManager;

namespace HappyHotel.Prop
{
    // 幽灵方向改变器：被触发时在原地放置一个普通的同方向方向改变器；自身不改变触发者方向
    [AutoInitComponent(typeof(DirectionComponent))]
    public class GhostDirectionChangerProp : ActivePlaceablePropBase
    {
        // 方向到中文文本的映射
        private static readonly Dictionary<Direction, string> directionTextMap = new()
        {
            { Direction.Up, "上" },
            { Direction.Down, "下" },
            { Direction.Left, "左" },
            { Direction.Right, "右" }
        };

        private SpriteRenderer directionIndicator;
        private DirectionComponent directionComponent;

        protected override void Start()
        {
            base.Start();
            if (!directionIndicator) directionIndicator = GetComponentInChildren<SpriteRenderer>();
            directionComponent = GetBehaviorComponent<DirectionComponent>();

            if (directionComponent != null)
            {
                directionComponent.onDirectionChanged += OnDirectionChanged;
                UpdateDirectionIndicator();
            }
        }

        public override void OnTriggerInternal(BehaviorComponentContainer triggerer)
        {
            var grid = GetBehaviorComponent<GridObjectComponent>();
            var dirComp = GetBehaviorComponent<DirectionComponent>();
            if (grid == null) return;

            var position = grid.GetGridPosition();
            var direction = dirComp != null ? dirComp.GetDirection() : Direction.Right;

            // 使用“双重调度”：先安排到下一tick，再在该tick的PostTick生成，避免与角色同格即刻触发
            if (ClockSystem.Instance != null)
            {
                // 安排 NextTick->PostTick 生成
                ClockSystem.Instance.ScheduleNextTick(() =>
                {
                    ClockSystem.Instance.SchedulePostTick(() =>
                    {
                        SpawnDirectionChanger(position, direction);
                    });
                });
            }
            else
            {
                // 无ClockSystem时无法安全调度，直接返回
            }
            // 不改变触发者方向
        }

        private void SpawnDirectionChanger(Vector2Int position, Direction direction)
        {
            var propTypeId = Core.Registry.TypeId.Create<PropTypeId>("DirectionChanger");
            var setting = new DirectionalSetting(direction);
            var controller = PropController.Instance;
            if (controller != null)
            {
                var placed = controller.PlaceProp(position, propTypeId, setting);
                if (!placed) { }
            }
            else
            {
                // 无PropController时无法生成
            }
        }

        protected override void OnDestroy()
        {
            if (directionComponent != null) directionComponent.onDirectionChanged -= OnDirectionChanged;
            base.OnDestroy();
        }

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            if (template is DirectionalPlacementCardTemplate)
            {
                if (!directionIndicator) directionIndicator = GetComponentInChildren<SpriteRenderer>();
                if (directionComponent == null) directionComponent = GetBehaviorComponent<DirectionComponent>();
                if (directionIndicator && directionComponent != null) UpdateDirectionIndicator();
            }
            else
            {
                Debug.LogError($"模板不是DirectionalPlacementCardTemplate类型！实际类型: {template?.GetType().Name}");
            }
        }

        private void OnDirectionChanged(Direction newDirection)
        {
            UpdateDirectionIndicator();
        }

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

        // 使用propDescription作为描述模板
        public override string GetDescriptionTemplate()
        {
            if (template is ActivePlacementCardTemplate activeTemplate) return activeTemplate.propDescription ?? "";
            return base.GetDescriptionTemplate();
        }

        // 替换{direction}占位符
        protected override string FormatDescriptionInternal(string template)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            var currentDirection = directionComponent?.GetDirection() ?? Direction.Right;
            var directionText = directionTextMap.TryGetValue(currentDirection, out var text)
                ? text
                : currentDirection.ToString();
            return template.Replace("{direction}", directionText);
        }
    }
}

