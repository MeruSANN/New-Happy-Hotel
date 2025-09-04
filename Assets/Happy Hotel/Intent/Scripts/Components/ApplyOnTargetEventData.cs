using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing.Components;

namespace HappyHotel.Intent.Components
{
	// 统筹发射物命中后的事件数据
	public class ApplyOnTargetEventData
	{
		public BehaviorComponentContainer Target;
		public HitPointValueComponent TargetHp;
		public int HitIndex;
		public bool IsLastHitOfAction;
	}
}


