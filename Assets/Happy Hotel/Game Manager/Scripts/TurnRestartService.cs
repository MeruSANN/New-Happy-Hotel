using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using HappyHotel.Buff.Components;
using HappyHotel.Card;
using HappyHotel.Core;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Grid;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.Singleton;
using HappyHotel.Core.ValueProcessing.Components;
using HappyHotel.Equipment;
using HappyHotel.GameManager;
using HappyHotel.Inventory;
using HappyHotel.Prop;
using UnityEngine;

namespace HappyHotel.TurnRestart
{
	// 回合重开服务：在玩家回合开始时拍快照，静止状态下可静默恢复
	[ManagedSingleton(SceneLoadMode.Exclude, "ShopScene", "MainMenu", "MapEditScene")]
	public class TurnRestartService : SingletonBase<TurnRestartService>
	{
		private Snapshot snapshot;

		protected override void OnSingletonAwake()
		{
			base.OnSingletonAwake();
			GameManager.GameManager.onGameStateChanged += OnGameStateChanged;
			TurnManager.onPlayerTurnStart += OnPlayerTurnStart;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			GameManager.GameManager.onGameStateChanged -= OnGameStateChanged;
			TurnManager.onPlayerTurnStart -= OnPlayerTurnStart;
		}

		private void OnGameStateChanged(GameManager.GameManager.GameState state)
		{
			// 保证仅在静止态下允许恢复
		}

		private async void OnPlayerTurnStart(int turn)
		{
			await UniTask.Yield();
			MakeSnapshot();
		}

		public bool CanRestartNow()
		{
			if (GameManager.GameManager.Instance == null || TurnManager.Instance == null) return false;
			return GameManager.GameManager.Instance.GetGameState() == GameManager.GameManager.GameState.Idle &&
			       TurnManager.Instance.GetCurrentPhase() == TurnManager.TurnPhase.Player && snapshot != null;
		}

		public void MakeSnapshot()
		{
			if (CardInventory.Instance == null) return;
			var randomState = Random.state;

			var deck = CardInventory.Instance.GetCardsInZone(CardInventory.CardZone.Deck).Select(c => c.TypeId).ToList();
			var discard = CardInventory.Instance.GetCardsInZone(CardInventory.CardZone.Discard).Select(c => c.TypeId).ToList();
			var hand = CardInventory.Instance.GetCardsInZone(CardInventory.CardZone.Hand).Select(c => c.TypeId).ToList();
			var consumed = CardInventory.Instance.GetCardsInZone(CardInventory.CardZone.Consumed).Select(c => c.TypeId).ToList();
			var temp = CardInventory.Instance.GetCardsInZone(CardInventory.CardZone.Temporary).Select(c => c.TypeId).ToList();

			var propEntries = new List<PropEntry>();
			if (GridObjectManager.Instance != null)
			{
				var props = GridObjectManager.Instance.GetObjectsOfType<PropBase>();
				foreach (var p in props)
				{
					var grid = p.GetBehaviorComponent<GridObjectComponent>();
					if (grid == null) continue;
					propEntries.Add(new PropEntry { typeId = p.TypeId, position = grid.GetGridPosition() });
				}
			}

			var main = GameObject.FindGameObjectWithTag("MainCharacter");
			int hpMax = 0, hpCur = 0, armor = 0, atk = 0;
			Vector2Int pos = default;
			Direction dir = Direction.Right;
			if (main != null)
			{
				var container = main.GetComponent<BehaviorComponentContainer>();
				var hp = container?.GetBehaviorComponent<HitPointValueComponent>();
				var armorComp = container?.GetBehaviorComponent<ArmorValueComponent>();
				var atkComp = container?.GetBehaviorComponent<AttackPowerComponent>();
				var grid = container?.GetBehaviorComponent<GridObjectComponent>();
				var d = container?.GetBehaviorComponent<DirectionComponent>();
				if (hp != null) { hpMax = hp.MaxHitPoint; hpCur = hp.CurrentHitPoint; }
				if (armorComp != null) armor = armorComp.CurrentArmor;
				if (atkComp != null) atk = atkComp.GetAttackPower();
				if (grid != null) pos = grid.GetGridPosition();
				if (d != null) dir = d.GetDirection();
			}

			var costCur = GameManager.CostManager.Instance ? GameManager.CostManager.Instance.CurrentCost : 0;
			var costMax = GameManager.CostManager.Instance ? GameManager.CostManager.Instance.MaxCost : 0;

			var equipAll = new List<EquipmentTypeId>();
			var unref = new Dictionary<EquipmentTypeId, int>();
			var refres = new Dictionary<EquipmentTypeId, int>();
			var destr = new Dictionary<EquipmentTypeId, int>();
			if (EquipmentInventory.Instance != null)
			{
				foreach (var e in EquipmentInventory.Instance.Equipments) equipAll.Add(e.TypeId);
				foreach (var kv in EquipmentInventory.Instance.UnrefreshedEquipments) unref[kv.Key] = kv.Value;
				foreach (var kv in EquipmentInventory.Instance.RefreshedEquipments) refres[kv.Key] = kv.Value;
				foreach (var kv in EquipmentInventory.Instance.DestroyedEquipments) destr[kv.Key] = kv.Value;
			}

			snapshot = new Snapshot
			{
				randomState = randomState,
				deck = deck,
				discard = discard,
				hand = hand,
				consumed = consumed,
				temporary = temp,
				props = propEntries,
				player = new PlayerEntry
				{
					hpMax = hpMax,
					hpCur = hpCur,
					armor = armor,
					attack = atk,
					position = pos,
					direction = dir,
					costCur = costCur,
					costMax = costMax
				},
				equipments = new EquipmentEntry { all = equipAll, unrefreshed = unref, refreshed = refres, destroyed = destr }
			};
		}

		public void RestoreSnapshot()
		{
			if (snapshot == null) return;
			if (GameManager.GameManager.Instance != null && GameManager.GameManager.Instance.GetGameState() != GameManager.GameManager.GameState.Idle) return;

			var prevRandom = Random.state;
			try
			{
				// 使用非静默方式以触发UI更新

				Random.state = snapshot.randomState;

				if (CardInventory.Instance != null)
				{
					CardInventory.Instance.RestoreFromSnapshot(
						snapshot.deck,
						snapshot.discard,
						snapshot.hand,
						snapshot.consumed,
						snapshot.temporary,
						false);
				}

				if (PropController.Instance != null)
				{
					PropController.Instance.ClearAllProps();
					foreach (var p in snapshot.props)
					{
						PropController.Instance.PlaceProp(p.position, p.typeId, null);
					}
				}

				var main = GameObject.FindGameObjectWithTag("MainCharacter");
				if (main != null)
				{
					var container = main.GetComponent<BehaviorComponentContainer>();
					var hp = container?.GetBehaviorComponent<HitPointValueComponent>();
					var armorComp = container?.GetBehaviorComponent<ArmorValueComponent>();
					var atkComp = container?.GetBehaviorComponent<AttackPowerComponent>();
					var grid = container?.GetBehaviorComponent<GridObjectComponent>();
					var d = container?.GetBehaviorComponent<DirectionComponent>();
					if (hp != null) hp.SetHitPoint(snapshot.player.hpMax, snapshot.player.hpCur);
					if (armorComp != null) armorComp.ClearArmor();
					if (snapshot.player.armor > 0 && armorComp != null) armorComp.AddArmor(snapshot.player.armor);
					if (atkComp != null) atkComp.SetAttackPower(snapshot.player.attack);
					if (grid != null) grid.MoveTo(snapshot.player.position);
					if (d != null) d.SetDirection(snapshot.player.direction);
				}

				if (GameManager.CostManager.Instance != null)
					GameManager.CostManager.Instance.SetCost(snapshot.player.costCur, snapshot.player.costMax, true);

				if (EquipmentInventory.Instance != null)
				{
					EquipmentInventory.Instance.ClearInventory();
					foreach (var typeId in snapshot.equipments.all)
						EquipmentInventory.Instance.AddEquipment(typeId, null);
					foreach (var kv in snapshot.equipments.refreshed)
					{
						var typeId = kv.Key; var count = kv.Value;
						EquipmentInventory.Instance.MoveRefreshedToUnrefreshed(typeId, -EquipmentInventory.Instance.GetRefreshedCount(typeId));
						EquipmentInventory.Instance.SetRefreshedCount(typeId, count);
					}
					foreach (var kv in snapshot.equipments.destroyed)
					{
						var typeId = kv.Key; var target = kv.Value; var cur = EquipmentInventory.Instance.GetDestroyedCount(typeId);
						while (cur < target) { EquipmentInventory.Instance.MarkAsDestroyed(typeId); cur++; }
					}
				}
			}
			finally
			{
				// 保持非静默，不需要还原标志
				Random.state = prevRandom;
			}
		}

		#region DTO
		private class Snapshot
		{
			public Random.State randomState;
			public List<CardTypeId> deck;
			public List<CardTypeId> discard;
			public List<CardTypeId> hand;
			public List<CardTypeId> consumed;
			public List<CardTypeId> temporary;
			public List<PropEntry> props;
			public PlayerEntry player;
			public EquipmentEntry equipments;
		}

		private struct PropEntry
		{
			public PropTypeId typeId;
			public Vector2Int position;
		}

		private struct PlayerEntry
		{
			public int hpMax;
			public int hpCur;
			public int armor;
			public int attack;
			public Vector2Int position;
			public Direction direction;
			public int costCur;
			public int costMax;
		}

		private class EquipmentEntry
		{
			public List<EquipmentTypeId> all;
			public Dictionary<EquipmentTypeId, int> unrefreshed;
			public Dictionary<EquipmentTypeId, int> refreshed;
			public Dictionary<EquipmentTypeId, int> destroyed;
		}
		#endregion
	}
}

