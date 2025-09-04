using Sirenix.OdinInspector;
using UnityEngine;

namespace HappyHotel.Buff.Templates
{
    // Buff模板
    [CreateAssetMenu(fileName = "New Buff Template", menuName = "Happy Hotel/Buff/Buff Template")]
    public class BuffTemplate : ScriptableObject
    {
        public string buffName;

        [PreviewField] public Sprite icon;

        [TextArea(2, 4)] public string description;
    }
}