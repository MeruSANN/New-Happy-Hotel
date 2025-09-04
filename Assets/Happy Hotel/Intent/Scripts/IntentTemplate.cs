using Sirenix.OdinInspector;
using UnityEngine;

namespace HappyHotel.Intent.Templates
{
	// 意图模板：提供描述和可选展示用数据
	[CreateAssetMenu(fileName = "New Intent Template", menuName = "Happy Hotel/Intent/Intent Template")]
	public class IntentTemplate : ScriptableObject
	{
		[PreviewField] public Sprite icon;
		[TextArea(2, 4)] public string description;
	}
}


