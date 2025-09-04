using System.Collections.Generic;
using HappyHotel.Core;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Prop
{
	[AutoInitComponent(typeof(DirectionComponent))]
	// 停止器道具：被触发时让触发者在本格停止，并将其朝向设置为本道具的方向
	public class StopperProp : ActivePlaceablePropBase
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
			var autoMove = triggerer.GetBehaviorComponent<AutoMoveComponent>();
			if (autoMove == null) return;

			// 读取本道具方向（若无方向组件，默认保持触发者当前方向不变）
			var faceDir = directionComponent != null ? directionComponent.GetDirection() : triggerer.GetBehaviorComponent<DirectionComponent>()?.GetDirection() ?? Direction.Right;

			// 让触发者执行外部停止逻辑（设置停止后的朝向并触发OnHitWall）
			autoMove.StopByExternalRequest(faceDir);
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

