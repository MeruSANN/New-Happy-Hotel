using Sirenix.OdinInspector;
using UnityEngine;

namespace HappyHotel.Action.Templates
{
    [CreateAssetMenu(fileName = "New Action Template", menuName = "Happy Hotel/Actions/Action Template")]
    public class ActionTemplate : ScriptableObject
    {
        [PreviewField] public Sprite actionSprite;

        [TextArea(2, 4)] public string description;

        [Header("数值颜色设置")] [Tooltip("是否使用自定义颜色（如果为false，则使用默认颜色）")]
        public bool useCustomColors;

        [ShowIf("useCustomColors")] [Tooltip("第一个数值的颜色")]
        public Color primaryValueColor = Color.white;

        [ShowIf("useCustomColors")] [Tooltip("第二个数值的颜色")]
        public Color secondaryValueColor = Color.white;
    }
}