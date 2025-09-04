using HappyHotel.Buff;
using HappyHotel.Buff.Components;
using HappyHotel.Buff.Settings;
using HappyHotel.Core.EntityComponent;

namespace HappyHotel.Intent.Components.Parts
{
	// 纯效果：对主角添加一次Buff（不播放弹射物）
	public class AddBuffOnMainCharacterEffectEntityComponent : EntityComponentBase, IEventListener
	{
		private string buffTypeString;
		private IBuffSetting buffSetting;

		public void SetBuffToApply(string buffType, IBuffSetting setting)
		{
			buffTypeString = buffType;
			buffSetting = setting;
		}

		public void OnEvent(EntityComponentEvent evt)
		{
			if (evt.EventName != "ApplyOnTarget") return;
			var data = evt.Data as ApplyOnTargetEventData;
			if (data == null || data.Target == null) return;

			if (string.IsNullOrEmpty(buffTypeString)) return;
			var buff = BuffManager.Instance.Create(buffTypeString, buffSetting);
			if (buff == null) return;

			var targetBuffContainer = data.Target.GetBehaviorComponent<BuffContainer>() ?? data.Target.AddBehaviorComponent<BuffContainer>();
			targetBuffContainer?.AddBuff(buff);
		}
	}
}


