using Sirenix.OdinInspector;
using UnityEngine;

namespace HappyHotel.Character.Templates
{
    [CreateAssetMenu(fileName = "New Character Template", menuName = "Happy Hotel/Characters/Character Template")]
    public class CharacterTemplate : SerializedScriptableObject
    {
        [PreviewField] public Sprite characterSprite;

        public int baseHealth = 5;
        
        [Header("攻击设置")]
        [SerializeField] [Min(0)] [Tooltip("角色的基础攻击力")]
        public int baseAttackPower = 1;
    }
}