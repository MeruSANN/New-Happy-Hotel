using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.Registry;
using HappyHotel.Intent.Templates;
using Cysharp.Threading.Tasks;
using HappyHotel.Intent.Components.Parts;

namespace HappyHotel.Intent
{
	// 意图基类
	[AutoInitEntityComponent(typeof(ExecutionCompletionAwaiterComponent))]
	public abstract class IntentBase : EntityComponentContainer, ITypeIdSettable<IntentTypeId>
	{
		protected BehaviorComponentContainer owner;
		protected IntentTemplate template;

		public IntentTypeId TypeId { get; private set; }

		// 公共属性，用于访问模板
		public IntentTemplate Template => template;

		// 公共属性：行为宿主（敌人/单位）
		public BehaviorComponentContainer Owner => owner;

		public void SetOwner(BehaviorComponentContainer container)
		{
			owner = container;
		}

		public void SetTypeId(IntentTypeId typeId)
		{
			TypeId = typeId;
		}

		// 设置模板
		public virtual void SetTemplate(IntentTemplate newTemplate)
		{
			template = newTemplate;
			OnTemplateSet();
		}

		// 模板设置完成时调用（子类重写以读取并缓存字段）
		protected virtual void OnTemplateSet()
		{
		}

		// 获取显示文本（子类重写以返回具体的显示字符串）
		public virtual string GetDisplayValue()
		{
			return "";
		}

		// 异步执行接口（使用事件触发组件执行）
		public virtual async UniTask ExecuteAsync()
		{
			var executeEvent = new EntityComponentEvent("Execute", this);
			SendEvent(executeEvent);
			// 等待一次执行完成（由发射命中时的ApplyOnTarget触发）
			var awaiter = GetEntityComponent<ExecutionCompletionAwaiterComponent>();
			if (awaiter != null)
			{
				await awaiter.WaitForCompletionAsync();
			}
		}
	}
}


