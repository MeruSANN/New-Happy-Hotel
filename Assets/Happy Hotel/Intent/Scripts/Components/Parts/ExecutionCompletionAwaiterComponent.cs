using Cysharp.Threading.Tasks;
using HappyHotel.Core.EntityComponent;

namespace HappyHotel.Intent.Components.Parts
{
	// 等待一次Execute流程完成的组件：
	// 1) 在收到 Execute 事件时进入等待状态；
	// 2) 在收到 ApplyOnTarget（或其他结束标志）后完成等待；
	public class ExecutionCompletionAwaiterComponent : EntityComponentBase, IEventListener
	{
		private UniTaskCompletionSource completionTcs;

		public void OnEvent(EntityComponentEvent evt)
		{
			if (evt.EventName == "Execute")
			{
				// 新的一次执行开始，重置等待源
				// 仅当宿主存在会“在命中时完成”的组件时，才进入等待。
				var hasCompleter = GetHost()?.GetEntityComponents<IEventListener>()?.Exists(c => c is ICompletesOnApplyOnTarget) == true;
				completionTcs = hasCompleter ? new UniTaskCompletionSource() : null;
			}
			else if (evt.EventName == "ApplyOnTarget")
			{
				// 命中后认为本次执行完成
				completionTcs?.TrySetResult();
			}
		}

		public UniTask WaitForCompletionAsync()
		{
			return completionTcs != null ? completionTcs.Task : UniTask.CompletedTask;
		}
	}
}


