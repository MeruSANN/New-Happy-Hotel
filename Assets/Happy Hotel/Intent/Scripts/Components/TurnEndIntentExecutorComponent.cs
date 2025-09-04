using System;
using System.Collections.Generic;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Intent.Settings;
using Cysharp.Threading.Tasks;

namespace HappyHotel.Intent.Components
{
	// 敌人用：维护意图序列并提供执行接口
	public class TurnEndIntentExecutorComponent : BehaviorComponentBase
	{
		public struct IntentPlan
		{
			public IntentTypeId TypeId;
			public IIntentSetting Setting;
		}

		private readonly List<IntentPlan> intentSequence = new();
		private int currentIndex = 0;
		private bool loop = false;
		private bool isActive = true;

		private IntentBase cachedCurrentIntent;

		public event Action<IntentBase> onCurrentIntentChanged;

		public IntentBase GetCachedCurrentIntent()
		{
			if (cachedCurrentIntent == null) BuildAndCacheCurrentIntent();
			return cachedCurrentIntent;
		}

		private void BuildAndCacheCurrentIntent()
		{
			if (intentSequence.Count == 0 || currentIndex < 0 || currentIndex >= intentSequence.Count)
			{
				if (cachedCurrentIntent != null)
				{
					cachedCurrentIntent.Dispose();
					cachedCurrentIntent = null;
				}
				onCurrentIntentChanged?.Invoke(cachedCurrentIntent);
				return;
			}

			var plan = intentSequence[currentIndex];
			var newIntent = IntentManager.Instance.Create(plan.TypeId, plan.Setting);
			if (newIntent != null)
			{
				newIntent.SetOwner(host);
			}

			if (!ReferenceEquals(cachedCurrentIntent, newIntent))
			{
				cachedCurrentIntent?.Dispose();
				cachedCurrentIntent = newIntent;
				onCurrentIntentChanged?.Invoke(cachedCurrentIntent);
			}
		}

		// 由协调器调用：执行当前意图（异步），执行后推进索引
		public async UniTask ExecuteCurrentIntentAsync()
		{
			if (!isActive || !IsEnabled || host == null) return;
			if (intentSequence.Count == 0) return;

			if (cachedCurrentIntent == null) BuildAndCacheCurrentIntent();
			if (cachedCurrentIntent != null) await cachedCurrentIntent.ExecuteAsync();

			if (currentIndex < intentSequence.Count - 1)
				currentIndex++;
			else if (loop)
				currentIndex = 0;

			BuildAndCacheCurrentIntent();
		}

		// 获取当前意图
		public IntentBase GetCurrentIntent()
		{
			// 兼容旧接口：返回一个新创建的实例（避免影响现有调用）
			if (intentSequence.Count == 0 || currentIndex >= intentSequence.Count) return null;
			var plan = intentSequence[currentIndex];
			var intent = IntentManager.Instance.Create(plan.TypeId, plan.Setting);
			if (intent != null) intent.SetOwner(host);
			return intent;
		}

		public void SetSequence(IEnumerable<IntentPlan> sequence)
		{
			intentSequence.Clear();
			if (sequence != null) intentSequence.AddRange(sequence);
			currentIndex = 0;
			BuildAndCacheCurrentIntent();
		}

		public void Append(IntentPlan plan)
		{
			intentSequence.Add(plan);
			if (intentSequence.Count == 1) // 之前为空，立即构建
			{
				currentIndex = 0;
				BuildAndCacheCurrentIntent();
			}
		}

		public void Clear()
		{
			intentSequence.Clear();
			currentIndex = 0;
			BuildAndCacheCurrentIntent();
		}

		public void SetLoop(bool shouldLoop)
		{
			loop = shouldLoop;
		}

		public void ResetSequence()
		{
			currentIndex = 0;
			BuildAndCacheCurrentIntent();
		}

		public void SetActive(bool active)
		{
			isActive = active;
		}

		public int GetCurrentIndex()
		{
			return currentIndex;
		}

		public IReadOnlyList<IntentPlan> GetSequence()
		{
			return intentSequence;
		}
	}
}


