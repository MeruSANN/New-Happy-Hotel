using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using HappyHotel.Enemy;
using HappyHotel.Intent.Components;
using HappyHotel.Core.Grid.Components;
using UnityEngine;

namespace HappyHotel.GameManager
{
	// 敌人回合协调器：在敌人回合开始时收集快照，按网格顺序依次执行敌人意图
	[ManagedSingleton(SceneLoadMode.Exclude, "ShopScene", "MainMenu", "MapEditScene")]
	public class EnemyTurnCoordinator : SingletonBase<EnemyTurnCoordinator>
	{
		public async UniTask ExecuteEnemyTurnAsync()
		{
			var enemies = EnemyManager.Instance != null ? EnemyManager.Instance.GetAllObjects() : new List<EnemyBase>();
			if (enemies == null || enemies.Count == 0) return;

			// 回合开始时的快照：仅包含在网格上的敌人
			var snapshot = new List<(EnemyBase enemy, Vector2Int pos, int tie)>();
			foreach (var e in enemies)
			{
				if (e == null) continue;
				var grid = e.GetBehaviorComponent<GridObjectComponent>();
				if (grid == null) continue;
				var pos = grid.GetGridPosition();
				snapshot.Add((e, pos, e.GetInstanceID()));
			}

			if (snapshot.Count == 0) return;

			// 排序规则：y 降序（上→下），x 升序（左→右），同格用 InstanceID 兜底
			snapshot = snapshot
				.OrderByDescending(i => i.pos.y)
				.ThenBy(i => i.pos.x)
				.ThenBy(i => i.tie)
				.ToList();

			// 依次执行（串行）
			foreach (var item in snapshot)
			{
				if (item.enemy == null) continue;
				var executor = item.enemy.GetBehaviorComponent<TurnEndIntentExecutorComponent>();
				if (executor == null) continue;
				await SafeExecute(executor);
			}
		}

		private async UniTask SafeExecute(TurnEndIntentExecutorComponent executor)
		{
			try
			{
				await executor.ExecuteCurrentIntentAsync();
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"EnemyTurnCoordinator: 执行敌人意图时异常 - {ex.Message}");
			}
		}
	}
}
